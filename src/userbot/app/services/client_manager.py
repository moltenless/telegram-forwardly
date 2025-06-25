import logging
from typing import Dict, Any, List

from telethon import TelegramClient, events
from telethon.errors import UserNotParticipantError
from telethon.sessions import StringSession
from telethon.tl.functions.channels import GetParticipantRequest
from telethon.tl.types import ChannelParticipantCreator, User, ChatForbidden, ChannelForbidden

from app.models import BotUser, UserClient, GroupingMode, Chat, Keyword
from app.services.message_handler import MessageHandler
from app.services.telegram_api_service import TelegramApiService

logger = logging.getLogger(__name__)

class ClientManager:

    def __init__(self):
        self.clients: Dict[int, UserClient] = {}
        self.telegram_api = TelegramApiService()
        self.message_handler = MessageHandler(self.remove_special_chars)

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
                logger.info(f"Disconnected {user_id} client")
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
                    logger.error(f"!!!!!!!!!!!!! Client {user.telegram_user_id} "
                                 f"has been launched but NEVERTHELES it's still UNauthorized")
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
        try:
            @user_client.client.on(events.NewMessage)
            async def handle_new_message(event):
                await self.message_handler.handle_message(event, user_client)

        except Exception as e:
            logger.error(f"Failed to setup message handler for user {user_client.user.telegram_user_id}", e)





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
            elif grouping == 'General':
                self.clients[user_id].user.topic_grouping = GroupingMode.GENERAL
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
                        'Title': entity.title[:99]
                    })

                    user.chats.append(Chat(telegram_user_id=user_id,
                                           telegram_chat_id=chat_id,
                                           title=entity.title[:99],
                                           id = -1))
                except: continue

            return {'Success': True, 'Chats': chat_infos}
        except Exception as e:
            logger.error(f'Error adding chats to user: {e}')
            return {'Success': False, 'ErrorMessage': f'Error adding chats to user: {e}'}

    async def remove_chats(self, user_id, removed_chats):
        try:
            user = self.clients[user_id].user

            for removed_chat_id in removed_chats:
                for user_chat in user.chats:
                    if user_chat.telegram_chat_id == removed_chat_id:
                        user.chats.remove(user_chat)
                        break

            return {'Success': True}
        except Exception as e:
            logger.error(f'Error removing chats for user: {e}')
            return {'Success': False, 'ErrorMessage': f'Error removing chats for user: {e}'}


    async def add_keywords(self, user_id, keywords):
        try:
            user = self.clients[user_id].user

            for keyword in keywords:
                try:
                    if keyword in user.keywords: continue
                    user.keywords.append(
                        Keyword(telegram_user_id=user_id,
                                value=keyword,
                                id = -1))
                except: continue

            return {'Success': True}
        except Exception as e:
            logger.error(f'Error adding keywords to user: {e}')
            return {'Success': False, 'ErrorMessage': f'Error adding keywords to user: {e}'}

    async def remove_keywords(self, user_id, keywords_without_special_characters):
        try:
            user = self.clients[user_id].user

            for keyword in keywords_without_special_characters:
                for user_keyword in user.keywords:
                    if (user_keyword.value == keyword
                            or self.remove_special_chars(user_keyword.value)
                            == keywords_without_special_characters):
                        user.keywords.remove(user_keyword)
                        break

            return {'Success': True}
        except Exception as e:
            logger.error(f'Error removing keywords for user: {e}')
            return {'Success': False, 'ErrorMessage': f'Error removing removing for user: {e}'}

    def remove_special_chars(self, input_str: str) -> str:
        special_chars = {'\\', '_', '*', '[', ']', '(', ')', '~', '`', '>', '<',
                         '#', '+', '-', '=', '|', '{', '}', '.',
                         '!'}
        # special_chars = {'_', '*', '[', ']', '(', ')', '`', '|'}
        return ''.join(c for c in input_str if c not in special_chars)

    async def toggle_forwarding(self, user_id, value):
        try:
            self.clients[user_id].user.forwardly_enabled = value
            return {'Success': True}
        except Exception as e:
            logger.error(f'Failed to toggle forwarding: {e}')
            return {'Success': False, 'ErrorMessage': f'Failed to toggle forwarding: {e}'}

