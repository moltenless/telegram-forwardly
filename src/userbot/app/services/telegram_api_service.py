import asyncio

import requests
import logging
from typing import List, Optional
from app.config import Config
from app.models import BotUser
from app.utils import log_error, log_info, parse_user_from_api
import aiohttp

logger = logging.getLogger(__name__)

class TelegramApiService:

    def __init__(self):
        self.base_url = Config.TELEGRAM_BOT_API_URL.rstrip('/')
        self.session = requests.Session()
        self.session.timeout = 30

    async def get_all_users(self) -> List[BotUser]:
        try:
            url = f"{self.base_url}/users/all/authenticated"
            response = self.session.get(url)
            response.raise_for_status()

            users_data = response.json()

            users = []
            for user_data in users_data:
                user = parse_user_from_api(user_data)
                users.append(user)

            return users

        except requests.RequestException as e:
            logger.error("Failed to get users from Telegram Bot API", e)
            return []
        except Exception as e:
            logger.error("Unexpected error getting users", e)
            return []

    async def send_message_to_topic(self, user_id, forum_id, topic_id, message):
        try:
            url = f"{self.base_url}/message/send"
            payload = {
                'UserId': user_id,
                'ForumId': forum_id,
                'TopicId': topic_id,
                'Message': message
            }
            response = self.session.post(url, json=payload)
            logger.info(f'sent message from bot and received response {response}')
            response.raise_for_status()

        except requests.RequestException as e:
            logger.error("Failed to send the message to forum topic from Telegram Bot API", e)
        except Exception as e:
            logger.error("Unexpected error sending message to forum topic", e)

    async def update_user_session(self, user_id: int, session_string: str) -> bool:
        try:
            url = f"{self.base_url}/users/{user_id}/session"
            data = {"session_string": session_string}

            response = self.session.put(url, json=data)
            response.raise_for_status()

            log_info(f"Updated session for user {user_id}")
            return True

        except requests.RequestException as e:
            log_error(f"Failed to update session for user {user_id}", e)
            return False




    async def check_telegram_bot_health(self, url: str):
        async with aiohttp.ClientSession() as session:
            async with session.get(url) as resp:
                if resp.status != 200:
                    text = await resp.text()
                    raise RuntimeError(f"Health check failed with status {resp.status}: {text}")
                logging.info("Telegram bot health check succeeded")

    async def periodic_health_check(self, attempts=10, interval_sec=5):
        for attempt in range(1, attempts + 1):
            try:
                await self.check_telegram_bot_health(f"{self.base_url}/telegram/health")
                return
            except Exception:
                logging.info(f"Attempt {attempt} to check bot service health failed:")
                if attempt == attempts:
                    raise RuntimeError("Telegram bot health check failed after all attempts")
                await asyncio.sleep(interval_sec)