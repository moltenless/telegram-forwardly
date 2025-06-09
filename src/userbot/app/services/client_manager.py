import logging
from typing import Dict, Any, List

from telethon import TelegramClient
from telethon.errors import UserNotParticipantError
from telethon.sessions import StringSession
from telethon.tl.functions.channels import GetParticipantRequest
from telethon.tl.types import ChannelParticipantCreator, User, ChatForbidden, ChannelForbidden

from app.models import BotUser, UserClient, GroupingMode, Chat
from app.services.telegram_api_service import TelegramApiService
from app.utils import log_error, log_info

logger = logging.getLogger(__name__)

class ClientManager:

    def __init__(self):
        self.clients: Dict[int, UserClient] = {}
        self.telegram_api = TelegramApiService()
        # self.message_handler = MessageHandler()

    async def launch_clients_from_database(self):
        await self._disconnect_and_clear_all_clients()
        users = await self.telegram_api.get_all_users()
        await self._launch_clients_from_users(users)
        logger.info(f"Launched {len(self.clients)} clients")

    async def launch_client(self, user: BotUser)-> bool:
        try:
            self.clients[user.telegram_user_id] = UserClient()
            self.clients[user.telegram_user_id].user = user
            connected = await self._connect_client(user)
            if connected:
                self.clients[user.telegram_user_id].user.is_authenticated = True
            else:
                return False

            await self._setup_message_handler(self.clients[user.telegram_user_id])

            if await self.clients[user.telegram_user_id].client.is_user_authorized():
                logger.info(f"Connected and launched {user.telegram_user_id} client")
            else:
                logger.info(f"Client {user.telegram_user_id} has been launched but NEVERTHELES it's still UNauthorized")
                return False
            
            return True
        except Exception as e:
            logger.warning(f'Exception while launching client {user.telegram_user_id}. {e}')
            return False

    async def _disconnect_and_clear_all_clients(self):
        for user_id, client in self.clients.items():
            try:
                await client.client.disconnect()
            except Exception as e:
                logger.error(f"Failed to disconnect client for user {user_id}: {e}")
        self.clients.clear()

    async def _launch_clients_from_users(self, users: List[BotUser]):
        try:
            for user in users:
                self.clients[user.telegram_user_id] = UserClient()
                self.clients[user.telegram_user_id].user = user
                await self._connect_client(user)
                await self._setup_message_handler(self.clients[user.telegram_user_id])
                if await self.clients[user.telegram_user_id].client.is_user_authorized():
                    logger.info(f"Connected and launched {user.telegram_user_id} client")
                else:
                    logger.error(
                        f"!!!!!!!!!!!!! Client {user.telegram_user_id} has been launched but NEVERTHELES it's still UNauthorized")
        except Exception as e:
            logger.error(f"Failed to launch clients from users: {e}")

    async def _connect_client(self, user: BotUser) -> bool:
        try:
            client = TelegramClient(StringSession(user.session_string), user.api_id, user.api_hash)
            await client.connect()

            if not client:
                self.clients[
                    user.telegram_user_id].last_error = "Couldn't connect to telegram client with this api id, hash and session string"
                logger.error(f"Can't connect to Telegram client for {user.telegram_user_id}")
                await client.disconnect()
                return False

            if not await client.is_user_authorized():
                logger.error(f"client.is_user_authorized(): is NOT")
                return False
            me = await client.get_me()
            if me.id != user.telegram_user_id:
                logger.error(f"me.id != user.telegram_user_id:")
                return False

            self.clients[user.telegram_user_id].client = client
            self.clients[user.telegram_user_id].is_connected = True
            return True
        except Exception as e:
            logger.error(f"Failed to open Telegram Client connection or similar for user {user.telegram_user_id}: {e}")
            return False

    async def _setup_message_handler(self, user_client: UserClient):
        # try:
        #     @user_client.client.on(events.NewMessage)
        #     async def handle_new_message(event):
        #         await self.message_handler.handle_message(event, user_client)
        #
        #     log_info(f"Set up message handler for user {user_client.user.telegram_user_id}")
        #
        # except Exception as e:
        #     log_error(f"Failed to setup message handler for user {user_client.user.telegram_user_id}", e)
        logger.info('Here I must subscribe client to handle new messages')

    async def check_and_update_forum(self, user_id, forum_id):
        try:
            client = self.clients[user_id].client
            try:
                entity = await client.get_entity(forum_id)
            except (ValueError, Exception) as e:
                return {"Success": False, "ErrorMessage": f"Chat not found: {e}"}

            if not getattr(entity, "forum", False):
                return {"Success": False, "ErrorMessage": "Forum topics are not enabled on that group"}

            try:
                participant = await client(GetParticipantRequest(channel=forum_id, participant=user_id))
            except UserNotParticipantError:
                return {"Success": False, "ErrorMessage": "You must be a member of this group"}

            if not isinstance(participant.participant, ChannelParticipantCreator):
                return {"Success": False, "ErrorMessage": "You must be the owner of the group"}

            self.clients[user_id].user.forum_supergroup_id = forum_id

            return {"Success": True}

        except Exception as e:
            logger.error(f'Failed to check and update forum id: {e}')
            return {"Success": False, "ErrorMessage": f'Failed to check and update forum id: {e}'}

    async def update_grouping(self, user_id, grouping):
        try:
            if grouping == 'ByKeyword':
                self.clients[user_id].user.topic_grouping = GroupingMode.BY_KEYWORD
            elif grouping == 'ByChat':
                self.clients[user_id].user.topic_grouping = GroupingMode.BY_CHAT
            return {'Success': True}
        except Exception as e:
            logger.error(f'Failed to update topic grouping type: {e}')
            return {'Success': False, 'ErrorMessage': f'Failed to update topic grouping type {e}'}

    async def delete_user(self, user_id):
        try:
            client = self.clients[user_id].client
            await client.disconnect()
            del self.clients[user_id]
            return {'Success': True}
        except Exception as e:
            logger.error(f'Failed to delete account data: {e}')
            return {'Success': False, 'ErrorMessage': f'Failed to delete account data: {e}'}

    async def enable_all_chats(self, user_id, enable_all_chats):
        try:
            self.clients[user_id].user.all_chats_filtering_enabled = enable_all_chats
            return {'Success': True}
        except Exception as e:
            logger.error(f'Failed to enable all chats: {e}')
            return {'Success': False, 'ErrorMessage': f'Failed to enable all chats: {e}'}

    async def get_user_chats(self, user_id):
        try:
            client = self.clients[user_id].client
            dialogs = await client.get_dialogs()
            chats = []
            seen_titles = set()

            for dialog in dialogs:
                entity = dialog.entity

                if isinstance(entity, User):
                    continue

                if isinstance(entity, (ChatForbidden, ChannelForbidden)):
                    continue

                title = entity.title
                if title in seen_titles:
                    continue
                seen_titles.add(title)

                chat = {
                    'Id': entity.id,
                    'Title': title
                }
                chats.append(chat)

            return {
                'Success': True,
                'Chats': chats
            }


        except Exception as e:
            logger.error(f'Failed to retrieve user chats: {e}')
            return {'Success': False, 'ErrorMessage': f'Failed to retrieve user chats: {e}'}

    async def add_chats(self, user_id, chats):
        try:
            user = self.clients[user_id].user
            client = self.clients[user_id].client
            chat_infos = []

            for chat_id in chats:
                try:
                    already_exists = False
                    for chat in user.chats:
                        if chat.telegram_chat_id == chat_id:
                            already_exists = True
                            break
                    if already_exists:
                        continue

                    entity = await client.get_entity(chat_id)

                    chat_infos.append({
                        'Id': chat_id,
                        'Title': entity.title
                    })

                    user.chats.append(Chat(telegram_user_id=user_id,
                                           telegram_chat_id=chat_id,
                                           title=entity.title,
                                           id = -1))
                except: continue

            return {'Success': True, 'Chats': chat_infos}
        except Exception as e:
            logger.error(f'Failed to enable all chats: {e}')
            return {'Success': False, 'ErrorMessage': f'Error adding chats to user: {e}'}





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

