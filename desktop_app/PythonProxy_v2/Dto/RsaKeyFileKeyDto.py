from pydantic import BaseModel

class RsaKeyFileKeyDto(BaseModel):
    pubKey: str
    fileKey: str
    fileIv: str
    