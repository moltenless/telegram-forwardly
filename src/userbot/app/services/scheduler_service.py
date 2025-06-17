import logging
from apscheduler.schedulers.asyncio import AsyncIOScheduler
from apscheduler.triggers.interval import IntervalTrigger

from app.async_loop_manager import event_loop_manager
from app.config import Config

logger = logging.getLogger(__name__)

class SchedulerService:
    def __init__(self, client_manager):
        self.client_manager = client_manager
        self.scheduler = AsyncIOScheduler(event_loop=event_loop_manager.loop)
        self._setup_jobs()

    def _setup_jobs(self):
        self.scheduler.add_job(
            func=self._update_users_job,
            trigger=IntervalTrigger(minutes=Config.USERS_UPDATE_INTERVAL_MINUTES),
            id='update_users',
            name='Update users from Telegram Bot API',
            replace_existing=True,
        )

    async def start(self):
        try:
            self.scheduler.start()
            logger.info("Scheduler started successfully")
        except Exception as e:
            logger.error("Failed to start scheduler", e)

    async def stop(self):
        try:
            self.scheduler.shutdown()
            logger.info("Scheduler stopped")
        except Exception as e:
            logger.error("Failed to stop scheduler", e)

    async def _update_users_job(self):
        try:
            logger.info("Starting periodic user update...")
            event_loop_manager.run_coroutine(self.client_manager.launch_clients_from_database())
            logger.info(f"Periodic user update completed. Active clients: {len(self.client_manager.clients)}")

        except Exception as e:
            logger.error("Error in periodic user update", e)
