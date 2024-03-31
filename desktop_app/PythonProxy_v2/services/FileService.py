from Dto.FIleParamsDto import FileParamsDto
from services.AuthenticateService import ProxyClass
from services.ApiCallsService import ApiCall
from CryptoFolder.Utils import *
from CryptoFolder.MerkleTree import MerkleTree
from Dto.FileResp import FileResp
from Dto.FileDecDto import FileDecDto
from Dto.FileEncDto import FileEncDto
from Dto.FileDedupDto import FileDedupDto
from Dto.ServerBlobFile import ServerBlobFile
from Dto.EncryptParamsDto import EncryptParamsDto
from Database.db import Base, User, File, BlobFile, UserFile
from Dto.FilesNameDate import FilesNameDate
from Dto.EmailFilenameDto import EmailFilenameDto
from Dto.FileTransferDto import FileTransferDto
from Dto.RsaKeyFileKeyDto import *
from Dto.FileFromCacheDto import FileFromCacheDto, UsersEmailsFileNames, PersonalisedInfoDto, FileFromCacheDto_v2
from Dto.FileKeyAndIvDto import FileKeyAndIvDto
from Dto.BlobFileParamsDto import BlobFileParamsDto
import asyncio
from sqlalchemy.orm.exc import NoResultFound
import base64
import os
from dotenv import load_dotenv  
import datetime
from sqlalchemy import and_

from fastapi import UploadFile
from services.AzureBlobService import download_blob

load_dotenv()


