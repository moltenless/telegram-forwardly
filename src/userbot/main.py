import asyncio
import logging
from flask import Flask
from app import create_app
from app.services.client_manager import ClientManager
from app.services.scheduler_service import SchedulerService

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)

app = create_app()

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=False)

