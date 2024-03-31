from pydantic import BaseModel

class FileResp(BaseModel):
    Id:str
    Answer:str
    FileName:str
    