using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.X509;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CryptoLib
{
    public class Utils
    {
        static public Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        static public byte[] GenerateRandomBytes(int length)
        {
            var randomGenerator = new SecureRandom();
            var randomBytes = new byte[length];
            randomGenerator.NextBytes(randomBytes);
            return randomBytes;
        }

        static public byte[] HexToByte(string hexString)
        {
            return Hex.Decode(hexString);
        }

        static public string ByteToHex(byte[] bytes)
        {
            return Hex.ToHexString(bytes);
        }
        static public string HashTextWithSalt(string text, byte[] salt)
        {
            Encoding encoding = Encoding.UTF8;
            byte[] textBytes = encoding.GetBytes(text);

            var sha512 = new Sha512Digest();


            var combinedBytes = new byte[salt.Length + textBytes.Length];
            Array.Copy(salt, combinedBytes, salt.Length);
            Array.Copy(textBytes, 0, combinedBytes, salt.Length, text.Length);

            sha512.BlockUpdate(combinedBytes, 0, combinedBytes.Length);

            byte[] hash = new byte[sha512.GetDigestSize()];
            sha512.DoFinal(hash, 0);

            return Hex.ToHexString(hash);

        }
        static public byte[] ComputeHash(byte[] array, int hashSize = 256)
        {
            byte[] h = new byte[hashSize/8];
            var sha3_256 = new Sha3Digest(hashSize);

            sha3_256.BlockUpdate(array, 0, array.Length);
            sha3_256.DoFinal(h, 0);

            return h;
        }
        static public string GetHashOfFile(Stream file)
        {

            byte[] h = new byte[64];
            var sha3_512 = new Sha3Digest(512);

            long fileSize = file.Length;
            byte[] buffer = new byte[1024];

            int bytesRead = 0;
            while((bytesRead = file.Read(buffer, 0, buffer.Length))>0)
            {
                sha3_512.BlockUpdate(buffer, 0, bytesRead);
            }

            sha3_512.DoFinal(h, 0);
            
            return Hex.ToHexString(h);
        }

        static private void GetLeaves(ref MerkleTree MT, int count)
        {
            try
            {
                while (count > 1)
                {
                    count = 0;
                    int level = MT.Levels + 1;
                    int n = MT.HashTree.Count;
                    MerkleTree aux = new MerkleTree();


                    for (int i = MT.IndexOfLevel[MT.Levels]; i < n; i += 2)
                    {
                        byte[] h = new byte[32];
                        var sha3_256 = new Sha3Digest(256);
                        sha3_256.BlockUpdate(MT.HashTree[i]._hash, 0, MT.HashTree[i]._hash.Length);
                        sha3_256.BlockUpdate(MT.HashTree[i + 1]._hash, 0, MT.HashTree[i + 1]._hash.Length);
                        sha3_256.DoFinal(h, 0);
                        aux.HashTree.Add(new MTMember(level, h));
                        count++;
                    }
                    foreach (MTMember m in aux.HashTree)
                    {
                        MT.HashTree.Add(m);
                    }
                    if (count % 2 != 0 && count != 1)
                    {
                        MT.HashTree.Add(MT.HashTree[MT.HashTree.Count - 1]);
                        count++;
                    }
                    MT.IndexOfLevel.Add(MT.HashTree.Count - count);
                    MT.Levels++;
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static public MerkleTree GetMerkleTree(Stream file)
        {
            MerkleTree MT = new MerkleTree();
            long fileSize = file.Length;
            //byte[] buffer = new byte[16384];
            byte[] buffer = new byte[1024];

            int bytesRead = 0;
            int count = 0;
            while ((bytesRead = file.Read(buffer, 0, buffer.Length)) > 0)
            {
                byte[] h = new byte[32];
                var sha3_256 = new Sha3Digest(256);
                sha3_256.BlockUpdate(buffer, 0, bytesRead);
                sha3_256.DoFinal(h, 0);
                MT.HashTree.Add(new MTMember(0, h));
                count++;
            }
            if (count % 2 != 0)
            {
                MT.HashTree.Add(MT.HashTree[0]);
                count++;
            }
            MT.IndexOfLevel.Add(0);
            GetLeaves(ref MT, count);

            return MT;
        }

        static public MerkleTree GetMerkleTreeFromFilePath(string filePath)
        {
            MerkleTree MT = new MerkleTree();
            FileInfo fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;
            int buffer_size = 0;

            if (fileSize < 1000)
                buffer_size = 300;
            else if (fileSize < 10000 && fileSize >= 1000)
                buffer_size = 3000;
            else if (fileSize < 1000000 && fileSize >= 10000)
                buffer_size = 10000;
            else
                buffer_size = 100000;

            byte[] buffer = new byte[buffer_size];

            int bytesRead = 0;
            int count = 0;

            using (FileStream fileStream = File.OpenRead(filePath))
            {

                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] h = new byte[32];
                    var sha3_256 = new Sha3Digest(256);
                    sha3_256.BlockUpdate(buffer, 0, bytesRead);
                    sha3_256.DoFinal(h, 0);
                    MT.HashTree.Add(new MTMember(0, h));
                    count++;
                }
                if (count % 2 != 0)
                {
                    MT.HashTree.Add(MT.HashTree[0]);
                    count++;
                }
                MT.IndexOfLevel.Add(0);
                GetLeaves(ref MT, count);
            }

            return MT;
        }

        static public MerkleTree GetMerkleTree(string filePath)
        {
            MerkleTree MT = new MerkleTree();
            FileInfo fileInfo = new FileInfo(filePath);
            long fileSize = fileInfo.Length;
            int buffer_size = 0;

            if (fileSize < 1000)
                buffer_size = 300;
            else if (fileSize < 10000 && fileSize >= 1000)
                buffer_size = 3000;
            else if (fileSize < 1000000 && fileSize >= 10000)
                buffer_size = 10000;
            else
                buffer_size = 100000;

            byte[] buffer = new byte[buffer_size];

            int bytesRead = 0;
            int count = 0;

            using (FileStream fileStream = File.OpenRead(filePath))
            {

                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] h = new byte[32];
                    var sha3_256 = new Sha3Digest(256);
                    sha3_256.BlockUpdate(buffer, 0, bytesRead);
                    sha3_256.DoFinal(h, 0);
                    MT.HashTree.Add(new MTMember(0, h));
                    count++;
                }
                if (count % 2 != 0)
                {
                    MT.HashTree.Add(MT.HashTree[0]);
                    count++;
                }
                MT.IndexOfLevel.Add(0);
                GetLeaves(ref MT, count);
            }

            return MT;
        }
        static public byte[] GetSeedFromTag(byte[] tag, byte[] secret)
        {
            int len = tag.Length+secret.Length;
            byte[] aux = new byte[len];

            Array.Copy(aux, tag, tag.Length);
            Array.Copy(secret, 0, aux, tag.Length, secret.Length);

            var randomGenerator = new SecureRandom(aux);
            var randomBytes = new byte[4];

            randomGenerator.NextBytes(randomBytes);
            return randomBytes;
        }

        static public byte[] GenerateResp(MerkleTree mt, ref int n1, ref int n2, ref int n3)  //cele 3 pozitii din MT
        {
            int mtNodes = mt.HashTree.Count;
            byte[] Resp = new byte[mt.HashTree[0]._hash.Length];
            byte[] aux = new byte[4]; 

            var randomGen = new SecureRandom();

            randomGen.NextBytes(aux);
            n1 = Math.Abs( BitConverter.ToInt32(aux, 0) % mtNodes);

            randomGen.NextBytes(aux);
            n2 = Math.Abs(BitConverter.ToInt32(aux, 0) % mtNodes);

            randomGen.NextBytes(aux);
            n3 = Math.Abs(BitConverter.ToInt32(aux, 0) % mtNodes);

            for(int i=0;i<Resp.Length;i++)
            {
                Resp[i] = (byte)(mt.HashTree[n1]._hash[i] ^ mt.HashTree[n2]._hash[i] ^ mt.HashTree[n3]._hash[i]);
            }

            return Resp;
        }
        static public AsymmetricCipherKeyPair GenerateRSAKeyPair()
        {
            int modulus = 2048;
            RsaKeyPairGenerator g = new RsaKeyPairGenerator();
            g.Init(new KeyGenerationParameters(new SecureRandom(), modulus));
            AsymmetricCipherKeyPair keys = g.GenerateKeyPair();

            return keys;
        }
        static public string GetPemAsString(AsymmetricKeyParameter key)
        {
            TextWriter textWriter = new StringWriter();
            PemWriter pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(key);
            return textWriter.ToString();
        }
        static public void WritePrivateKeyToFile(AsymmetricKeyParameter privKey, string privFileName)
        {
            TextWriter textWriterPrivate = new StringWriter();
            PemWriter pemWriterPrivate = new PemWriter(textWriterPrivate);
            pemWriterPrivate.WriteObject(privKey);
            File.WriteAllText(privFileName, textWriterPrivate.ToString());
        }

        static public void WritePublicKeyToFile(AsymmetricKeyParameter pubKey, string pubFileName)
        {
            TextWriter textWriterPublic = new StringWriter();
            PemWriter pemWriterPublic = new PemWriter(textWriterPublic);
            pemWriterPublic.WriteObject(pubKey);
            File.WriteAllText(pubFileName, textWriterPublic.ToString());
        }

        static public DHPrivateKeyParameters ReadPrivateKeyFromPemString(string key)
        {
            TextReader textReaderPrivate = new StringReader(key);
            PemReader pemReaderPrivate = new PemReader(textReaderPrivate);
            DHPrivateKeyParameters keyPair = (DHPrivateKeyParameters)pemReaderPrivate.ReadObject();
            return keyPair;
        }

        static public AsymmetricKeyParameter ReadRSAPrivateKeyFromFile(string privFileName)
        {
            TextReader textReaderPrivate = new StringReader(File.ReadAllText(privFileName));
            PemReader pemReaderPrivate = new PemReader(textReaderPrivate);
            AsymmetricCipherKeyPair keyPair = (AsymmetricCipherKeyPair)pemReaderPrivate.ReadObject();
            return keyPair.Private;
        }

        static public AsymmetricKeyParameter ReadRSAPublicKeyFromFile(string pubFileName)
        {
            TextReader textReaderPublic = new StringReader(File.ReadAllText(pubFileName));
            PemReader pemReaderPublic = new PemReader(textReaderPublic);
            AsymmetricKeyParameter pubKey = (AsymmetricKeyParameter)pemReaderPublic.ReadObject();
            return pubKey;
        }

        static public DHParameters GenerateParameters()
        {
            var generator = new DHParametersGenerator();
            generator.Init(256, 30, new SecureRandom());
            return generator.GenerateParameters();
        }

        static public string GetG(DHParameters parameters)
        {
            return parameters.G.ToString();
        }

        static public string GetP(DHParameters parameters)
        {
            return parameters.P.ToString();
        }

        static public DHParameters GenerateParameters(string G, string P)
        {
            BigInteger GBig = new BigInteger(G);
            BigInteger PBig = new BigInteger(P);

            return new DHParameters(PBig, GBig);

        }

        static public AsymmetricCipherKeyPair GenerateDFKeys(DHParameters parameters)
        {
            var keyGen = GeneratorUtilities.GetKeyPairGenerator("DH");
            var kgp = new DHKeyGenerationParameters(new SecureRandom(), parameters);
            keyGen.Init(kgp);
            return keyGen.GenerateKeyPair();
        }

        // This returns A
        static public string GetPublicKey(AsymmetricCipherKeyPair keyPair)
        {
            var dhPublicKeyParameters = keyPair.Public as DHPublicKeyParameters;
            if (dhPublicKeyParameters != null)
            {
                return dhPublicKeyParameters.Y.ToString();
            }
            throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
        }
        // This returns a
        static public string GetPrivateKey(AsymmetricCipherKeyPair keyPair)
        {
            var dhPrivateKeyParameters = keyPair.Private as DHPrivateKeyParameters;
            if (dhPrivateKeyParameters != null)
            {
                return dhPrivateKeyParameters.X.ToString();
            }
            throw new NullReferenceException("The key pair provided is not a valid DH keypair.");
        }

        static public BigInteger ComputeSharedSecret(string A, AsymmetricKeyParameter bPrivateKey, DHParameters internalParameters)
        {
            var importedKey = new DHPublicKeyParameters(new BigInteger(A), internalParameters);
            var internalKeyAgree = AgreementUtilities.GetBasicAgreement("DH");
            internalKeyAgree.Init(bPrivateKey);
            return internalKeyAgree.CalculateAgreement(importedKey);

            /* Exemplu de folosire
             
            DHParameters parameters = GenerateParameters();
            DHParameters parameters2 = GenerateParameters(GetG(parameters), GetP(parameters));

            AsymmetricCipherKeyPair bobKeys = GenerateKeys(parameters2);
            AsymmetricCipherKeyPair aliceKeys = GenerateKeys(parameters2);

            string bobPublicKey = GetPublicKey(bobKeys);
            string alicePublicKey = GetPublicKey(aliceKeys);

            BigInteger secretBob = ComputeSharedSecret(alicePublicKey, bobKeys.Private, parameters2);
            BigInteger secretAlice = ComputeSharedSecret(bobPublicKey, aliceKeys.Private, parameters2);

            if (secretBob.Equals(secretAlice))
                Console.WriteLine("The secret is oke");
            else
                Console.WriteLine("Error secret");
             */
        }
        static public string EncryptAesWithIv(byte[] plainTextBytes, byte[] key, byte[] iv)
        {
            var cipher = new CbcBlockCipher(new AesEngine());
            var parameters = new ParametersWithIV(new KeyParameter(key), iv);
            cipher.Init(true, parameters);

            int blockSize = cipher.GetBlockSize();
            int padding = blockSize - (plainTextBytes.Length % blockSize);
            byte[] paddedInput = new byte[plainTextBytes.Length + padding];
            Array.Copy(plainTextBytes, paddedInput, plainTextBytes.Length);
            for (int i = plainTextBytes.Length; i < paddedInput.Length; i++)
            {
                paddedInput[i] = (byte)padding;
            }
            byte[] cipherText = new byte[paddedInput.Length];

            for (int i = 0; i < plainTextBytes.Length; i += 16)
                cipher.ProcessBlock(paddedInput, i, cipherText, i);


            return Convert.ToBase64String(cipherText);
        }

        static public string DecryptAesWithIv(string cipherText, byte[] key, byte[] iv)
        {
            var cipherTextBytes = Convert.FromBase64String(cipherText);
            /*  var key = Convert.FromBase64String(keyString);*/
            /* var iv = Convert.FromBase64String(ivString)*/
            
            var cipher = new CbcBlockCipher(new AesEngine());
            var parameters = new ParametersWithIV(new KeyParameter(key), iv);
            cipher.Init(false, parameters);

            var plaintextBytes = new byte[cipherTextBytes.Length];

            for (int i = 0; i < cipherTextBytes.Length; i += 16)
                cipher.ProcessBlock(cipherTextBytes, i, plaintextBytes, i);

            byte paddingValue = plaintextBytes[plaintextBytes.Length - 1];
            int unpaddedLength = cipherTextBytes.Length - paddingValue;
            byte[] unpaddedOutput = new byte[unpaddedLength];
            Array.Copy(plaintextBytes, unpaddedOutput, unpaddedLength);

            return Convert.ToBase64String(unpaddedOutput);
        }
        static public string EncryptAes(byte[] plainTextBytes, byte[] key, byte[] iv = null)
        {
            //var cipher = new CbcBlockCipher(new AesEngine());
            //var parameters = new KeyParameter(key);
            //cipher.Init(true, parameters);

            // int blockSize = cipher.GetBlockSize();
            // int padding = blockSize - (plainTextBytes.Length % blockSize);
            // byte[] paddedInput = new byte[plainTextBytes.Length + padding];
            // Array.Copy(plainTextBytes, paddedInput, plainTextBytes.Length);
            // for(int i=plainTextBytes.Length; i<paddedInput.Length; i++)
            // {
            //     paddedInput[i] = (byte)padding;
            // }
            // byte[] cipherText = new byte[paddedInput.Length];

            //for(int i = 0; i<plainTextBytes.Length; i+=16) 
            //   cipher.ProcessBlock(paddedInput, i, cipherText, i);


            //return Convert.ToBase64String(cipherText);
            try
            {
                var cipher = CipherUtilities.GetCipher("AES/GCM/NoPadding");
                if (iv == null)
                    cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), new byte[16]));
                else
                    cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));

                var cipherTextBytes = cipher.DoFinal(plainTextBytes);
                return Convert.ToBase64String(cipherTextBytes);
            }
            catch(CryptoException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        static public string DecryptAes(byte[] cipherText, byte[] key, byte[] iv = null)
        {
            //  var cipherTextBytes = Convert.FromBase64String(cipherText);
            ///*  var key = Convert.FromBase64String(keyString);*/
            // /* var iv = Convert.FromBase64String(ivString)*/;
            //  var cipher = new CbcBlockCipher(new AesEngine());
            //  var parameters = new KeyParameter(key);
            //  cipher.Init(false, parameters);

            //  var plaintextBytes = new byte[cipherTextBytes.Length];

            //  for (int i = 0; i < cipherTextBytes.Length; i += 16)
            //      cipher.ProcessBlock(cipherTextBytes, i, plaintextBytes, i);

            //  byte paddingValue = plaintextBytes[plaintextBytes.Length - 1];
            //  int unpaddedLength = cipherTextBytes.Length - paddingValue;
            //  byte[] unpaddedOutput = new byte[unpaddedLength];
            //  Array.Copy(plaintextBytes, unpaddedOutput, unpaddedLength);

            //  return Convert.ToBase64String(unpaddedOutput);

            try
            {
                var cipher = CipherUtilities.GetCipher("AES/GCM/NoPadding");
                if (iv == null)
                    cipher.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), new byte[16]));
                else
                    cipher.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));
               
                var plainTextBytes = cipher.DoFinal(cipherText);
                return Convert.ToBase64String(plainTextBytes);
            }
            catch (CryptoException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
        }
        //static public string EncryptWithGCM(string plaintext, byte[] key/*, byte[] nonce*/)
        //{
        //    try 
        //    { 
        //        var tagLength = 16;

        //        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        //        var ciphertextTagBytes = new byte[plaintextBytes.Length + tagLength];

        //        var cipher = new GcmBlockCipher(new AesEngine());
        //        var parameters = new AeadParameters(new KeyParameter(key), tagLength * 8, new byte[8]);  //al treilea parametru trebuia sa fie nonce
        //        cipher.Init(true, parameters);

        //        var offset = cipher.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, ciphertextTagBytes, 0);
        //        cipher.DoFinal(ciphertextTagBytes, offset); // create and append tag: ciphertext | tag

        //        return Convert.ToBase64String(ciphertextTagBytes);
        //    }
        //    catch (InvalidCipherTextException e)
        //    {
        //        Console.WriteLine(e.ToString());
        //        return null;
        //    }
        //}

        //public static string DecryptWithGCM(string ciphertextTag, byte[] key/*, byte[] nonce*/)
        //{
        //    try
        //    {
        //        var tagLength = 16;

        //        var ciphertextTagBytes = Convert.FromBase64String(ciphertextTag);
        //        var plaintextBytes = new byte[ciphertextTagBytes.Length - tagLength];

        //        var cipher = new GcmBlockCipher(new AesEngine());
        //        var parameters = new AeadParameters(new KeyParameter(key), tagLength * 8, new byte[8]); //al treilea parametru trebuia sa fie nonce
        //        cipher.Init(false, parameters);

        //        var offset = cipher.ProcessBytes(ciphertextTagBytes, 0, ciphertextTagBytes.Length, plaintextBytes, 0);
        //        cipher.DoFinal(plaintextBytes, offset); // authenticate data via tag

        //        return Convert.ToBase64String(plaintextBytes);
        //    }
        //    catch (InvalidCipherTextException e)
        //    {
        //        Console.WriteLine(e.ToString());
        //        return null;
        //    }
        //}

        static public byte[] GetDerEncodedRSAPrivateKey(AsymmetricCipherKeyPair keyPair)
        {
            try
            {
                RsaPrivateCrtKeyParameters rsaPrivateKeyParams = (RsaPrivateCrtKeyParameters)keyPair.Private;
                PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(rsaPrivateKeyParams);
                return privateKeyInfo.ToAsn1Object().GetDerEncoded();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        static public string GetBase64RSAPublicKey(AsymmetricCipherKeyPair keyPair)
        {
            try
            {
                SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyPair.Public);
                byte[] publicKeyBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();
                return Convert.ToBase64String(publicKeyBytes);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        static public byte[] EncryptRSAwithPublicKey(byte[] plaintextbytes, string base64publicKey)
        {
            try
            {
                AsymmetricKeyParameter pubKeyParameter = PublicKeyFactory.CreateKey(Convert.FromBase64String(base64publicKey));
                IAsymmetricBlockCipher cipher = new RsaEngine();
                cipher.Init(true, pubKeyParameter);
                byte[] cipherBytes = cipher.ProcessBlock(plaintextbytes, 0, plaintextbytes.Length);
                return cipherBytes;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }

        static public byte[] DecryptRSAwithPrivateKey(byte[] ciphertextbytes, byte[] derEncodedPrivateKey)
        {
            try
            {
                RsaKeyParameters privateKeyParams2 = (RsaKeyParameters)PrivateKeyFactory.CreateKey(derEncodedPrivateKey);
                IAsymmetricBlockCipher cipher = new RsaEngine();
                cipher.Init(false, privateKeyParams2);
                byte[] plaintextbytes = cipher.ProcessBlock(ciphertextbytes, 0, ciphertextbytes.Length);
                return plaintextbytes;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
