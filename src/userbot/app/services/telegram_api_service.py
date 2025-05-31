import requests
import logging
from typing import List, Dict, Any, Optional
from app.config import Config
from app.models import BotUser
from app.utils import log_error, log_info, parse_user_from_api

logger = logging.getLogger(__name__)

class TelegramApiService:

    def __init__(self):
        self.base_url = Config.TELEGRAM_BOT_API_URL.rstrip('/')
        self.session = requests.Session()
        self.session.timeout = 30

    async def get_all_users(self) -> List[BotUser]:
        try:
            url = f"{self.base_url}/users/all"
            response = self.session.get(url)
            response.raise_for_status()

            users_data = response.json()
            users = [parse_user_from_api(user_data)
                    for user_data in users_data]

            log_info(f"Retrieved {len(users)} users from Telegram Bot API")
            return users

        except requests.RequestException as e:
            log_error("Failed to get users from Telegram Bot API", e)
            return []
        except Exception as e:
            log_error("Unexpected error getting users", e)
            return []

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

    async def report_user_status(self, user_id: int, is_connected: bool, error_message: Optional[str] = None):
        """Report user connection status to Telegram Bot API"""
        try:
            url = f"{self.base_url}/users/{user_id}/status"
            data = {
                "is_connected": is_connected,
                "error_message": error_message
            }

            response = self.session.put(url, json=data)
            response.raise_for_status()

        except requests.RequestException as e:
            log_error(f"Failed to report status for user {user_id}", e)
