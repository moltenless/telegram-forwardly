import logging
from typing import Dict, Any, List
from telethon import TelegramClient, events
from telethon.errors import SessionPasswordNeededError, PhoneCodeInvalidError, PasswordHashInvalidError
from telethon.sessions import StringSession
from app.models import BotUser, UserClient
from app.services.authentication_manager import connect_existing_client
from app.services.telegram_api_service import TelegramApiService
from app.services.message_handler import MessageHandler
import app.services.authentication_manager
from app.utils import log_error, log_info

logger = logging.getLogger(__name__)

class ClientManager:

    def __init__(self):
        self.clients: Dict[int, UserClient] = {}
        self.telegram_api = TelegramApiService()
        # self.message_handler = MessageHandler()
        # self._auth_sessions = {}

    async def initialize_from_database(self):
        log_info("Initializing clients from database...")

        await self._disconnect_and_clear_all_clients()

        users = await self.telegram_api.get_all_users()

        await self._launch_clients_from_users(users)

        log_info(f"Connected {len(self.clients)} clients")

    async def _disconnect_and_clear_all_clients(self):
        for user_id, client in self.clients.items():
            try:
                await client.client.disconnect()
            except Exception as e:
                log_error(f"Failed to disconnect client for user {user_id}", e)
        self.clients.clear()

    async def _launch_clients_from_users(self, users: List[BotUser]):
        try:
            for user in users:
                self.clients[user.telegram_user_id] = UserClient()
                self.clients[user.telegram_user_id].user = user
                await self._connect_client(user)
                await self._setup_message_handler(self.clients[user.telegram_user_id])
        except Exception as e:
            log_error(f"Failed to launch clients from users", e)

    async def _connect_client(self, user: BotUser):
        try:
            client = await connect_existing_client(user.session_string, user.api_id, user.api_hash)
            if not client:
                self.clients[
                    user.telegram_user_id].last_error = "Couldn't connect to telegram client with this api id, hash and session string"
                log_error(f"Can't connect to Telegram client for {user.telegram_user_id}")
                await client.disconnect()
                return

            self.clients[user.telegram_user_id].client = client
            self.clients[user.telegram_user_id].is_connected = True
            log_info(f"Connected to Telegram client for {user.telegram_user_id}")
        except Exception as e:
            log_error(f"Failed to open Telegram Client connection or similar for user {user.telegram_user_id}", e)

    async def _setup_message_handler(self, user_client: UserClient):
        # try:
        #     monitored_chat_ids = [chat.telegram_chat_id for chat in user_client.user.chats]
        #
        #     if not monitored_chat_ids and not user_client.user.all_chats_filtering_enabled:
        #         return
        #
        #     # Create event handler
        #     @user_client.client.on(events.NewMessage)
        #     async def handle_new_message(event):
        #         await self.message_handler.handle_message(event, user_client)
        #
        #     log_info(f"Set up message handler for user {user_client.user.telegram_user_id}")
        #
        # except Exception as e:
        #     log_error(f"Failed to setup message handler for user {user_client.user.telegram_user_id}", e)
        log_info('Here I must subscribe client to handle new messages')

    async def start_authentication(self, user_id: int, phone: str, api_id: str, api_hash: str) -> Dict[
        str, Any]:
        """Start authentication process for a user"""
        try:
            # Create temporary client for authentication
            client = TelegramClient(StringSession(), int(api_id), api_hash)
            await client.connect()

            # Send code request
            await client.send_code_request(phone)

            # Store temp session for verification
            self._auth_sessions[user_id] = {
                'client': client,
                'phone': phone,
                'api_id': api_id,
                'api_hash': api_hash
            }

            log_info(f"Started authentication for user {user_id}")
            return {'success': True, 'message': 'Code sent to phone'}

        except Exception as e:
            log_error(f"Failed to start authentication for user {user_id}", e)
            return {'success': False, 'error': str(e)}

    async def verify_code(self, user_id: int, code: str) -> Dict[str, Any]:
        try:
            if user_id not in self._auth_sessions:
                return {'success': False, 'error': 'Authentication session not found'}

            session_data = self._auth_sessions[user_id]
            client = session_data['client']

            try:
                await client.sign_in(session_data['phone'], code)

                # Get session string
                session_string = client.session.save()

                # Update session in bot API
                await self.telegram_api.update_user_session(user_id, session_string)

                # Clean up temp session
                await client.disconnect()
                del self._auth_sessions[user_id]

                log_info(f"Successfully verified code for user {user_id}")
                return {'success': True, 'message': 'Authentication successful'}

            except SessionPasswordNeededError:
                log_info(f"2FA password required for user {user_id}")
                return {'success': False, 'requires_password': True, 'message': '2FA password required'}

            except PhoneCodeInvalidError:
                return {'success': False, 'error': 'Invalid verification code'}

        except Exception as e:
            log_error(f"Failed to verify code for user {user_id}", e)
            return {'success': False, 'error': str(e)}

    async def verify_password(self, user_id: int, password: str) -> Dict[str, Any]:
        """Verify 2FA password"""
        try:
            if user_id not in self._auth_sessions:
                return {'success': False, 'error': 'Authentication session not found'}

            session_data = self._auth_sessions[user_id]
            client = session_data['client']

            try:
                await client.sign_in(password=password)

                # Get session string
                session_string = client.session.save()

                # Update session in bot API
                await self.telegram_api.update_user_session(user_id, session_string)

                # Clean up temp session
                await client.disconnect()
                del self._auth_sessions[user_id]

                log_info(f"Successfully verified password for user {user_id}")
                return {'success': True, 'message': 'Authentication successful'}

            except PasswordHashInvalidError:
                return {'success': False, 'error': 'Invalid password'}

        except Exception as e:
            log_error(f"Failed to verify password for user {user_id}", e)
            return {'success': False, 'error': str(e)}

    async def update_user(self, user_data: BotUser) -> Dict[str, Any]:
        """Update user configuration and reconnect if necessary"""
        try:
            user_id = user_data.telegram_user_id

            # If user already exists, disconnect old client
            if user_id in self.clients:
                await self._disconnect_client(user_id)

            # If user is authenticated, create new connection
            if user_data.is_authenticated and user_data.session_string:
                success = await self._create_and_connect_client(user_data)
                if success:
                    return {'success': True, 'message': 'User updated and connected'}
                else:
                    return {'success': False, 'error': 'Failed to connect client'}

            return {'success': True, 'message': 'User updated'}

        except Exception as e:
            log_error(f"Failed to update user {user_data.telegram_user_id}", e)
            return {'success': False, 'error': str(e)}

    async def remove_user(self, user_id: int) -> Dict[str, Any]:
        """Remove user and disconnect client"""
        try:
            await self._disconnect_client(user_id)
            log_info(f"Removed user {user_id}")
            return {'success': True, 'message': 'User removed'}

        except Exception as e:
            log_error(f"Failed to remove user {user_id}", e)
            return {'success': False, 'error': str(e)}

    async def _disconnect_client(self, user_id: int):
        """Disconnect client for user"""
        if user_id in self.clients:
            user_client = self.clients[user_id]
            if user_client.client and user_client.is_connected:
                await user_client.client.disconnect()
            del self.clients[user_id]

            # Report disconnection to bot API
            await self.telegram_api.report_user_status(user_id, False)

    def get_all_users_status(self) -> Dict[str, Any]:
        """Get status of all connected users"""
        return {
            'connected_users': len(self.clients),
            'users': [
                {
                    'user_id': user_id,
                    'is_connected': client.is_connected,
                    'last_error': client.last_error
                }
                for user_id, client in self.clients.items()
            ]
        }

    # async def start_message_handling(self):
    #     """Start message handling for all connected clients"""
    #     log_info("Starting message handling for all clients...")
    #
    #     # All clients are already set up with message handlers
    #     # This method can be used for additional setup if needed
    #     pass

