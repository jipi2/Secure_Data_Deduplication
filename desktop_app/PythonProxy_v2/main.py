import uvicorn
from fastapi import FastAPI, Body, Depends, HTTPException, UploadFile, Form, File
from fastapi.responses import FileResponse
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
from services.AzureBlobService import download_blob

#asta este doar pt test:
from CryptoFolder import Utils
from CryptoFolder import MerkleTree
from CryptoFolder import MTMember

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


# celery = Celery(
#     "tasks",
#     broker="redis://redis:6379/0",
#     backend="redis://redis:6379/0",
#     include=["tasks_"]
# )

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
    download_blob('zN4n8TV/Z9dtfPzdryc83Rqn/S5H6MMLKjFy5Hv9fDA=', '50MiB.txt')

@app.get("/testMethod")   
def testMethod():
    testDto = TestDto(id=1,name="test")
    return testDto

@app.post("/uploadFile", tags = ['file'])
async def uploadFile(request: Request,
                    fileName: str = Form(...),
                    base64Key: str = Form(...),
                    base64Iv : str = Form(...),
                    base64Tag:str = Form(...),
                    file: UploadFile = UploadFile(...)):
    authorization_header = request.headers.get("Authorization")
    token=""
    if authorization_header is not None:
        token = authorization_header.split(" ")[1]
    try:
        _fileService = FileService(userToken=token, filename=fileName, base64Key=base64Key, base64Iv=base64Iv)
        await _fileService.computeFileVerification_v2(file, base64Tag)
    
    except Exception as e:
        print(str(e))
        raise HTTPException(status_code=400, detail=str(e))

@app.get("/crazy_cloud", tags = ['file'])
async def crazy_cloud(request: Request):
    try:
        fs = FileService("asdasda")
        await fs.sendFilesToServer_v2()
    except Exception as e:
        print(str(e))
        raise HTTPException(status_code=400, detail=str(e))

@app.post("/uploadTEST/")
async def upload_file(file: UploadFile = UploadFile(...)):
    try:
        file_path = os.path.join(os.getcwd(), file.filename)
        print(file.size)
        with open(file_path, "wb") as buffer:
            while True:
                chunk = await file.read(4096)
                if not chunk:
                    break
                buffer.write(chunk)
        return {"message": "File uploaded successfully"}
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error uploading file: {str(e)}")

# @app.post("/uploadFile_v2", tags = ['file'])
# async def uploadFile_v2(request: Request,file: UploadFile = File(...)):
#     try:
#         mt = await Utils.get_merkle_tree(file)
#         print(len(mt._HashTree))
#     except Exception as e:
#         print(str(e))
#         raise HTTPException(status_code=400, detail=str(e))

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
        print(str(e.args[0]))
        raise HTTPException(status_code=400, detail=str(e.args[0]))

@app.get("/getFileFromStorage/", tags=['file'])
async def getFileFromStorage(request:Request, filename:str):
    authorization_header = request.headers.get("Authorization")
    token=""
    if authorization_header is not None:
        token = authorization_header.split(" ")[1]
    print(filename)
    _fileService = FileService(userToken=token, filename=filename)
    try:
        result = await _fileService.getFileFromStorage()
        return FileResponse(result)
    except Exception as e:
        print(str(e))
        raise HTTPException(status_code=400, detail=str(e))

@app.get("/getKeyAndIvForFile/", tags=['file'])
async def getKeyAndIvFromStorage(request:Request, filename:str):
    authorization_header = request.headers.get("Authorization")
    token=""
    if authorization_header is not None:
        token = authorization_header.split(" ")[1]
    print(filename)
    _fileService = FileService(userToken=token, filename=filename)
    try:
        result = await _fileService.getKeyAndIvForFile(filename)
        return result
    except Exception as e:
        print(str(e))
        raise HTTPException(status_code=400, detail=str(e))

@app.get("/download/{filename}")
async def download_file():
    file_path = './uploadedFiles/mihnea@mta/craiului.jpg'
    try:
        return FileResponse(file_path, media_type='application/octet-stream', filename='craiului.jpg')
    except FileNotFoundError:
        raise HTTPException(status_code=404, detail="File not found")


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
    

# @app.post("/sendFile", tags = ['file'])
# async def sendFile(request:Request, fileTransferDto:FileTransferDto):
#     token = fileTransferDto.senderToken
#     _fileService = FileService(userToken=token, recieverEmail=fileTransferDto.recieverEmail, base64EncKey=fileTransferDto.base64EncKey, base64EncIv=fileTransferDto.base64EncIv,filename=fileTransferDto.fileName)
#     try:
#         result, userEmail = await _fileService.sendFile()
#         if result == True:
#             result = celery.send_task("tasks_.transferFileBetweenUsers", args=(userEmail, fileTransferDto.senderToken, fileTransferDto.recieverEmail, fileTransferDto.fileName, fileTransferDto.base64EncKey, fileTransferDto.base64EncIv))
#         return "Ok"
#     except Exception as e:
#         print(str(e))
#         raise HTTPException(status_code=400, detail=str(e))

if __name__== "__main__":
    uvicorn.run("main:app",
                host="0.0.0.0",
                port=8000,
                reload=True,
                ssl_keyfile="./SSL_Folder/localhost+2-key.pem", 
                ssl_certfile="./SSL_Folder/localhost+2.pem"
            )