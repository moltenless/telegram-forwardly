from pydantic import BaseModel

class StartRequest(BaseModel):
    user_id: int
    phone: str
    api_id: str
    api_hash: str

class CodeRequest(BaseModel):
    user_id: int
    code: str

class PasswordRequest(BaseModel):
    user_id: int
    password: str