from pydantic import BaseModel
from datetime import datetime

class UsersEmailsFileNames(BaseModel):
    userEmail:str
    fileName:str
    uploadTime:str
    
class PersonalisedInfoDto(BaseModel):
    fileName:str
    base64key:str
    base64iv:str
    email:str
    UploadDate:str

class FileFromCacheDto(BaseModel):
    base64EncFile:str
    base64Tag:str
    # key:str
    # iv:str
    personalisedList:list[PersonalisedInfoDto]
    
class FileFromCacheDto_v2(BaseModel):
    base64Tag:str
    encFilePath:str
    personalisedList:list[PersonalisedInfoDto]    