import logging
from logging.handlers import RotatingFileHandler

from app import create_app

logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)

app = create_app()

file_handler = RotatingFileHandler('logs/userbot.log', maxBytes=10240, backupCount=10)
file_handler.setFormatter(logging.Formatter(
    '%(asctime)s %(levelname)s: %(message)s [in %(pathname)s:%(lineno)d]'
))
file_handler.setLevel(logging.INFO)
app.logger.addHandler(file_handler)


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000, debug=False)

