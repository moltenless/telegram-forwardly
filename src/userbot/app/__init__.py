from flask import Flask
from app.config import Config
from app.routes import api_bp
from app.services.client_manager import ClientManager
from app.services.scheduler_service import SchedulerService
from app.async_loop_manager import event_loop_manager
from app.services.telegram_api_service import TelegramApiService

def create_app():
    app = Flask(__name__)
    app.config.from_object(Config)
    app.register_blueprint(api_bp, url_prefix='/api')

    event_loop_manager.start_loop()

    client_manager = ClientManager()
    app.client_manager = client_manager

    scheduler_service = SchedulerService(client_manager)
    app.scheduler_service = scheduler_service

    event_loop_manager.run_coroutine(client_manager.telegram_api.periodic_health_check())
    event_loop_manager.run_coroutine(client_manager.launch_clients_from_database())
    event_loop_manager.run_coroutine(scheduler_service.start())

    return app