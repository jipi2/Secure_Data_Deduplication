from pydantic import BaseModel
class FileTransferDto(BaseModel):
    senderToken: str
    recieverEmail: str
    fileName: str
    base64EncKey: str
    base64EncIv: str