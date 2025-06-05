from fastapi import FastAPI
from fastapi.responses import JSONResponse
from telethon import TelegramClient
from telethon.sessions import StringSession

from app.models import StartRequest, CodeRequest, PasswordRequest
from app.authentication_manager import start_authentication, verify_code#, #verify_password
from app.authentication_manager import logger

import os
import httpx

BOT_URL = os.getenv("TELEGRAM_BOT_API_URL")

app = FastAPI()

async def fetch_data_from_bot(user_id: str):
    url = f"{BOT_URL}/get_code?user_id={user_id}"  # Example URL with query param
    logger.error(f'this message is written beyond the bridge. By the way url is {url}')

    # async with httpx.AsyncClient() as client:
    #     try:
    #         response = await client.get(url)
    #         response.raise_for_status()  # Raise exception on 4xx/5xx
    #
    #         data = response.json()  # <--- Deserialize JSON
    #         return data  # Now it's a Python dict
    #
    #     except httpx.RequestError as e:
    #         print(f"Request failed: {e}")
    #         return None
    #     except httpx.HTTPStatusError as e:
    #         print(f"Non-200 response: {e.response.status_code}")
    #         return None

@app.post("/start")
async def start_authentication_endpoint(data: StartRequest):
    user_id = data.user_id
    chat_id = data.chat_id
    phone = str(data.phone)
    api_id = data.api_id
    api_hash = data.api_hash

    http_client = httpx.Client()

    def get_phone() -> str:
        return phone

    def send_and_await_code() -> str:
        code_response = http_client.get(f"{BOT_URL}/auth/code",
                                        params={'userId': user_id, 'chatId': chat_id},
                                        timeout=180.0)
        code_response.raise_for_status()
        return code_response.json().get('code')

    def await_password() -> str:
        pass_response = http_client.get(f"{BOT_URL}/auth/password",
                                        params={'user_id': user_id, 'chat_id': chat_id},
                                        timeout=120)
        pass_response.raise_for_status()
        return pass_response.json().get('password')

    client = TelegramClient(StringSession(), int(api_id), api_hash)
    await client.start(
        phone=get_phone,
        password=await_password,
        code_callback=send_and_await_code
    )
    session_string = client.session.save()
    logger.error(f'client is authorized: {await client.is_user_authorized()}\nsessions string: {session_string}')

    async with httpx.AsyncClient() as async_http:
        session_response = await async_http.post(f"{BOT_URL}/auth/session",
                                    json={'user_id': user_id, 'chat_id': chat_id, 'session_string': session_string})
        session_response.raise_for_status()

    return JSONResponse(content=session_string, status_code=200)



@app.post("/verify")
async def verify_code_endpoint(data: CodeRequest):
    try:
        if not all([data.user_id, data.code]):
            return JSONResponse(
                content={
                    "Success": False,
                    "RequiresPassword": False,
                    "ErrorMessage": "Missing required fields",
                }, status_code=400)

        result = await verify_code(
            data.user_id, data.code
        )

        if result.get("Success"):
            return JSONResponse(content=result, status_code=200)
        else:
            return JSONResponse(content=result, status_code=400)

    except Exception as e:
        logger.error(
            {"Error verifying code": f'telegram_user_id: {data.user_id}'},
                    exc_info=e)
        return JSONResponse(
            content={
                "Success": False,
                "RequiresPassword": False,
                "ErrorMessage": str(e),
            }, status_code=500)

# @app.post("/password")
# async def verify_password_endpoint(data: PasswordRequest):
#     return await sign_in_with_password(data.phone, data.password)

