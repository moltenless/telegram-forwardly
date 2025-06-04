from fastapi import FastAPI
from fastapi.responses import JSONResponse

from app.models import StartRequest, CodeRequest, PasswordRequest
from app.authentication_manager import start_authentication, verify_code, verify_password
from app.authentication_manager import logger

app = FastAPI()

@app.post("/start")
async def start_authentication_endpoint(data: StartRequest):
    try:
        if not all([data.user_id, data.phone, data.api_id, data.api_hash]):
            return JSONResponse(
                content={
                "Success": False,
                "ErrorMessage": "Missing required fields",
            }, status_code=400)

        result = await start_authentication(
            data.user_id, data.phone, data.api_id, data.api_hash
        )

        if result.get("Success"):
            return JSONResponse(content=result, status_code=200)
        else:
            return JSONResponse(content=result, status_code=400)

    except Exception as e:
        logger.error(
            {"Error starting authentication": f'telegram_user_id: {data.user_id}'},
                    exc_info=e)
        return JSONResponse(
            content={
            "Success": False,
            "ErrorMessage": "Error occurred trying to make connection or sending the code",
        }, status_code=500)

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

