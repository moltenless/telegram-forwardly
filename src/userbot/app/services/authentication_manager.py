from telethon import TelegramClient
from telethon.sessions import StringSession
from telethon.errors import SessionPasswordNeededError
import asyncio

async def authenticate_first_step_not_disconnected(api_id, api_hash, phone_number):
    client = TelegramClient(StringSession(), api_id, api_hash)
    await client.connect()
    await client.send_code_request(phone_number)
    return client


async def authenticate_second_step(client, phone_number, verification_code):
    try:
        await client.sign_in(phone_number, verification_code)
        two_fa_enabled = False
    except SessionPasswordNeededError:
        two_fa_enabled = True
    return two_fa_enabled


async def authenticate_third_step(client, account_password):
    await client.sign_in(password=account_password)


# the function creates session file, adds a new telegram session as a device session
# the function connects and disconnects a client by itself
async def establish_new_client():
    client = await authenticate_first_step_not_disconnected(api_id, api_hash, phone_number)

    two_fa_enabled = await authenticate_second_step(client, phone_number, verification_code)

    if two_fa_enabled:
        await authenticate_third_step(client, account_password)

    session_string = client.session.save()
    me = await client.get_me()
    with open(f'user_session_{me.id}.txt', 'w') as f:
        f.write(session_string)

    await client.disconnect()


# the function uses existing session file and also existing telegram session
# the function doesn't disconnect the client it returns. !Responsibility for client disconnection lies with caller!
async def connect_existing_client(session_string, api_id, api_hash):
    client = TelegramClient(StringSession(session_string), api_id, api_hash)
    await client.connect()
    return client if await client.is_user_authorized() else None


async def use_example():
    with open('user_session_587388238.txt', 'r') as f:
        session_string1 = f.read()
    client1 = await connect_existing_client(session_string1, '27445408',
                                  'b6ea5bb8d925fa5a08eafe32adbc401c')
    me1 = await client1.get_me()
    print(me1.phone)
    await client1.disconnect()


    with open('user_session_7414743371.txt', 'r') as f:
        session_string2 = f.read()
    client2 = await connect_existing_client(session_string2, '28908917',
                                            '6035da822051f8846d2ed83d5269a336')
    me2 = await client2.get_me()
    print(me2.phone)
    await client2.disconnect()


asyncio.run(main())
