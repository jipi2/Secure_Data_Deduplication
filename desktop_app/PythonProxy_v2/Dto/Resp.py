from pydantic import BaseModel

class Resp(BaseModel):
    Success: bool
    Message: str
    AccessToken: str
    