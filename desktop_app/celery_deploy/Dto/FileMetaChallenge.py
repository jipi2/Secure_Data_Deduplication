from pydantic import BaseModel

class FileMetaChallenge(BaseModel):
    id:str
    n1:str
    n2:str
    n3:str