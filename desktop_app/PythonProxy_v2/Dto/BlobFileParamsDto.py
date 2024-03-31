from pydantic import BaseModel


class BlobFileParamsDto(BaseModel):
    fileName:str
    fileKey:str
    fileIv:str
    base64tag:str