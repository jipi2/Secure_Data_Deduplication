from pydantic import BaseModel

class FileEncDto(BaseModel):
    userEmail:str
    base64KeyEnc: str
    base64IvEnc: str
    encFileName: str
    encBase64Tag: str