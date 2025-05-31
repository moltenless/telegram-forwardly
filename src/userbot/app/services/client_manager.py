import asyncio
import logging
from typing import Dict, List, Optional, Any
from telethon import TelegramClient, events
from telethon.errors import SessionPasswordNeededError, PhoneCodeInvalidError, PasswordHashInvalidError
from telethon.sessions import StringSession
from app.models import BotUser, UserClient
from app.services.telegram_api_service import TelegramApiService
from app.services.message_handler import MessageHandler
from app.utils import log_error, log_info

logger = logging.getLogger(__name__)

class ClientManager:

    def __init__(self):
        self.clients: Dict[int, UserClient] = {}
        self.telegram_api = TelegramApiService()
        self.message_handler = MessageHandler()
        self._auth_sessions = {}

    async def initialize_from_database(self):
        log_info("Initializing clients from database...")

        users = await self.telegram_api.get_all_users()

        for user in users:
            if user.is_authenticated and user.session_string:
                await self._create_and_connect_client(user)

        log_info(f"Initialized {len(self.clients)} clients")

        async def _create_and_connect_client(self, user: BotUser) -> bool:
            """Create and connect a Telegram client for user"""
            try:
                if not all([user.api_id, user.api_hash, user.session_string]):
                    log_error(f"Missing credentials for user {user.telegram_user_id}")
                    return False

                # Create client with string session
                session = StringSession(user.session_string)
                client = TelegramClient(
                    session,
                    int(user.api_id),
                    user.api_hash
                )

                # Connect client
                await client.connect()

                if not await client.is_user_authorized():
                    log_error(f"User {user.telegram_user_id} session is not authorized")
                    await client.disconnect()
                    return False

                # Store client
                user_client = UserClient(user=user, client=client, is_connected=True)
                self.clients[user.telegram_user_id] = user_client

                # Set up message handler
                await self._setup_message_handler(user_client)

                log_info(f"Connected client for user {user.telegram_user_id}")

                # Report status to bot API
                await self.telegram_api.report_user_status(user.telegram_user_id, True)

                return True

            except Exception as e:
                log_error(f"Failed to connect client for user {user.telegram_user_id}", e)
                await self.telegram_api.report_user_status(user.telegram_user_id, False, str(e))
                return False

        async def _setup_message_handler(self, user_client: UserClient):
            """Set up message handler for user client"""
            try:
                # Get monitored chat IDs
                monitored_chat_ids = [chat.telegram_chat_id for chat in user_client.user.chats]

                if not monitored_chat_ids and not user_client.user.all_chats_filtering_enabled:
                    return

                # Create event handler
                @user_client.client.on(events.NewMessage)
                async def handle_new_message(