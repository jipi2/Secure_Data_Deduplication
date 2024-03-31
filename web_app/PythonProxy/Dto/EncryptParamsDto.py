from pydantic import BaseModel

class EncryptParamsDto(BaseModel):
    userEmail:str
    fileName:str
    fileKey:str
    fileIv:str