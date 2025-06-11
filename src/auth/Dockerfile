FROM python:3.11-slim

WORKDIR /app

COPY src/auth/requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY src/auth .

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
