from pydantic import BaseModel, constr
from pydantic import BaseModel, constr

class FileParamsDto(BaseModel):
    base64Key: str
    base64Iv: str
    base64Tag: str
    fileName: str