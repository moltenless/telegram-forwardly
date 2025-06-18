import os
from dotenv import load_dotenv

load_dotenv()

class Config:
    TELEGRAM_BOT_API_URL = os.getenv('TELEGRAM_BOT_API_URL')

    SECRET_KEY = os.getenv('SECRET_KEY')
    DEBUG = os.getenv('FLASK_DEBUG').lower() == 'true'

    USERS_UPDATE_INTERVAL_MINUTES = int(os.getenv('USERS_UPDATE_INTERVAL_MINUTES'))

    LOG_LEVEL = os.getenv('LOG_LEVEL')

    API_KEY = os.getenv('API_KEY')
    