FROM python:3.11-slim

WORKDIR /app

RUN apt-get update && apt-get install -y \
    gcc \
    && rm -rf /var/lib/apt/lists/*

COPY src/userbot/requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY src/userbot/ .

RUN mkdir -p /app/logs

CMD ["hypercorn", "--config", "hypercorn.toml", "main:app"]
