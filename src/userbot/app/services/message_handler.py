import asyncio
import logging
from typing import Optional, List

import unicodedata
from telethon import events, TelegramClient
from telethon.errors import FloodWaitError
from telethon.tl.functions.channels import GetForumTopicsRequest, CreateForumTopicRequest
from telethon.tl.types import Message

from app import ClientManager
from app.models import UserClient, GroupingMode
from app.services.telegram_api_service import TelegramApiService

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
            sender = await event.get_sender()

            if sender.id == user_client.user.telegram_user_id:
                return
            ######################## if sender isn't forwardly bot!!!!!!!!!!
            if (source_chat.id == user_client.user.forum_supergroup_id or
                '-100' + str(source_chat.id) == str(user_client.user.forum_supergroup_id)):
                return
            if user_client.user.forwardly_enabled is None or user_client.user.forwardly_enabled is False:
                return
            if not user_client.user.forum_supergroup_id:
                return
            if ((not user_client.user.chats or len(user_client.user.chats) == 0)
                    and not user_client.user.all_chats_filtering_enabled):
                return
            if not user_client.user.keywords or len(user_client.user.keywords) == 0:
                return
            if not self._is_chat_monitored(source_chat.id, user_client):
                return
            detected_kws = self._message_contains_keywords(event.message, user_client.user.keywords)
            if detected_kws is None or len(detected_kws) == 0:
                return

            event_data = {
                'text': event.message.text,
                'detected_kws': detected_kws,
                'date_time' : event.message.date,
                'source_chat_id' : source_chat.id,
                'source_chat_title' : source_chat.title if hasattr(source_chat, 'title')
                    else source_chat.first_name if source_chat.first_name else f'Chat {source_chat.id}',
                'message_id' : event.message.id,
                'user_id' : sender.id,
                'username' : sender.username,
                'first_name' : sender.first_name if hasattr(sender, 'first_name') else ''
            }

            await self._forward_message(event_data, user_client)

        except Exception as e:
            logger.error(f"Error handling message for user {user_client.user.telegram_user_id}: {e}")

    def _is_chat_monitored(self, chat_id: int, user_client: UserClient) -> bool:
        if user_client.user.all_chats_filtering_enabled:
            return True
        monitored_chat_ids = [chat.telegram_chat_id for chat in user_client.user.chats]
        return chat_id in monitored_chat_ids

    def _message_contains_keywords(self, message: Message, keywords: List) -> List[str]:
        if not keywords or not message.text:
            return []
        message_text = message.text.lower()
        keyword_values = [kw.value.lower() for kw in keywords]
        return [keyword for keyword in keyword_values if keyword in message_text]

    async def _forward_message(self, event_data, user_client: UserClient):
        try:
            topic_name = self._get_topic_name(event_data.get('source_chat_title'),
                                              (event_data.get('detected_kws'))[0],
                                              user_client)

            topic_id = await self._get_or_create_topic(
                user_client.client,
                user_client.user.forum_supergroup_id,
                topic_name
            )

            body = f"{ClientManager.remove_special_chars(event_data.get('text'))}"
            footer = (f"\n\nLink to message: https://t.me/c/{event_data.get('source_chat_id')}/{event_data.get('message_id')}\n"
                          f"Detected keywords: {', '.join(event_data.get('detected_kws'))}\n" 
                          f"From chat: {event_data.get('source_chat_title')[:25]}\n"
                          f"Message by: {event_data.get('first_name')} {'@' + event_data.get('username') if event_data.get('username') else ''}\n"
                          f"Time: {event_data.get('date_time').strftime('%H:%M | %d.%m ')}")

            len_delta = len(body+footer) - 4096
            if len_delta <= 0:
                final_text = body + footer
            else:
                final_text = body[:len(body) - len_delta - 3] + '...' + footer

            await self.telegram_api.send_message_to_topic(
                user_client.user.telegram_user_id,
                user_client.user.forum_supergroup_id,
                topic_id,
                final_text
            )

        except Exception as e:
            logger.error(f"Failed to forward message for user {user_client.user.telegram_user_id}", e)

    def _get_topic_name(self, chat_title: str, first_detected_kw: str, user_client: UserClient) -> str:
        if user_client.user.topic_grouping == GroupingMode.BY_CHAT:
            return chat_title
        else:
            return first_detected_kw.capitalize()

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

    def _normalize(self, s: str) -> str:
        return unicodedata.normalize('NFC', s.strip()).casefold()

    async def _find_topic_by_title(self, client: TelegramClient, forum_id: int, topic_title: str) -> Optional[int]:
        try:
            try:
                forum = await client.get_entity(forum_id)
            except Exception as e:
                logger.error(f"Failed to get forum entity {forum_id}: {e}")
                return None

            for attempt in range(3):
                try:
                    result = await client(GetForumTopicsRequest(
                        channel=forum,
                        offset_date=None,
                        offset_id=0,
                        offset_topic=0,
                        limit=150
                    ))

                    if hasattr(result, 'topics'):
                        for topic in result.topics:
                            if hasattr(topic, 'title') and hasattr(topic, 'id'):
                                if topic.title.lower() == topic_title.lower():
                                    return topic.id
                                if self._normalize(topic.title) == self._normalize(topic_title):
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
            try:
                forum = await client.get_entity(forum_id)
            except Exception as e:
                logger.error(f"Failed to get forum entity {forum_id}: {e}")
                return None

            for attempt in range(3):
                try:
                    result = await client(CreateForumTopicRequest(
                        channel=forum,
                        title=topic_name,
                        icon_color=None,
                        icon_emoji_id=None,
                        send_as=None
                    ))

                    if hasattr(result, 'updates'):
                        for update in result.updates:
                            if hasattr(update, 'message') and hasattr(update.message, 'id'):
                                topic_id = update.message.id
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