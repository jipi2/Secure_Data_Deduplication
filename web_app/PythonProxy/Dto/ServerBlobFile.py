from pydantic import BaseModel

class ServerBlobFile(BaseModel):
    FileName:str
    FileKey:str
    EncBase64File:str
    FileIv:str