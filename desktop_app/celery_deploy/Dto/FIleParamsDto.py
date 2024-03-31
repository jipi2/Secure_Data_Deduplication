from pydantic import BaseModel

class FileParamsDto(BaseModel):
    base64EncFile: str
    base64KeyEnc: str
    base64IvEnc: str
    base64TagEnc: str
    encFileName: str