import logging
from typing import Optional, List
from telethon import events
from telethon.tl.types import Message
from app.models import UserClient, GroupingMode
from app.services.telegram_api_service import TelegramApiService
from app.utils import log_error, log_info

logger = logging.getLogger(__name__)


class MessageHandler:
    """Handles incoming Telegram messages and forwards them"""

    def __init__(self):
        self.telegram_api = TelegramApiService()

    async def handle_message(self, event: events.NewMessage.Event, user_client: UserClient):
        """Handle incoming message from user's monitored chats"""
        try:
            if not user_client.user.forwardly_enabled:
                return

            message = event.message
            chat_id = event.chat_id

            # Check if this chat should be monitored
            if not self._should_monitor_chat(chat_id, user_client):
                return

            # Check if message contains keywords
            if not self._message_contains_keywords(message, user_client.user.keywords):
                return

            # Forward the message
            await self._forward_message(message, chat_id, user_client)

        except Exception as e:
            log_error(f"Error handling message for user {user_client.user.telegram_user_id}", e)

    def _should_monitor_chat(self, chat_id: int, user_client: UserClient) -> bool:
        """Check if chat should be monitored"""
        if user_client.user.all_chats_filtering_enabled:
            return True

        monitored_chat_ids = [chat.telegram_chat_id for chat in user_client.user.chats]
        return chat_id in monitored_chat_ids

    def _message_contains_keywords(self, message: Message, keywords: List) -> bool:
        """Check if message contains any of the keywords"""
        if not keywords or not message.text:
            return False

        message_text = message.text.lower()
        keyword_values = [kw.value.lower() for kw in keywords]

        return any(keyword in message_text for keyword in keyword_values)

    async def _forward_message(self, message: Message, source_chat_id: int, user_client: UserClient):
        """Forward message to user's forum group"""
        try:
            if not user_client.user.forum_supergroup_id:
                log_error(f"No forum group configured for user {user_client.user.telegram_user_id}")
                return

            # Get chat info for topic name
            chat = await user_client.client.get_entity(source_chat_id)
            chat_title = getattr(chat, 'title', f'Chat {source_chat_id}')

            # Determine topic based on grouping mode
            topic_name = self._get_topic_name(message, chat_title, user_client)

            # Create or get topic ID
            topic_id = await self._get_or_create_topic(
                user_client.client,
                user_client.user.forum_supergroup_id,
                topic_name
            )

            if topic_id:
                # Forward message to topic
                await user_client.client.forward_messages(
                    entity=user_client.user.forum_supergroup_id,
                    messages=message,
                    from_peer=source_chat_id,
                    reply_to=topic_id
                )

                log_info(f"Forwarded message to topic '{topic_name}' for user {user_client.user.telegram_user_id}")

        except Exception as e:
            log_error(f"Failed to forward message for user {user_client.user.telegram_user_id}", e)

    def _get_topic_name(self, message: Message, chat_title: str, user_client: UserClient) -> str:
        """Get topic name based on grouping mode"""
        if user_client.user.topic_grouping == GroupingMode.BY_CHAT:
            return chat_title
        else:  # BY_KEYWORDS or default
            # Find matching keyword
            if message.text:
                message_text = message.text.lower()
                for keyword in user_client.user.keywords:
                    if keyword.value.lower() in message_text:
                        return keyword.value.capitalize()
            return "General"

    async def _get_or_create_topic(self, client, forum_id: int, topic_name: str) -> Optional[int]:
        """Get existing topic or create new one"""
        try:
            # This is a simplified implementation
            # In a real scenario, you'd need to track topics or use Telegram's topic API
            # For now, we'll use a basic message reply approach
            return None  # Placeholder - implement topic creation logic

        except Exception as e:
            log_error(f"Failed to get/create topic '{topic_name}'", e)
            return None
