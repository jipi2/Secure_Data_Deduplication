using CryptoLib;
using FileStorageApp.Server.Entity;
using FileStorageApp.Server.Repositories;
using FileStorageApp.Shared;
using FileStorageApp.Shared.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;

namespace FileStorageApp.Server.Services
{
    public class FileService
    {
        public UserFileRepo _userFileRepo { get; set; }
        public FileRepository _fileRepo { get; set; }
        public RespRepository _respRepo { get; set; }
        public UserService _userService { get; set; }
        public UserRepository _userRepo { get; set; }
        public AzureBlobService _azureBlobService { get; set; } 
        public FileTransferRepo _fileTransferRepo { get; set; }
        IConfiguration _configuration { get; set; }
        public FileService(FileRepository fileRepository, UserService userService, AzureBlobService azureBlobService, IConfiguration configuration, RespRepository respRepo, UserRepository userRepo, UserFileRepo userFileRepo, FileTransferRepo fileTransferRepo)
        {
            _fileRepo = fileRepository;
            _userService = userService;
            _azureBlobService = azureBlobService;
            _configuration = configuration;
            _respRepo = respRepo;
            _userRepo = userRepo;
            _userFileRepo = userFileRepo;
            _fileTransferRepo = fileTransferRepo;
        }

        public async Task<DFparametersDto> GetDFParameters(string Userid)
        {
            try
            {
                User user = await _userService.GetUserById(Userid);

                DHParameters parameters = Utils.GenerateParameters();
                AsymmetricCipherKeyPair serverKeys = Utils.GenerateDFKeys(parameters);

                user.ServerDHPrivate = Utils.GetPemAsString(serverKeys.Private);
                user.ServerDHPublic = Utils.GetPemAsString(serverKeys.Public);

                await _userService.SaveServerDFKeysForUser(user, serverKeys, Utils.GetP(parameters), Utils.GetG(parameters));

                DFparametersDto dFparams = new DFparametersDto
                {
                    G = Utils.GetG(parameters),
                    P = Utils.GetP(parameters),
                    ServerPubKey = Utils.GetPublicKey(serverKeys)
                };

                return dFparams;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> DFkeyExchange(string pubKey, string userId)
        {
            try
            {
                string stringPrivKey = await _userService.GetPrivateKeyOfServerForUser(userId);
              
                AsymmetricKeyParameter privKey = (AsymmetricKeyParameter)Utils.ReadPrivateKeyFromPemString(stringPrivKey);

                Dictionary<string, string> stringParams = await _userService.GetParametersForDF(userId);
                DHParameters parameters = Utils.GenerateParameters(stringParams["G"], stringParams["P"]);

                //calculam secretul in continuare
                Org.BouncyCastle.Math.BigInteger serverSecret = Utils.ComputeSharedSecret(pubKey, privKey, parameters);
                byte[] byteSecret = serverSecret.ToByteArray();
                byte[] symKey = Utils.ComputeHash(byteSecret);
                await _userService.SaveSymKeyForUser(userId, symKey);
                return true;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        public async Task<ServerBlobFIle>? GetFileFromBlob(string userId, string fileName)
        {
            User user = await _userService.GetUserById(userId);
            UserFile userFile = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, fileName);
            //FileMetadata file = user.Files.Where(f => f.Id == userFile.FileId).FirstOrDefault();
            FileMetadata? file = await _fileRepo.GetFileMetaById(userFile.FileId);

            string base64file = await _azureBlobService.GetContentFileFromBlob(file.Tag);

            ServerBlobFIle serverFile = new ServerBlobFIle
            {
                FileName = Utils.EncryptAes(Encoding.UTF8.GetBytes(fileName), Convert.FromBase64String(user.SymKey)),
                FileKey = Utils.EncryptAes(Convert.FromBase64String(userFile.Key), Convert.FromBase64String(user.SymKey)),
                FileIv = Utils.EncryptAes(Convert.FromBase64String(userFile.Iv), Convert.FromBase64String(user.SymKey)),
                EncBase64File = base64file
            };

            return serverFile;
        }

        public async Task<string> GetFileFromBlobWithoutEncryption(string base64tag)
        {
            string base64file = await _azureBlobService.GetContentFileFromBlob(base64tag);
            return base64file;
        }

        private async Task GenerateMerkleTreeChallenges(int fileMetadataId, MerkleTree mt)
        {
            int J = Convert.ToInt32(_configuration["J"]);
            List<Resp> respList = new List<Resp>(J);
            int pos1=0, pos2=0, pos3=0;
            byte[] answ = new byte[mt.HashTree[0]._hash.Length];

            for (int i = 0; i < J; i++)
            {
                answ = Utils.GenerateResp(mt, ref pos1, ref pos2, ref pos3);
                respList.Add(new Resp
                {
                    Position_1 = pos1,
                    Position_2 = pos2,
                    Position_3 = pos3,
                    Answer = Convert.ToBase64String(answ),
                    wasUsed = false,
                    FileMetadataId = fileMetadataId
                });
            }
            await _respRepo.SaveMultipleResps(respList);
            
        }

        private async Task<MerkleTree> GetMerkleTree(string base64EncFile)
        {
            byte[] bytesEncFile = Convert.FromBase64String(base64EncFile);
            Stream stream = new MemoryStream(bytesEncFile);
            MerkleTree mt = Utils.GetMerkleTree(stream);
            Console.WriteLine(mt.HashTree.Count);
            return mt;
        }

        private async Task<MerkleTree> GetMerkleTree_v2(string filePath)
        {
            MerkleTree mt = Utils.GetMerkleTreeFromFilePath(filePath);
            Console.WriteLine(mt.HashTree.Count);
            return mt;
        }

        private async Task SaveBlob(string base64EncFile, string base64Tag, string fileName, string base64Key, string base64Iv, string userId)
        {
            string? blobUrl = await _azureBlobService.UploadFileToCloud(base64EncFile, base64Tag);

            if (blobUrl == null)
            {
                throw new Exception("Url for Azure Blob Stroage is null");
            }

            //FileMetadata fileMeta = new FileMetadata
            //{
            //    FileName = fileName,
            //    BlobLink = blobUrl,
            //    isDeleted = false,
            //    Key = base64Key,
            //    Iv = base64Iv,
            //    Tag = base64Tag,
            //    UploadDate = DateTime.UtcNow
            //};

            //cod modificat

            FileMetadata fileMeta = new FileMetadata
            {
                BlobLink = blobUrl,
                isDeleted = false,
                Tag = base64Tag
            };

            //gata cod modificat

            await _fileRepo.SaveFile(fileMeta);

            //si am mai adaugat asta

            UserFile userFile = new UserFile
            {
                FileName = fileName,
                Key = base64Key,
                Iv = base64Iv,
                UploadDate = DateTime.UtcNow,
                FileId = fileMeta.Id,
                UserId = Convert.ToInt32(userId)
            };

          
            await _userFileRepo.SaveUserFile(userFile);
            //gata cu adaugatul
            
/*            await _userService.AddFile(userId, fileMeta);*/
            MerkleTree mt = await GetMerkleTree(base64EncFile);
            await GenerateMerkleTreeChallenges(fileMeta.Id, mt);
        }

        private async Task<bool> CheckIfFileNameExists(User user, string fileName)
        {
            //if (user.Files == null)
            //    return false;
            //foreach (FileMetadata file in user.Files)
            //{
            //    if (file.FileName.Equals(fileName))
            //        return true;
            //}
            //return false;

            //cod modficat
            UserFile uf = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, fileName);
            if(uf == null)
                return false;
            return true;

            //gata cod modificat
        
        }   
        public async Task<FileMetaChallenge?> ComputeFileMetadata(FileParamsDto fileParams, string userId)
        {
            try
            {
                User user = await _userService.GetUserById(userId);
                string base64SymKey;
                if (user.SymKey != null)
                    base64SymKey = user.SymKey;
                else
                    return null;

                string base64Tag = Utils.DecryptAes(Convert.FromBase64String(fileParams.base64TagEnc), Convert.FromBase64String(base64SymKey));
                string base64Key = Utils.DecryptAes(Convert.FromBase64String(fileParams.base64KeyEnc), Convert.FromBase64String(base64SymKey));
                string base64Iv = Utils.DecryptAes(Convert.FromBase64String(fileParams.base64IvEnc), Convert.FromBase64String(base64SymKey));
                string fileName = Encoding.UTF8.GetString( Convert.FromBase64String( Utils.DecryptAes(Convert.FromBase64String(fileParams.encFileName), Convert.FromBase64String(base64SymKey))));

                FileMetaChallenge fmc = new FileMetaChallenge();
                fmc.id = "";

                if(await CheckIfFileNameExists(user, fileName) == true)
                {
                    fmc.id = "File name already exists!";
                    return fmc;
                }

                if (await _fileRepo.GetFileMetaByTag(base64Tag) == false)
                {

                    await SaveBlob(fileParams.base64EncFile, base64Tag, fileName, base64Key, base64Iv, userId);
                   
                }      
                
                FileMetadata? fileMeta = await _fileRepo.GetFileMetaWithResp(base64Tag);
                if (fileMeta == null)
                    return null;

                //aici am adaugat cod
                UserFile uf = new UserFile
                {
                    FileName = fileName,
                    Key = base64Key,
                    Iv = base64Iv,
                    UploadDate = DateTime.UtcNow,
                    UserId = Convert.ToInt32(userId)
                };
                await _userFileRepo.SaveUserFile(uf);
                //aici gata codul

                Resp? resp = fileMeta.Resps.Where(r => r.wasUsed == false).FirstOrDefault();
                if(resp == null)
                {
                    //reload the challenges
                    ServerBlobFIle blobFile = await GetFileFromBlob(userId, fileName);
                    MerkleTree mt = await GetMerkleTree(blobFile.EncBase64File);
                    await GenerateMerkleTreeChallenges(fileMeta.Id, mt);
                    resp = fileMeta.Resps.Where(r => r.wasUsed == false).FirstOrDefault();
                }
                fmc.id = Utils.EncryptAes(BitConverter.GetBytes(resp.Id), Convert.FromBase64String(base64SymKey));
                fmc.n1 = Utils.EncryptAes(BitConverter.GetBytes(resp.Position_1), Convert.FromBase64String(base64SymKey));
                fmc.n2 = Utils.EncryptAes(BitConverter.GetBytes(resp.Position_2), Convert.FromBase64String(base64SymKey));
                fmc.n3 = Utils.EncryptAes(BitConverter.GetBytes(resp.Position_3), Convert.FromBase64String(base64SymKey));
                resp.wasUsed = true;
                await _respRepo.UpdateResp(resp);
                return fmc;            
               
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<bool> CheckEncTag(string userId, string encTag)
        {
            User user = await _userService.GetUserById(userId);
            string base64SymKey;
            if (user.SymKey != null)
                base64SymKey = user.SymKey;
            else
                throw new Exception("Problems with Crypto params!");
            string base64Tag = Utils.DecryptAes(Convert.FromBase64String(encTag), Convert.FromBase64String(base64SymKey));
            if(await _fileRepo.GetFileMetaByTag(base64Tag) == false)
            {
                return false;
            }
            return true;

        }
        public async Task<bool> SaveFileToUser(string userId, FileResp userResp)
        {
            try
            {
                User user = await _userService.GetUserById(userId);
                string base64SymKey;
                if (user.SymKey != null)
                    base64SymKey = user.SymKey;
                else
                    return false;

                byte[] symKey = Convert.FromBase64String(base64SymKey);

                int respId = BitConverter.ToInt32(Convert.FromBase64String(Utils.DecryptAes(Convert.FromBase64String(userResp.Id), symKey)));
                Resp? resp = await _respRepo.GetRespById(Convert.ToInt32(respId));

                string userFilename = Encoding.UTF8.GetString(Convert.FromBase64String(Utils.DecryptAes(Convert.FromBase64String(userResp.FileName), symKey)));
                string userAnsw = Utils.DecryptAes(Convert.FromBase64String(userResp.Answer), symKey);

                if (resp == null) return false;
                if (resp.Answer.Equals(userAnsw))
                {
                    FileMetadata? fileMeta = await _fileRepo.GetFileMetaById(resp.FileMetadataId);
                    if (fileMeta == null)
                        return false;

                    UserFile? uf = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, userFilename);
                    if (uf != null)
                    {
                        uf.FileId = fileMeta.Id;
                        await _userFileRepo.UpdateUserFile(uf);
                    }

                    return true;
                }
                else
                {
                    UserFile userFile = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, userFilename);
                    _userFileRepo.DeleteUserFile(userFile);

                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        public async Task<List<FilesNameDate>?> GetFileNamesAndDatesOfUser(string id)
        {
            User? user = await _userService.GetUserById(id);
            if (user == null)
                return null;

            //List<FilesNameDate>? result = user.Select(fileMeta => new FilesNameDate
            //{
            //    FileName = fileMeta.FileName,
            //    UploadDate = fileMeta.UploadDate,

            //}).ToList();

            //am modificat codul
            List<UserFile>? list = await _userFileRepo.GetUserFileByUserId(Convert.ToInt32(id));
            List<FilesNameDate> result = new List<FilesNameDate>();


            //codul asta era, nu-l modifica
            if (list == null)
            {
                result = new List<FilesNameDate>();
                result.Add(new FilesNameDate
                {
                    FileName = "No Files.",
                    UploadDate = DateTime.UtcNow
                });
            }
            //gata codul care era, restul poti modifica
            else
            {
                foreach (UserFile uf in list)
                {
                    result = list.Select(uf => new FilesNameDate
                    {
                        FileName = uf.FileName,
                        UploadDate = uf.UploadDate

                    }).ToList();
                }
            }

            return result;
        }

        //proxy logic
       public async Task<bool> CheckTagAvailabilityInCloud(string base64tag)
        {
            return await _fileRepo.GetFileMetaByTag(base64tag);
        }

        public async Task<FileMetaChallenge?> GetChallengeForTag(string base64Tag)
        {
            try
            {
                //string base64encFile = await GetFileFromBlobWithoutEncryption(base64Tag);
                FileMetadata fileMeta = await _fileRepo.GetFileMetaWithResp(base64Tag);
                int random = (new Random()).Next(10); //aici va trebui modificat si sa pui 100
                List<Resp?> resp = fileMeta.Resps.Where(r => r.wasUsed == false).ToList<Resp>(); 
                if (resp == null)
                {
                    //reload the challenges
                    //aici va trebui sa completam noi
                    //MerkleTree mt = await GetMerkleTree(base64encFile);
                    //await GenerateMerkleTreeChallenges(fileMeta.Id, mt);
                    //resp = fileMeta.Resps.Where(r => r.wasUsed == false).FirstOrDefault();
                }
                FileMetaChallenge fmc = new FileMetaChallenge();
                fmc.id = resp[random].Id.ToString();
                fmc.n1 = resp[random].Position_1.ToString();
                fmc.n2 = resp[random].Position_2.ToString();
                fmc.n3 = resp[random].Position_3.ToString();
/*                resp.wasUsed = true;*/
                await _respRepo.UpdateResp(resp[random]);
                return fmc;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task<bool> VerifyChallengeResponseFromProxy(FileResp proxyResp)
        {
            try
            {
                int respId = Int32.Parse(proxyResp.Id);
                Resp? resp = await _respRepo.GetRespById(Convert.ToInt32(respId));

                if (resp == null) return false;
                if (resp.Answer.Equals(resp.Answer))
                {
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public async Task<FileDecDataDto> GetDecryptedFileParams(FileEncDataDto fileEncData)
        {
            FileDecDataDto fileDecData = new FileDecDataDto();

            User? user = await _userRepo.GetUserByEmail(fileEncData.userEmail);
            if (user == null)
                throw new Exception("User does not exist!");

            fileDecData.tag = Utils.DecryptAes(Convert.FromBase64String(fileEncData.encBase64Tag), Convert.FromBase64String(user.SymKey));
            fileDecData.base64key = Utils.DecryptAes(Convert.FromBase64String(fileEncData.base64KeyEnc), Convert.FromBase64String(user.SymKey));
            fileDecData.base64iv = Utils.DecryptAes(Convert.FromBase64String(fileEncData.base64IvEnc), Convert.FromBase64String(user.SymKey));
            fileDecData.fileName = Encoding.UTF8.GetString(Convert.FromBase64String(Utils.DecryptAes(Convert.FromBase64String(fileEncData.encFileName), Convert.FromBase64String(user.SymKey))));
            
   
            if (await CheckIfFileNameExists(user, fileDecData.fileName) == true)
                throw new Exception("User has allready a file with this name!");

            return fileDecData;
        }


        public async Task SaveDedupFile(FileDedupDto fileDedupDto)
        {
            User? user = await _userRepo.GetUserByEmail(fileDedupDto.userEmail);
            if(user == null)
                throw new Exception("User does not exist!");

            FileMetadata? fileMeta = await _fileRepo.GetFileMetaByTagIfExists(fileDedupDto.base64tag);
            if (fileMeta == null)
                throw new Exception("The file with this tag does not exists!");

            UserFile? uf = new UserFile
            {
                FileName = fileDedupDto.fileName,
                Key = fileDedupDto.base64key,
                Iv = fileDedupDto.base64iv,
                UploadDate = DateTime.UtcNow,
                UserId = user.Id,
                FileId = fileMeta.Id
            };

            await _userFileRepo.SaveUserFile(uf);
        }

        public async Task SaveFileFromCache(FileFromCacheDto cacheFile)
        {
            string? blobUrl = await _azureBlobService.UploadFileToCloud(cacheFile.base64EncFile, cacheFile.base64Tag);

            if (blobUrl == null)
            {
                throw new Exception("Url for Azure Blob Stroage is null");
            }
            List<string> fileNamesUsed = new List<string>();

            FileMetadata fileMeta = new FileMetadata
            {
                BlobLink = blobUrl,
                isDeleted = false,
                Tag = cacheFile.base64Tag
            };
            await _fileRepo.SaveFile(fileMeta);
            MerkleTree mt = await GetMerkleTree(cacheFile.base64EncFile);
            await GenerateMerkleTreeChallenges(fileMeta.Id, mt);

            foreach (PersonalisedInfoDto pid in cacheFile.personalisedList)
            {
                User? user = await _userRepo.GetUserByEmail(pid.email);
                if (user == null)
                    throw new Exception("User does not exist!");

                UserFile uf = new UserFile
                {
                    FileName = pid.fileName,
                    Key = pid.base64key,
                    Iv = pid.base64iv,
                    UploadDate = DateTime.Parse(pid.UploadDate),
                    UserId = user.Id,
                    FileId = fileMeta.Id
                };

                await _userFileRepo.SaveUserFile(uf);
            }
        }

        private string GetFilePathByTag(string base64Tag)
        {
            string base64TagForFilePath = base64Tag.Replace("/", "_");
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            string filePath = Path.Combine(uploadsFolder, base64TagForFilePath);
            return filePath;

        }

        public async Task SaveFileFromCache(FileFromCacheDto_v2 fileParams)
        {
            string? blobUrl = await _azureBlobService.GetUri(fileParams.base64Tag);
            if (blobUrl == null)
            {
                throw new Exception("Url for Azure Blob Stroage is null");
            }

            FileMetadata fileMeta = new FileMetadata
            {
                BlobLink = blobUrl,
                isDeleted = false,
                Tag = fileParams.base64Tag
            };

           string filePath = GetFilePathByTag(fileParams.base64Tag);

            await _fileRepo.SaveFile(fileMeta);
            MerkleTree mt = await GetMerkleTree_v2(filePath);
            await GenerateMerkleTreeChallenges(fileMeta.Id, mt);

            foreach (PersonalisedInfoDto pid in fileParams.personalisedList)
            {
                User? user = await _userRepo.GetUserByEmail(pid.email);
                if (user == null)
                    throw new Exception("User does not exist!");

                UserFile uf = new UserFile
                {
                    FileName = pid.fileName,
                    Key = pid.base64key,
                    Iv = pid.base64iv,
                    UploadDate = DateTime.Parse(pid.UploadDate),
                    UserId = user.Id,
                    FileId = fileMeta.Id
                };

                await _userFileRepo.SaveUserFile(uf);
            }
            
            
        }

        public async Task WriteFileOnDiskAndOnCloud(IFormFile file, string base64Tag)
        {
            string? blobUrl = await _azureBlobService.UploadFileToCloud_v2(file, base64Tag);
            if (blobUrl == null)
            {
                throw new Exception("Url for Azure Blob Stroage is null");
            }
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            //va trebui sa modificam tagul sa nu ave "/"
            string base64tagForFilePath = base64Tag.Replace("/", "_");
            string filePath = Path.Combine(uploadsFolder, base64tagForFilePath);
           
            using (var stream = new FileStream(filePath, FileMode.Append))
            {
                await file.CopyToAsync(stream);
            }

        }

        public async Task<EncryptParamsDto> encryptParams(string userId, EncryptParamsDto paramsDto)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            paramsDto.fileName = Utils.EncryptAes(Encoding.UTF8.GetBytes(paramsDto.fileName), Convert.FromBase64String(user.SymKey));
            paramsDto.fileKey = Utils.EncryptAes(Convert.FromBase64String(paramsDto.fileKey), Convert.FromBase64String(user.SymKey));
            paramsDto.fileIv = Utils.EncryptAes(Convert.FromBase64String(paramsDto.fileIv), Convert.FromBase64String(user.SymKey));

            return paramsDto;
        }

        public async Task DeleteFile(string userId, string fileName)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            UserFile? userFile = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, fileName);
            if (userFile == null)
                throw new Exception("File does not exist!");
            List<UserFile>? luf = await _userFileRepo.GetUserFilesByFileId(userFile.FileId);
            if(luf == null)
                throw new Exception("File does not exist!");
            if(luf.Count > 1)
            {
                await _userFileRepo.DeleteUserFile(userFile);
            }
            else
            {
                //aici trebuie sa stergem de pe peste tot
                FileMetadata? fileMeta = await _fileRepo.GetFileMetaById(userFile.FileId);
                bool res = await _azureBlobService.DeleteFileFromCloud(fileMeta.Tag);
                if (res == false)
                {
                    throw new Exception("Could not delete from cloud");
                }
                await _userFileRepo.DeleteUserFile(userFile);
                await _fileRepo.DeleteFile(fileMeta);
            }
        }

        public async Task SendFile(FileTransferDto ftdto, string senderid)
        {
            User? sender = await _userRepo.GetUserById(Convert.ToInt32(senderid));
            User? reciever = await _userRepo.GetUserByEmail(ftdto.recieverEmail);
            if(reciever == null)
                throw new Exception("Reciever does not exist!");
            if (sender == null)
                throw new Exception("User does not exist!");
            if(await CheckIfFileNameExists(sender, ftdto.fileName) == false)
                throw new Exception("The sender does not have that file!");
            if (sender == reciever)
                throw new Exception("You cannot send a file to yourself!");

            UserFile? ufrec = await _userFileRepo.GetUserFileByUserIdAndFileName(reciever.Id, ftdto.fileName);
            if (ufrec != null)
                throw new Exception("Reciever already has this file!");

            FileTransfer fte = await _fileTransferRepo.GetFileTransferBySendIdRecIdFilename(sender.Id, reciever.Id, ftdto.fileName);
            if (fte != null)
                throw new Exception("You already sent this file to this uer");

            FileTransfer ft = new FileTransfer()
            {
                ReceiverId = reciever.Id,
                SenderId = sender.Id,
                FileName = ftdto.fileName,
                base64EncKey = ftdto.base64EncKey,
                base64EncIv = ftdto.base64EncIv,
                isDeleted = false
            };
            await _fileTransferRepo.SaveFileTransfer(ft);
        }

        public async Task<RsaKeyFileKeyDto> GetRsaPubKeyAndFileKey(EmailFilenameDto ef, string id)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(id));
            if(user == null)
                throw new Exception("User does not exist!");
            User? reciever = await _userRepo.GetUserByEmail(ef.userEmail);
            if(reciever == null)
                throw new Exception("Reciever does not exist!");
            UserFile? uf = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, ef.fileName);        
            if(uf == null)
                throw new Exception("File does not exist!");

            RsaKeyFileKeyDto rsaKeyFileKeyDto = new RsaKeyFileKeyDto
            {
                pubKey = reciever.Base64RSAPublicKey,
                fileKey = uf.Key,
                fileIv = uf.Iv
            };
            return rsaKeyFileKeyDto;
        }

        public async Task<List<RecievedFilesDto>?> GetRecievedFiles(string id)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(id));
            if (user == null)
                throw new Exception("User does not exist!");
            List<RecievedFilesDto> lrfd = new List<RecievedFilesDto>();
            List<FileTransfer>? listFileTransfer = await _fileTransferRepo.GetFileTransferByRecId(user.Id);
            foreach(FileTransfer ft in listFileTransfer)
            {
                User? sender = await _userRepo.GetUserById(Convert.ToInt32(ft.SenderId));
                lrfd.Add(new RecievedFilesDto
                {
                    senderEmail =  sender.Email,
                    fileName = ft.FileName,
                    base64EncKey = ft.base64EncKey,
                    base64EncIv = ft.base64EncIv
                });
            }

            return lrfd;
        }

        public async Task RemoveRecievedFile(RecievedFilesDto rfd, string id)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(id));
            if (user == null)
                throw new Exception("User does not exist!");

            User? sender = await _userRepo.GetUserByEmail(rfd.senderEmail);
            if (sender == null)
                throw new Exception("Sender does not exist!");

            FileTransfer? ft = await _fileTransferRepo.GetFileTransferBySendIdRecIdFilename(sender.Id, user.Id, rfd.fileName);
            if (ft == null)
                throw new Exception("File transfer does not exist!");
            await _fileTransferRepo.DeleteFileTransfer(ft);
        }

