from pydantic import BaseModel, constr
from pydantic import BaseModel, constr

class FileParamsDto(BaseModel):
    base64EncFile: str
    base64KeyEnc: str
    base64IvEnc: str
    base64TagEnc: str
    encFileName: str