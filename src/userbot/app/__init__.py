from flask import Flask
import asyncio
import threading
from app.config import Config
from app.routes import api_bp
from app.services.client_manager import ClientManager
from app.services.scheduler_service import SchedulerService

def create_app():
    app = Flask(__name__)
    app.config.from_object(Config)

    app.register_blueprint(api_bp, url_prefix='/api')

    client_manager = ClientManager()
    scheduler_service = SchedulerService(client_manager)

    app.client_manager = client_manager
    app.scheduler_service = scheduler_service

    def start_background_services():
        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)

        loop.run_until_complete(client_manager.initialize_from_database())

        scheduler_service.start()

        loop.run_until_complete(client_manager.start_message_handling())

        loop.run_forever()

    background_thread = threading.Thread(target=start_background_services, daemon=True)
    background_thread.start()

    return app