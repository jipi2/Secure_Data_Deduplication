import uvicorn
from fastapi import FastAPI, Body, Depends, HTTPException
import httpx
from auth.auth_bearer import JWTBearer
from starlette.requests import Request
from decouple import config
from fastapi.middleware.cors import CORSMiddleware
from fastapi_utilities import repeat_every
from CryptoFolder.Utils import *
import schedule
import os
from dotenv import load_dotenv
from celery import Celery

# Database
from Database.db import Base

# SSL
import ssl

# #DTO-uri
from Dto.LoginUserDto import LoginUserDto
from  Dto.TestDto import TestDto
from Dto.Resp import Resp
from Dto.FIleParamsDto import FileParamsDto
from Dto.TagDto import TagDto
from Dto.FileTransferDto import FileTransferDto
from Dto.EmailFilenameDto import *

#Services
from services.FileService import FileService

baseDB = Base()
baseDB.metadata.create_all(baseDB.engine)

app = FastAPI() 
#CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

load_dotenv()

# SSL
# ssl_context = ssl.SSLContext(ssl.PROTOCOL_TLSv1_2)
# ssl_context.load_cert_chain('./SSL_Folder/fastApiServer.cer', './SSL_Folder/key_ca.prv')

# @app.on_event("startup")
# @repeat_every(seconds=60)
# def send_files_task():
#     print('sending files to server')
#     sendFilesToServer.delay()


celery = Celery(
    "tasks",
    broker="redis://redis:6379/0",
    backend="redis://redis:6379/0",
    include=["tasks_"]
)

# @app.on_event("startup")
# @repeat_every(seconds=60*60)
# def send_files_task():
#     print('sending files to server')
#     result = celery.send_task("tasks_.sendFilesToServer")
#     return {"message": "Task enqueued", "task_id": result.id}

@app.get("/")
def root():
    print(os.environ.get('backendBaseUrl'))
    return {"message": "Hello World"}

@app.get("/testBACK")
def testBack():
    with httpx.Client(verify=False) as client:
        response = client.get("https://localhost:7109/api/User/testProxyController")
        print(response.text)

@app.get("/testMethod")   
def testMethod():
    testDto = TestDto(id=1,name="test")
    return testDto

@app.post("/uploadFile", tags = ['file'])
async def uploadFile(request: Request, fileParams:FileParamsDto ):
    authorization_header = request.headers.get("Authorization")
    token=""
    if authorization_header is not None:
        token = authorization_header.split(" ")[1]
    _fileService = FileService(token, fileParams)
    try:
        print('Ajungem aici')
        await _fileService.computeFileVerification()
        return {"message": "File uploaded successfully"}
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))


@app.get ("/getFileNamesAndDates", tags=['file'])
async def getFilesNamesAndDates(request:Request):
    authorization_header = request.headers.get("Authorization")
    token=""
    if authorization_header is not None:
        token = authorization_header.split(" ")[1]
    _fileService = FileService(token)
    try:
        filesList = await _fileService.getFilesNamesAndDates()
        return filesList
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))

@app.post("/getFileFromStorage", tags=['file'])
async def getFileFromStorage(request:Request,filename:str = Body(...)):
    authorization_header = request.headers.get("Authorization")
    token=""
    if authorization_header is not None:
        token = authorization_header.split(" ")[1]
    _fileService = FileService(userToken=token, filename=filename)
    try:
        result = await _fileService.getFileFromStorage()
        return result
    except Exception as e:
        print(str(e))
        raise HTTPException(status_code=400, detail=str(e))

@app.post("/deleteFile", tags=['file'])
async def deleteFile(request:Request, file_name:str = Body(...)):
    authorization_header = request.headers.get("Authorization")
    token=""
    if authorization_header is not None:
        token = authorization_header.split(" ")[1]
    _fileService = FileService(userToken=token, filename=file_name)
    try:
        result = await _fileService.deleteFile()
        return result
    except Exception as e:
        print(str(e))
        raise HTTPException(status_code=400, detail=str(e))

@app.post("/getPubKeyAndFileKey", tags = ['file'])
async def getPubKeyAndFileKey(request:Request, emailFileNameDto:EmailFilenameDto):
    authorization_header = request.headers.get("Authorization")
    token=""
    if authorization_header is not None:
        token = authorization_header.split(" ")[1]
    _fileService = FileService(userToken=token, recieverEmail=emailFileNameDto.userEmail ,filename=emailFileNameDto.fileName)
    try:
        result = await _fileService.getPubKeyAndFileKey()
        return result
    except Exception as e:
        print(str(e))
        raise HTTPException(status_code=400, detail=str(e))
    

@app.post("/sendFile", tags = ['file'])
async def sendFile(request:Request, fileTransferDto:FileTransferDto):
    token = fileTransferDto.senderToken
    _fileService = FileService(userToken=token, recieverEmail=fileTransferDto.recieverEmail, base64EncKey=fileTransferDto.base64EncKey, base64EncIv=fileTransferDto.base64EncIv,filename=fileTransferDto.fileName)
    try:
        result, userEmail = await _fileService.sendFile()
        if result == True:
            result = celery.send_task("tasks_.transferFileBetweenUsers", args=(userEmail, fileTransferDto.senderToken, fileTransferDto.recieverEmail, fileTransferDto.fileName, fileTransferDto.base64EncKey, fileTransferDto.base64EncIv))
        return "Ok"
    except Exception as e:
        print(str(e))
        raise HTTPException(status_code=400, detail=str(e))

if __name__== "__main__":
    uvicorn.run("main:app",
                host="0.0.0.0",
                port=8000,
                reload=True,
                ssl_keyfile="./SSL_Folder/localhost+2-key.pem", 
                ssl_certfile="./SSL_Folder/localhost+2.pem"
            )