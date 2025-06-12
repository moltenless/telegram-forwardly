import asyncio
import logging
from typing import Optional, List
from telethon import events, TelegramClient
from telethon.errors import FloodWaitError
from telethon.tl.functions.channels import GetForumTopicsRequest, CreateForumTopicRequest
from telethon.tl.types import Message
from app.models import UserClient, GroupingMode
from app.services.telegram_api_service import TelegramApiService
from app.utils import log_error, log_info

logger = logging.getLogger(__name__)


class MessageHandler:
    def __init__(self):
        self.telegram_api = TelegramApiService()

    async def handle_message(self, event: events.NewMessage.Event, user_client: UserClient):
        try:
            # me = await user_client.client.get_me()
            # await user_client.client.send_message(entity=me, message=f"somebody wrote: {event.message.text}")
            # return None
            source_chat = await event.get_chat()

            if source_chat.id == user_client.user.forum_supergroup_id:
                return
            if user_client.user.forwardly_enabled is None or user_client.user.forwardly_enabled is False:
                logger.warning(f"text: '{event.message.text}' has been filtered on 'forwardly enabled' stage")
                return
            if not user_client.user.forum_supergroup_id:
                logger.warning(f"text: '{event.message.text}' has been filtered on 'forum supergroup id' stage")
                return
            if ((not user_client.user.chats or len(user_client.user.chats) == 0)
                    and not user_client.user.all_chats_filtering_enabled):
                logger.warning(f"text: '{event.message.text}' has been filtered on 'chats' stage")
                return
            if not user_client.user.keywords or len(user_client.user.keywords) == 0:
                logger.warning(f"text: '{event.message.text}' has been filtered on 'keywords' stage")
                return
            if not self._is_chat_monitored(source_chat.id, user_client):
                logger.warning(f"text: '{event.message.text}' has been filtered on 'chats is monitored' stage")
                return
            if not self._message_contains_keywords(event.message, user_client.user.keywords):
                logger.warning(f"text: '{event.message.text}' has been filtered on 'message contains keywords' stage")
                return
            await self._forward_message(event.message, source_chat.id, user_client)

        except Exception as e:
            logger.error(f"Error handling message for user {user_client.user.telegram_user_id}: {e}")

    def _is_chat_monitored(self, chat_id: int, user_client: UserClient) -> bool:
        if user_client.user.all_chats_filtering_enabled:
            return True
        logger.info(f"inside chat monitored:\n\nchat_id(from event) is '{chat_id}\n'")
        monitored_chat_ids = [chat.telegram_chat_id for chat in user_client.user.chats]
        logger.info(f"meanwhile monitored chats of that user is \n{monitored_chat_ids}\n")
        return chat_id in monitored_chat_ids

    def _message_contains_keywords(self, message: Message, keywords: List) -> bool:
        if not keywords or not message.text:
            return False
        message_text = message.text.lower()
        keyword_values = [kw.value.lower() for kw in keywords]
        return any(keyword in message_text for keyword in keyword_values)

    async def _forward_message(self, message: Message, source_chat_id: int, user_client: UserClient):
        try:
            chat = await user_client.client.get_entity(source_chat_id)
            chat_title = getattr(chat, 'title', f'Chat {source_chat_id}')

            topic_name = self._get_topic_name(message, chat_title, user_client)

            topic_id = await self._get_or_create_topic(
                user_client.client,
                user_client.user.forum_supergroup_id,
                topic_name
            )

            final_text = f'{message.from_id.user_id} said: {message.text}'

            await user_client.client.send_message(
                entity=user_client.user.forum_supergroup_id,
                message=final_text,
                reply_to=topic_id
            )

        except Exception as e:
            log_error(f"Failed to forward message for user {user_client.user.telegram_user_id}", e)

    def _get_topic_name(self, message: Message, chat_title: str, user_client: UserClient) -> str:
        if user_client.user.topic_grouping == GroupingMode.BY_CHAT:
            return chat_title
        else:
            if message.text:
                message_text = message.text.lower()
                for keyword in user_client.user.keywords:
                    if keyword.value.lower() in message_text:
                        return keyword.value.capitalize()
            return "General"

    async def _get_or_create_topic(self, client: TelegramClient, forum_id: int, topic_name: str) -> Optional[int]:
        try:
            topic_name = self._sanitize_topic_name(topic_name)

            existing_topic_id = await self._find_topic_by_title(client, forum_id, topic_name)
            if existing_topic_id:
                return existing_topic_id

            return await self._create_topic(client, forum_id, topic_name)

        except Exception as e:
            logger.error(f"Error in get_or_create_topic for forum {forum_id}, topic '{topic_name}': {e}")
            return None

    def _sanitize_topic_name(self, topic_name: str) -> str:
        topic_name = topic_name.strip()
        if len(topic_name) >= 100:
            topic_name = topic_name[:96] + "..."
        if len(topic_name) == 0:
            topic_name = "General"

        return topic_name

    async def _find_topic_by_title(self, client: TelegramClient, forum_id: int, topic_title: str) -> Optional[int]:
        try:
            for attempt in range(3):
                try:
                    result = await client(GetForumTopicsRequest(
                        channel=forum_id,
                        offset_date=None,
                        offset_id=0,
                        offset_topic=0,
                        limit=150
                    ))

                    if hasattr(result, 'topics'):
                        for topic in result.topics:
                            if hasattr(topic, 'title') and hasattr(topic, 'id'):
                                if topic.title.lower() == topic_title.lower():
                                    logger.info(f"Found existing topic '{topic_title}' with ID {topic.id}")
                                    return topic.id

                    return None

                except FloodWaitError as e:
                    if attempt < 2:
                        logger.warning(f"FloodWait when searching topics, waiting {e.seconds} seconds")
                        await asyncio.sleep(e.seconds)
                        continue
                    else:
                        logger.error(f"FloodWait exceeded max retries when searching topics")
                        return None

                except Exception as e:
                    logger.error(f"Error searching topics (attempt {attempt + 1}): {e}")
                    if attempt < 2:
                        await asyncio.sleep(2 ** attempt)
                        continue
                    else:
                        return None

            return None

        except Exception as e:
            logger.error(f"Failed to search for topic '{topic_title}' in forum {forum_id}: {e}")
            return None

    async def _create_topic(self, client: TelegramClient, forum_id: int, topic_name: str) -> Optional[int]:
        try:
            for attempt in range(3):
                try:
                    result = await client(CreateForumTopicRequest(
                        channel=forum_id,
                        title=topic_name,
                        icon_color=None,
                        icon_emoji_id=None,
                        send_as=None
                    ))

                    if hasattr(result, 'updates'):
                        for update in result.updates:
                            if hasattr(update, 'message') and hasattr(update.message, 'id'):
                                topic_id = update.message.id
                                logger.info(f"Created topic '{topic_name}' with ID {topic_id} in forum {forum_id}")
                                return topic_id

                    logger.error(f"Unexpected result format when creating topic '{topic_name}'")
                    return None

                except FloodWaitError as e:
                    if attempt < 2:
                        logger.warning(f"FloodWait when creating topic, waiting {e.seconds} seconds")
                        await asyncio.sleep(e.seconds)
                        continue
                    else:
                        logger.error(f"FloodWait exceeded max retries when creating topic")
                        return None

                except Exception as e:
                    logger.error(f"Error creating topic '{topic_name}' (attempt {attempt + 1}): {e}")
                    if attempt < 2:
                        await asyncio.sleep(2 ** attempt)
                        continue
                    else:
                        return None

            return None

        except Exception as e:
            logger.error(f"Failed to create topic '{topic_name}' in forum {forum_id}: {e}")
            return None