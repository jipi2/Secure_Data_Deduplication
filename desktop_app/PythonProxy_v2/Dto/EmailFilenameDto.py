from pydantic import BaseModel

class EmailFilenameDto(BaseModel):
    userEmail:str
    fileName:str
    