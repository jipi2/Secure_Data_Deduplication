from pydantic import BaseModel
from datetime import datetime

class UsersEmailsFileNames(BaseModel):
    userEmail:str
    fileName:str
    uploadTime:str

class FileFromCacheDto(BaseModel):
    base64EncFile:str
    base64Tag:str
    key:str
    iv:str
    emailsFilenames:list[UsersEmailsFileNames]