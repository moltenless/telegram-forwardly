import logging
from apscheduler.schedulers.asyncio import AsyncIOScheduler
from apscheduler.triggers.interval import IntervalTrigger

from app.config import Config

logger = logging.getLogger(__name__)

class SchedulerService:
    def __init__(self, client_manager, loop):
        self.client_manager = client_manager
        self.scheduler = AsyncIOScheduler(event_loop=loop)
        self.scheduler.add_job(
            func=self._update_users_job,
            trigger=IntervalTrigger(minutes=Config.USERS_UPDATE_INTERVAL_MINUTES),
            id='update_users',
            name='Update users from Telegram Bot API',
            replace_existing=True,
        )

    async def _update_users_job(self):
        try:
            await self.client_manager.launch_clients_from_database()
        except Exception as e:
            logger.error("Error in periodic user update", e)

    def start(self):
        try:
            self.scheduler.start()
        except Exception as e:
            logger.error("Failed to start scheduler", e)

    def stop(self):
        try:
            self.scheduler.shutdown()
        except Exception as e:
            logger.error("Failed to stop scheduler", e)

