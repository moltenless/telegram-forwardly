from telethon import TelegramClient
from telethon.sessions import StringSession
from telethon.errors import SessionPasswordNeededError
from typing import Dict, Any
import logging

logger = logging.getLogger(__name__)

_incomplete_sessions = {}

async def start_authentication(user_id, phone, api_id, api_hash) -> Dict[str, Any]:
    try:
        client = TelegramClient(StringSession(), api_id, api_hash)
        await client.connect()

        sent_code = await client.send_code_request(phone, force_sms=True)
        phone_code_hash = sent_code.phone_code_hash
        logger.error(f"Phone code hash {phone_code_hash}")
        session_string = client.session.save()
        logger.error(f"session string {session_string}")
        # await client.disconnect()

        if user_id in _incomplete_sessions:
            session = _incomplete_sessions[user_id]
            await session['client'].disconnect()
        _incomplete_sessions[user_id] = {
            'client': client,
            'phone': phone,
            'api_id': api_id,
            'api_hash': api_hash,
            'session_string': session_string,
            'phone_code_hash': phone_code_hash
        }

        logger.error(f"Started authentication for user {user_id} and sent the code")
        return {"Success": True}

    except Exception as e:
        logger.error(f"Failed to start authentication for user {user_id}", exc_info=e)
        return {
            "Success": False,
            "ErrorMessage": "Error occurred trying to make connection or sending the code",
        }


async def verify_code(user_id, code) -> Dict[str, Any]:
    try:
        session = _incomplete_sessions[user_id]
        client = session['client']
        api_id = session['api_id']
        api_hash = session['api_hash']
        phone = session['phone']
        incomplete_session_string = session['session_string']
        phone_code_hash = session['phone_code_hash']

        # client = TelegramClient(StringSession(incomplete_session_string), api_id, api_hash)
        # await client.connect()

        # logger.error(f"session string after send code request: {incomplete_session_string}")
        # logger.error(f"session string after rereading client from local dict: {client.session.save()}")

        try:
            result = await client.sign_in(
                phone=phone,
                code=code,
                phone_code_hash=phone_code_hash
            )
            logger.error(f'{result}')
            two_fa_enabled = False
        except SessionPasswordNeededError:
            two_fa_enabled = True
        except Exception as e:
            logger.error(f"Failed to verify client with code. Exception: {str(e)}")
            return {'Success': False, 'RequiresPassword': False,
                    'ErrorMessage': f'Invalid verification code or similar. {str(e)}'}

        if two_fa_enabled:
            logger.error(f"2FA password required for user {user_id}")
            return {'Success': False, 'RequiresPassword': True, 'ErrorMessage': '2FA password required'}

        if not await client.is_user_authorized():
            issue = "The client passed code verification without issues, but the user is still not authorized"
            logger.error(f"!!! For user {user_id}: {issue} !!!")
            return {'Success': False, 'RequiresPassword': False, 'ErrorMessage': issue}

        session_string = client.session.save()

        try:
            await client.disconnect()
            del _incomplete_sessions[user_id]
        except:
            logger.error(f"Can't disconnect or del authenticated but temporary session {user_id}")

        logger.error(f"Successfully verified code for user {user_id}")
        return {'Success': True, 'SessionString': session_string}

    except Exception as e:
        logger.error(f"Failed to verify code for user {user_id}", exc_info=e)
        return {
            "Success": False,
            'RequiresPassword': False,
            "ErrorMessage": str(e),
        }

# async def verify_password(phone: str, password: str) -> Dict:
#     client = _sessions.get(phone)
#     if not client:
#         return {"success": False, "error": "Session not found"}
#
#     try:
#         await client.sign_in(password=password)
#         session_string = client.session.save()
#         await client.disconnect()
#         del _sessions[phone]
#         return {"success": True, "session_string": session_string}
#     except Exception as e:
#         return {"success": False, "error": str(e)}
