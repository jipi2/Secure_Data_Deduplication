from pydantic import BaseModel

class FileDecDto(BaseModel):
    base64Key: str
    base64Iv: str
    fileName: str
    tag:str