class FileService():
    def __init__(self, userToken:str, fileParams:FileParamsDto=None, filename:str=None, recieverEmail:str=None, base64EncKey:str=None, base64EncIv:str=None, base64Key:str=None, base64Iv:str=None):
        self.userToken = userToken
        self.userEmail = ""
        self.fileParams = fileParams
        self.filename = filename
        self.recieverEmail = recieverEmail
        self.base64EncKey = base64EncKey
        self.base64EncIv = base64EncIv
        self.authService = ProxyClass()
        self.gateWay = ApiCall(os.environ.get("backendBaseUrl"))
        
        self.base64Key = base64Key
        self.base64Iv = base64Iv
    
    async def __getUserEmail(self):
        userEmail = await self.authService.getUserEmail(self.userToken)
        if(userEmail == None):
            print("e None")
            raise Exception("JWT not valid")
        self.userEmail = str(userEmail)
        
    def __getFileBase64Tag(self, base64EncFile:str):
        tag = generateHashForTag(base64EncFile)
        return tag
    
    async def __verifyTag(self, base64tag:str):
        response = await self.gateWay.callBackendPostMethodWithSimpleParams("/api/File/checkTag", self.authService.token, base64tag)
        if response.text == "false":
            return False
        return True
    
    async def __getMerkleTree(self, file:UploadFile):
        self.mt = await get_merkle_tree(file)
        

    async def __getChallenge(self, base64tag:str):
        response = await self.gateWay.callBackendPostMethodWithSimpleParams("/api/File/getChallenge",
                                                                            self.authService.token,
                                                                            base64tag)
        fileChallenge = FileMetaChallenge(**response.json())
        return fileChallenge
    
    async def __getDecryptedFileParams(self):
        fileEncDto = FileEncDto(userEmail=self.userEmail,
                                base64KeyEnc=self.fileParams.base64KeyEnc,
                                base64IvEnc=self.fileParams.base64IvEnc,
                                encFileName=self.fileParams.encFileName,
                                encBase64Tag=self.fileParams.base64TagEnc)
        response = await self.gateWay.callBackendPostMethodDto("/api/File/getDecryptedFileParams",
                                                         self.authService.token,
                                                         fileEncDto)
        if response.status_code != 200:
            raise Exception(response.text)
        
        fileDecDto = FileDecDto(
            base64Key=response.json()['base64key'],
            base64Iv=response.json()['base64iv'],
            fileName=response.json()['fileName'],
            tag=response.json()['tag']
        )
        
        return fileDecDto
    
    async def __saveDeduplicateFileForUser(self, tag, fileName, base64key, base64iv):
        fileDedupDto = FileDedupDto(userEmail=self.userEmail,base64tag=tag,
                                    fileName=fileName, base64key=base64key, base64iv=base64iv)
        response = await self.gateWay.callBackendPostMethodDto("/api/File/saveDeduplicateFileForUser",
                                                         self.authService.token,
                                                         fileDedupDto)
        if(response.status_code != 200):
            raise Exception(response.text)
    
    def __saveUser(self, session):
        new_user = User(email=self.userEmail)
        session.add(new_user)
        session.commit()
    
    def __saveFile(self, tag, fileDecDto, session):
        blob_file = BlobFile(base64EncFile=base64.b64decode(self.fileParams.base64EncFile))        
        new_file = File(tag=tag, blob_file=blob_file)
        
        session.add(new_file)
        session.commit()
        
    
    def __checkIfUserHasFile(self, filename): #verifica numai daca userul are fisierul in cache
        basedb = Base()
        session = basedb.getSession()
        result = session.query(UserFile).join(User, UserFile.user_id == User.id).filter(and_(UserFile.fileName == filename, User.email == self.userEmail)).all() # asta trebuie verificata
        if not result:
            return False
        return True
    
    async def __saveInCache(self, base64tag, fileDecDto):
        try:
            basedb = Base()
            session = basedb.getSession()
            user = session.query(User).filter(User.email==self.userEmail).first()
            if not user:
                self.__saveUser(session)
                user = session.query(User).filter(User.email==self.userEmail).first()

            print('here')
            file = session.query(File).filter(File.tag == base64tag).first()
            if not file:
                self.__saveFile(base64tag, fileDecDto, session)
                file = session.query(File).filter(File.tag == base64tag).first()
            new_user_file = UserFile(user_id=user.id, file_id=file.id, key=fileDecDto.base64Key, iv=fileDecDto.base64Iv, fileName=fileDecDto.fileName, date=datetime.datetime.utcnow())
            session.add(new_user_file)
            session.commit()
            session.close()
            print('finally')
        except Exception as e:
            print(f"Error saving in cache: {e}")
        finally:
            session.close()

    async def computeFileVerification(self):
        await self.authService.getProxyToken()
        await self.__getUserEmail()
        fileDecDto = await self.__getDecryptedFileParams()
        # tag = fileDecDto.tag
        tag = self.__getFileBase64Tag(self.fileParams.base64EncFile)
        MakeMerkleTreetask = asyncio.create_task(self.__getMerkleTree())
        if await self.__verifyTag(tag) == False:
            print('save in cache')
            # fileDecDto = await self.__getDecryptedFileParams()
            await self.__saveInCache(tag, fileDecDto)
        else:
            fileChallenge = await self.__getChallenge(tag)
            print(fileChallenge)
            await MakeMerkleTreetask
            fileResp = getRespForchallenge(self.mt, fileChallenge, self.fileParams.base64EncFile)
            response = await self.gateWay.callBackendPostMethodDto("/api/File/verifyFileChallengeForProxy", self.authService.token, fileResp)
            print(response)
            if(response == False):
                raise Exception("File not verified")
            # fileDecDto = await self.__getDecryptedFileParams()
            await self.__saveDeduplicateFileForUser(tag, fileDecDto.fileName, base64key=fileDecDto.base64Key, base64iv=fileDecDto.base64Iv)

    async def __createPathForUserIfItDoesntExist(self, path):
        if not os.path.exists(path):
            os.makedirs(path)

    async def __writeFileOnDisk(self, fileToSave:UploadFile, base64tag):
        try:
            basedb = Base()
            session = basedb.getSession()
            user = session.query(User).filter(User.email==self.userEmail).first()
            if not user:
                self.__saveUser(session)
                user = session.query(User).filter(User.email==self.userEmail).first()
            file = session.query(File).filter(File.tag == base64tag).first()
            if not file:
                user_path = os.getcwd()+'/uploadedFiles/'+self.userEmail
                await self.__createPathForUserIfItDoesntExist(user_path)
                file_path = os.getcwd()+'/uploadedFiles/'+self.userEmail+'/'+self.filename
                
                with open(file_path, "wb") as buffer:
                    while True:
                        chunk = await fileToSave.read(1048576)
                        if not chunk:
                            break
                        buffer.write(chunk)
                        
                blob_file = BlobFile(filePath=file_path)
                new_file = File(tag=base64tag, blob_file=blob_file)
                session.add(new_file)
                session.commit()
                file = session.query(File).filter(File.tag == base64tag).first()
                
            new_user_file = UserFile(user_id=user.id, file_id=file.id, key=self.base64Key, iv=self.base64Iv, fileName=self.filename, date=datetime.datetime.utcnow())
            session.add(new_user_file)
            session.commit()
            session.close()
            print('finally')
        except Exception as e:
            print(str(e))
            raise Exception("Error writing file on disk")
        finally:
            session.close()
    
    async def computeFileVerification_v2(self, file:UploadFile, tag):        
        await self.authService.getProxyToken()
        await self.__getUserEmail()
       
        if self.__checkIfUserHasFile(self.filename) == True:
            raise Exception("User already has this file")
        if await self.__verifyTag(tag) == False:
            await self.__writeFileOnDisk(file, tag)
            
        else:
            MakeMerkleTreetask = asyncio.create_task(self.__getMerkleTree(file))
            fileChallenge = await self.__getChallenge(tag)
            await MakeMerkleTreetask
            fileResp = getRespForchallenge(self.mt, fileChallenge, self.filename)
            response = await self.gateWay.callBackendPostMethodDto("/api/File/verifyFileChallengeForProxy", self.authService.token, fileResp)

            if(response == False):
                raise Exception("File not verified")

            fileDecDto = FileDecDto(
                base64Key=self.base64Key,
                base64Iv=self.base64Iv,
                fileName=self.filename,
                tag=tag
            )
            await self.__saveDeduplicateFileForUser(tag, fileDecDto.fileName, base64key=fileDecDto.base64Key, base64iv=fileDecDto.base64Iv)

    async def __getFileNameAndDatesFromCache(self):
        try:
            basedb = Base()
            session = basedb.getSession()            
            #cod adauagat dupa modificare baza de date
            query = (
                session.query(User.email, UserFile.fileName, UserFile.upload_date)
                .join(UserFile, User.id == UserFile.user_id)
                .filter(User.email == self.userEmail)
            )
            result = query.all()
            files_and_dates = [(row.fileName, row.upload_date) for row in result]
            return files_and_dates
            #gata codul adaugat 
            
        except Exception as e:
            print(f"Error fetching files and dates: {e}")
            raise Exception("It was a problem, try later!")

    async def getKeyAndIvForFile(self, filename):
        await self.authService.getProxyToken()
        await self.__getUserEmail()
        try:
            basedb = Base()
            session = basedb.getSession() 
            result = session.query(UserFile.iv, UserFile.key).join(User, User.id == UserFile.user_id).filter(UserFile.fileName == filename, User.email == self.userEmail).first()  
            
            if result is not None:
                return FileKeyAndIvDto(base64key=result.key, base64iv=result.iv)
            
            result = await self.gateWay.callBackendPostMethodDto("/api/File/getKeyAndIvForFile", self.authService.token, EmailFilenameDto(userEmail=self.userEmail, fileName=filename))
            return FileKeyAndIvDto(**result.json())
            
        except Exception as e:
            print(str(e))
            raise Exception("Not good")
    
    async def getFilesNamesAndDates(self):
        await self.authService.getProxyToken()
        await self.__getUserEmail()
        FileNameAndDateFromCacheTask = asyncio.create_task(self.__getFileNameAndDatesFromCache())
        response = await self.gateWay.callBackendPostMethodWithSimpleParams("/api/File/getUploadedFileNamesAndDatesWithoutProxy",
                                                     self.authService.token,
                                                     self.userEmail)
        
        if(response.status_code != 200):
            raise Exception(response.text)
        filesList = [FilesNameDate(**item) for item in response.json()]
        filesAndDatesFromCache = await FileNameAndDateFromCacheTask
        filesList.extend([FilesNameDate(fileName=file_name, uploadDate=upload_date) for file_name, upload_date in filesAndDatesFromCache])
        return filesList
    
    async def getFilePathFromCache(self, filename:str):
        try:
            basedb = Base()
            session = basedb.getSession()
            result = session.query(UserFile.iv, UserFile.key, BlobFile.encFilePath).join(User, User.id == UserFile.user_id).join(File, File.id == UserFile.file_id).join(BlobFile, BlobFile.id == File.blob_file_id).filter(UserFile.fileName == filename, User.email == self.userEmail).first()
            
            if result == None:
                return None
            else:
                return result
        except Exception as e:
            raise e
    
    async def getFileFromStorage(self):
        await self.authService.getProxyToken()
        try:
            await self.__getUserEmail()
            fileFromCache = await self.getFilePathFromCache(self.filename)
            if fileFromCache == None:
                response = await self.gateWay.callBackendPostMethodDto("/api/File/proxyGetUrlFileFromStorage", self.authService.token, EmailFilenameDto(userEmail=self.userEmail, fileName=self.filename))
                if response.status_code != 200:
                    raise Exception(response.text)
                dto = BlobFileParamsDto(**response.json())
                download_path = os.path.join(os.getcwd(), "tmp", self.filename)
                download_blob(dto.base64tag, download_path)
                return download_path
            else:      
                return fileFromCache.encFilePath
        except Exception as e:
            raise e
    
    async def deleteFile(self):
        await self.authService.getProxyToken()
        await self.__getUserEmail()
        if self.__checkIfUserHasFile(filename=self.filename) == False:
            #trebuie sa interogam si backendul
            response = await self.gateWay.callBackendPostMethodDto("/api/File/deleteFile", self.authService.token, EmailFilenameDto(userEmail=self.userEmail, fileName=self.filename))
            if response.status_code != 200:
                raise Exception(response.text)
        else:
            basedb = Base()
            session = basedb.getSession()
            print(self.filename)
            user_file = session.query(UserFile).join(User, User.id == UserFile.user_id).filter(UserFile.fileName == self.filename).all()
            tag = session.query(File).join(UserFile, UserFile.file_id == File.id).filter(UserFile.fileName == self.filename).first().tag
            dif_name_same_file = session.query(UserFile).join(File, File.id == UserFile.file_id).filter(UserFile.fileName != self.filename, tag == File.tag).all()
            if len(user_file) > 1 or len(dif_name_same_file) > 0:
                uf = session.query(UserFile).join(User, User.id == UserFile.user_id).filter(UserFile.fileName == self.filename, User.email == self.userEmail).first()
                session.delete(uf)
                session.commit()
            else:
                # aici trebuie sters fisierul de tot
                uf = session.query(UserFile).join(User, User.id == UserFile.user_id).filter(UserFile.fileName == self.filename, User.email == self.userEmail).first()
                file = session.query(File).filter(File.id == uf.file_id).first()
                blob_file = session.query(BlobFile).filter(BlobFile.id == file.blob_file_id).first()
                session.delete(uf)
                session.delete(file)
                session.delete(blob_file)
                session.commit()
            return True
     
    async def getPubKeyAndFileKey(self):
        await self.authService.getProxyToken()
        await self.__getUserEmail()
        if self.__checkIfUserHasFile(filename=self.filename) == True:
            response = await self.gateWay.callBackendPostMethodWithSimpleParams("/api/User/getUserRsaPubKey", self.authService.token, self.recieverEmail)
            if(response.status_code != 200):
                raise Exception(response.text)
            rsaPubKey = response.text
            basedb = Base()
            session = basedb.getSession()
            uf = session.query(UserFile).join(User, User.id == UserFile.user_id).filter(UserFile.fileName == self.filename, User.email == self.userEmail).first()
            return RsaKeyFileKeyDto(pubKey=rsaPubKey, fileKey=uf.key, fileIv=uf.iv)
        else:
            response = await self.gateWay.callBackendPostMethodDto("/api/File/getPubKeyAndFileKey", self.userToken, EmailFilenameDto(userEmail=self.recieverEmail, fileName=self.filename)) 
            if response.status_code != 200:
                raise Exception(response.text)
            return RsaKeyFileKeyDto(**response.json())
        
                
    async def sendFile(self):
        await self.authService.getProxyToken()
        await self.__getUserEmail()
        if self.__checkIfUserHasFile(filename=self.filename) == True:
            # aici va trebui ori sa facem la nivel de proxy send=ul, ori sa trimitem care backend
            # verificam si daca persoana careia ii trimitem are deja fisierul
            aux = self.userEmail
            self.userEmail = self.recieverEmail
            if self.__checkIfUserHasFile(filename=self.filename) == True:
                raise Exception("User already has this file")
            self.userEmail = aux
            return True, self.userEmail
        else:
            print('ajungem oare aici')
            response = await self.gateWay.callBackendPostMethodDto("/api/File/sendFile", self.authService.token, FileTransferDto(senderToken=self.userToken, recieverEmail=self.recieverEmail, fileName=self.filename, base64EncKey=self.base64EncKey, base64EncIv=self.base64EncIv))
            if response.status_code != 200:
                raise Exception(response.text)
            return False, self.userEmail
    
    
    async def sendFilesToServer_v2(self):
        try:
            gateWay = ApiCall(os.environ.get("backendBaseUrl"))
            pc = ProxyClass()
            pc.getProxyTokenSYNC()
            basedb = Base()
            session = basedb.getSession()
            print('---------------------------------------')
            blobs = session.query(BlobFile).all()
            if len(blobs) == 0:
                return
            personalisedInfo = []
            for f in blobs:            
                file_path = f.encFilePath
                base64Tag = f.file[0].tag
                user_files = session.query(UserFile).filter(UserFile.file_id == f.file[0].id).all()
                personalisedInfo = []
                for uf in user_files:
                    user = session.query(User).filter(User.id == uf.user_id).first()
                    personalisedInfo.append(PersonalisedInfoDto(fileName=uf.fileName, base64key=uf.key, base64iv=uf.iv, email=user.email, UploadDate=str(uf.upload_date)))
                filesMetaDto = FileFromCacheDto_v2(encFilePath=file_path, base64Tag=base64Tag, personalisedList=personalisedInfo)
                response = gateWay.sendChunkFromFile("/api/File/writeFileOnDisk", "/api/File/saveFileFromCacheParams",pc.token, f.encFilePath, filesMetaDto)
                
                if response.status_code != 200:
                    raise Exception(response.text)
                  
            files = session.query(File).all()
            blob_files = session.query(BlobFile).all()
            user_files = session.query(UserFile).all()

            for user_file in user_files:
                session.delete(user_file)

            for file in files:
                session.delete(file)

            for blob_file in blob_files:
                session.delete(blob_file)
                
            session.commit()

        except Exception as e:
            print(f"Error fetching files and dates: {str(e)}")
            raise Exception("Not good")
        finally:
            session.close()
            print('finally')