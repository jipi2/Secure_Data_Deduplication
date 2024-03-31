using CryptoLib;
using DesktopApp.Dto;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using System.Formats.Asn1;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace FileStorageApp.Client
{
    public class CryptoService
    {
        public async Task<CryptoParams> GenerateKeys(DFparametersDto dfParams)
        {

            DHParameters parameters = Utils.GenerateParameters(dfParams.G, dfParams.P);
            AsymmetricCipherKeyPair keys = Utils.GenerateDFKeys(parameters);
           // Dictionary<string, string> stringKeys = new Dictionary<string, string>();
           CryptoParams stringKeys = new CryptoParams();

            BigInteger bigSymKey = Utils.ComputeSharedSecret(dfParams.ServerPubKey, keys.Private, parameters);
            byte[] byteSymKey = bigSymKey.ToByteArray();
           
            stringKeys.SymKey =  Convert.ToBase64String(Utils.ComputeHash(byteSymKey));
            stringKeys.Public = Utils.GetPemAsString(keys.Public);
            stringKeys.Private = Utils.GetPemAsString(keys.Private);
            stringKeys.A =  Utils.GetPublicKey(keys);
           
            return stringKeys;
        }

        public async Task<string> GetHashInBase64(byte[] text, int hashSize)
        {
            byte[] h = Utils.ComputeHash(text, hashSize);
            return await Task.FromResult(Convert.ToBase64String(h));
        }

        public async Task<byte[]> ExtractIv(byte[] keyAndIv, int lenKey, int lenIv)
        {
            byte[] iv = new byte[lenIv];
            Array.Copy(keyAndIv, lenKey, iv, 0, lenIv);
            return iv;
        }
        public async Task<byte[]> ExtractKey(byte[] keyAndIv, int lenKey)
        {
            byte[] key = new byte[lenKey];
            Array.Copy(keyAndIv, key, lenKey);
            return key;
        }
        public async Task<string> GetEcnryptedFileBase64(string base64FileKey, string base64IvFile,byte[] byteFile)     //generam aici iv-ul
        {
            byte[] key = Convert.FromBase64String(base64FileKey);
            byte[] iv = Convert.FromBase64String(base64IvFile);
            string cipherText = Utils.EncryptAesWithIv(byteFile, key, iv);

            return await Task.FromResult(cipherText);
        }

        public async Task<byte[]> GetDecryptedFile(string encBase64File, string base64FileKey, string base64IvFile)
        {
            byte[] key = Convert.FromBase64String(base64FileKey);
            byte[] iv = Convert.FromBase64String(base64IvFile);

           string fileContent = Utils.DecryptAesWithIv(encBase64File, key, iv);
           return await Task.FromResult(Convert.FromBase64String(fileContent));
        }

        //public async Task<FileParamsDto> GetEncryptedFileParameters(string base64Tag, string base64Key, string base64Iv, string base64SymKey, string fileName)
        //{
        //    FileParamsDto fileParams = new FileParamsDto();
        //    string base64TagEnc = Utils.EncryptAes(Convert.FromBase64String(base64Tag), Convert.FromBase64String(base64SymKey));
        //    string base64KeyEnc = Utils.EncryptAes(Convert.FromBase64String(base64Key), Convert.FromBase64String(base64SymKey));
        //    string base64IvEnc = Utils.EncryptAes(Convert.FromBase64String(base64Iv), Convert.FromBase64String(base64SymKey));
        //    string encFileName = Utils.EncryptAes(Encoding.UTF8.GetBytes(fileName), Convert.FromBase64String(base64SymKey));

        //    fileParams.base64TagEnc = base64TagEnc;
        //    fileParams.base64KeyEnc = base64KeyEnc;
        //    fileParams.base64IvEnc = base64IvEnc;
        //    fileParams.encFileName =  encFileName;

        //    return await Task.FromResult(fileParams);
        //}

        public string? DecryptString(byte[] ciphertext, byte[] key, byte[] iv = null)
        {

            try
            {
                string plaintext;
                if (iv != null)
                    plaintext = Utils.DecryptAes(ciphertext, key, iv);
                else
                    plaintext = Utils.DecryptAes(ciphertext, key);

                return plaintext;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            
        }
        private async Task<MerkleTree> GetMerkleTree(string base64EncFile)
        {
            byte[] bytesEncFile = Convert.FromBase64String(base64EncFile);
            Stream stream = new MemoryStream(bytesEncFile);
            MerkleTree mt = Utils.GetMerkleTree(stream);
            Console.WriteLine(mt.HashTree.Count);
            return mt;
        }
        public async Task<FileResp> GetFileResp(FileMetaChallenge fmc, string encBase64File, string fileName, string base64SymKey)
        {
            MerkleTree mt = await GetMerkleTree(encBase64File);
            byte[] answ = new byte[mt.HashTree[0]._hash.Length];
            byte[] byteSymKey = Convert.FromBase64String(base64SymKey);

           // int Id = BitConverter.ToInt32(Convert.FromBase64String( Utils.DecryptAes(Convert.FromBase64String(fmc.id), byteSymKey)));
            int n1 = BitConverter.ToInt32(Convert.FromBase64String( Utils.DecryptAes(Convert.FromBase64String(fmc.n1), byteSymKey)));
            int n2 = BitConverter.ToInt32(Convert.FromBase64String( Utils.DecryptAes(Convert.FromBase64String(fmc.n2), byteSymKey)));
            int n3 = BitConverter.ToInt32(Convert.FromBase64String(Utils.DecryptAes(Convert.FromBase64String(fmc.n3), byteSymKey)));

            for (int i = 0; i < answ.Length; i++)
            {
                answ[i] = (byte)(mt.HashTree[n1]._hash[i] ^ mt.HashTree[n2]._hash[i] ^ mt.HashTree[n3]._hash[i]);
            }

            FileResp fr = new FileResp
            {
                Answer = Utils.EncryptAes(answ, byteSymKey),
                Id = fmc.id,
                FileName = Utils.EncryptAes(Encoding.UTF8.GetBytes(fileName), byteSymKey)
            };
            return fr;
        }

        public async Task<RsaDto?> GetRsaDto(string password)
        {
            try
            {
                byte[] key = new byte[16];
                byte[] iv = new byte[16];

                AsymmetricCipherKeyPair userKeyPair = Utils.GenerateRSAKeyPair();
                string base64PubKey = Utils.GetBase64RSAPublicKey(userKeyPair);
                byte[] privateKeyDerEnc = Utils.GetDerEncodedRSAPrivateKey(userKeyPair);
                //facem hash 256 peste parola, jumate va fi cheia jumate va fi iv-ul
                byte[] hash = Utils.ComputeHash(Encoding.UTF8.GetBytes(password));
                Array.Copy(hash, key, 16);
                Array.Copy(hash, hash.Length - 16, iv, 0, 16);

                string base64EncPrivKey = Utils.EncryptAes(privateKeyDerEnc, key, iv);
                RsaDto rsaDto = new RsaDto();
                rsaDto.base64PubKey = base64PubKey;
                rsaDto.base64EncPrivKey = base64EncPrivKey;
                return rsaDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        public async Task<Tuple<string, string>> GetRsaEncKeyAndIv(string base64PubKey, string base64fileKey, string base64fileIv)
        {
            byte[] fileKey = Convert.FromBase64String(base64fileKey);
            byte[] fileIv = Convert.FromBase64String(base64fileIv);

            string base64EncRsaFileKey = Convert.ToBase64String(Utils.EncryptRSAwithPublicKey(fileKey, base64PubKey));
            string base64EncRsaFileIv = Convert.ToBase64String(Utils.EncryptRSAwithPublicKey(fileIv, base64PubKey));

            return new Tuple<string, string>(base64EncRsaFileKey, base64EncRsaFileIv);
        }

        public byte[] GetRsaDecKry(string base64EncRsaPrivKey, string password)
        {
            try
            {
                byte[] key = new byte[16];
                byte[] iv = new byte[16];
                byte[] hash = Utils.ComputeHash(Encoding.UTF8.GetBytes(password));
                Array.Copy(hash, key, 16);
                Array.Copy(hash, hash.Length - 16, iv, 0, 16);

                string base64rsakey = Utils.DecryptAes(Convert.FromBase64String(base64EncRsaPrivKey), key, iv);
                return Convert.FromBase64String(base64rsakey);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
        public async Task<Tuple<string, string>> GetDecKeyAndIv(string base64EncRsaPrivKey, string password, string base64EncFileKey, string base64EncFileIv)
        {
            byte[] rsaPrivKeyBytes = GetRsaDecKry(base64EncRsaPrivKey, password);
            if(rsaPrivKeyBytes == null)
                throw new Exception("Error decrypting RSA private key");
            byte[] fileKey = Utils.DecryptRSAwithPrivateKey(Convert.FromBase64String(base64EncFileKey), rsaPrivKeyBytes);
            byte[] fileIv = Utils.DecryptRSAwithPrivateKey(Convert.FromBase64String(base64EncFileIv), rsaPrivKeyBytes);

            Tuple<string, string> fileKeyAndIv = new Tuple<string, string>(Convert.ToBase64String(fileKey), Convert.ToBase64String(fileIv));
            return fileKeyAndIv;
        }

        public async Task<byte[]> GetHashOfFile(FileStream fileStream)
        {

            byte[] hash = Utils.GetHashOfFile(fileStream);
            return hash;
        }

        public async Task<byte[]> EncryptFileStream(FileStream fileStream, byte[] key, byte[] iv, string encFileName)
        {
            byte[] tag = Utils.EncryptAndSaveFileWithAesGCM_AndGetTag(encFileName, fileStream, key, iv);
            return tag;
        }

    }
}
