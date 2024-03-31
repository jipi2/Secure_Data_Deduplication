from pydantic import BaseModel

class LoginUserDto(BaseModel):
    Email: str
    password: str