        public async Task AcceptRecievedFile(AcceptFileTransferDto aft, string id)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(id));
            if (user == null)
                throw new Exception("User does not exist!");

            User? sender = await _userRepo.GetUserByEmail(aft.senderEmail);
            if (sender == null)
                throw new Exception("Sender does not exist!");

            UserFile? uf = await _userFileRepo.GetUserFileByUserIdAndFileName(sender.Id, aft.fileName);
            if (uf == null)
                throw new Exception("Erro file");
            
            UserFile userFile = new UserFile
            {
                FileName = aft.fileName,
                Key = aft.base64FileKey,
                Iv = aft.base64FileIv,
                UploadDate = DateTime.UtcNow,
                UserId = user.Id,
                FileId = uf.FileId
            };
            await _userFileRepo.SaveUserFile(userFile);

            FileTransfer? ft = await _fileTransferRepo.GetFileTransferBySendIdRecIdFilename(sender.Id, user.Id, aft.fileName);
            await _fileTransferRepo.DeleteFileTransfer(ft);
        }

        public async Task<BlobFileParamsDto> GetUrlFileFromStorage(string userId, string fileName)
        {
            User user = await _userService.GetUserById(userId);
            UserFile userFile = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, fileName);
;
            FileMetadata? file = await _fileRepo.GetFileMetaById(userFile.FileId);

            BlobFileParamsDto serverFile = new BlobFileParamsDto
            {
                FileName = Utils.EncryptAes(Encoding.UTF8.GetBytes(fileName), Convert.FromBase64String(user.SymKey)),
                FileKey = Utils.EncryptAes(Convert.FromBase64String(userFile.Key), Convert.FromBase64String(user.SymKey)),
                FileIv = Utils.EncryptAes(Convert.FromBase64String(userFile.Iv), Convert.FromBase64String(user.SymKey)),
                base64tag = file.Tag
            };

            return serverFile;
        }

        public async Task<FileKeyAndIvDto> GetKeyAndIvForFile(string userId, string fileName)
        {
            User user = await _userService.GetUserById(userId);
            UserFile userFile = await _userFileRepo.GetUserFileByUserIdAndFileName(user.Id, fileName);

            FileKeyAndIvDto dto = new FileKeyAndIvDto
            {
                base64key = userFile.Key,
                base64iv = userFile.Iv
            };
            return dto;
        }
    }
}
