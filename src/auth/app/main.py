from fastapi import FastAPI
from fastapi.responses import JSONResponse
from telethon.sync import TelegramClient
from telethon.errors import SessionPasswordNeededError
from telethon.sessions import StringSession

from app.models import StartRequest, CodeRequest, PasswordRequest
from app.authentication_manager import start_authentication, verify_code, verify_password
from app.authentication_manager import logger

import os
import httpx

BOT_URL = os.getenv("TELEGRAM_BOT_API_URL")

app = FastAPI()

@app.post("/start")
async def start_authentication_endpoint(data: StartRequest):
    user_id = data.user_id
    chat_id = data.chat_id
    phone = str(data.phone)
    api_id = data.api_id
    api_hash = data.api_hash

    http_client = httpx.Client()

    def await_code() -> str:
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

    client = TelegramClient(StringSession(), api_id, api_hash)
    client.connect()
    result = client.send_code_request(phone)
    code = await_code()

    try:
        result = client.sign_in(phone=phone, code=code, phone_code_hash=result.phone_code_hash)
        logger.error(f'{result}')
        two_fa_enabled = False
    except SessionPasswordNeededError:
        two_fa_enabled = True
    except Exception as e:
        logger.error(f'Failed to erify client with code. Exception: {e}')
        #maybe notify
        raise e

    if two_fa_enabled:
        password = await_password()
        result = client.sign_in(password=password)
        logger.error(f'{result}')

    if not client.is_user_authorized():
        issue = "The client passed code verification without issues, but the user is still not authorized"
        logger.error(f"!!! For user {user_id}: {issue} !!!")

    me = client.get_me()
    logger.error(me.phone)

    session_string = client.session.save()

    logger.error(f'client is authorized: {client.is_user_authorized()}\nsessions string: {session_string}')

    client.disconnect()

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

