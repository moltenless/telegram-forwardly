import asyncio
import logging
from apscheduler.schedulers.asyncio import AsyncIOScheduler
from apscheduler.triggers.interval import IntervalTrigger
from app.config import Config
from app.utils import log_info, log_error

logger = logging.getLogger(__name__)


class SchedulerService:
    """Handles scheduled tasks like periodic user updates"""

    def __init__(self, client_manager):
        self.client_manager = client_manager
        self.scheduler = AsyncIOScheduler()
        self._setup_jobs()

    def _setup_jobs(self):
        """Setup scheduled jobs"""
        # Update users every N minutes
        self.scheduler.add_job(
            func=self._update_users_job,
            trigger=IntervalTrigger(minutes=Config.USERS_UPDATE_INTERVAL_MINUTES),
            id='update_users',
            name='Update users from Telegram Bot API',
            replace_existing=True
        )

    def start(self):
        """Start the scheduler"""
        try:
            self.scheduler.start()
            log_info("Scheduler started successfully")
        except Exception as e:
            log_error("Failed to start scheduler", e)

    def stop(self):
        """Stop the scheduler"""
        try:
            self.scheduler.shutdown()
            log_info("Scheduler stopped")
        except Exception as e:
            log_error("Failed to stop scheduler", e)

    async def _update_users_job(self):
        """Periodic job to update users from Telegram Bot API"""
        try:
            log_info("Starting periodic user update...")

            # Get latest users from API
            users = await self.client_manager.telegram_api.get_all_users()

            # Update existing clients and create new ones
            current_user_ids = set(self.client_manager.clients.keys())
            new_user_ids = {user.telegram_user_id for user in users if user.is_authenticated}

            # Remove disconnected users
            users_to_remove = current_user_ids - new_user_ids
            for user_id in users_to_remove:
                await self.client_manager._disconnect_client(user_id)

            # Update/create clients for active users
            for user in users:
                if user.is_authenticated and user.session_string:
                    if user.telegram_user_id not in self.client_manager.clients:
                        await self.client_manager._create_and_connect_client(user)
                    else:
                        # Update existing client if needed
                        existing_client = self.client_manager.clients[user.telegram_user_id]
                        existing_client.user = user

            log_info(f"Periodic user update completed. Active clients: {len(self.client_manager.clients)}")

        except Exception as e:
            log_error("Error in periodic user update", e)
