using CryptoLib;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TestingConsole
{
    internal class Program
    {
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

        
        static public AsymmetricCipherKeyPair GenerateKeys(DHParameters parameters)
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
        }

        static void Main(string[] args)
        {
            AsymmetricCipherKeyPair aliceKeyPair  = Utils.GenerateRSAKeyPair();
            string plaintext = "Ana are mere si mihai are o gramada de pere";

            //RsaPrivateCrtKeyParameters privateKeyParams = (RsaPrivateCrtKeyParameters)aliceKeyPair.Private;
            //PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKeyParams);
            //byte[] privateKeyBytes = privateKeyInfo.ToAsn1Object().GetDerEncoded();
            //string serializedPrivateKey = Convert.ToBase64String(privateKeyBytes);
            //RsaKeyParameters privateKeyParams2 = (RsaKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(serializedPrivateKey));

            //SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(aliceKeyPair.Public);
            //byte[] publicKeyBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();
            //string serializedPublicKey = Convert.ToBase64String(publicKeyBytes);
            //AsymmetricKeyParameter alicePublicKey2 = PublicKeyFactory.CreateKey(Convert.FromBase64String(serializedPublicKey));

            //byte[] plaintextbytes = Encoding.UTF8.GetBytes(plaintext);
            //IAsymmetricBlockCipher cipher = new RsaEngine();
            //cipher.Init(true, alicePublicKey2);
            //byte[] cipherBytes = cipher.ProcessBlock(plaintextbytes, 0, plaintextbytes.Length);
            //string cipherText = Convert.ToBase64String(cipherBytes);
            //Console.WriteLine("Encrypted message: " + cipherText);

            //cipher.Init(false, privateKeyParams2);
            //byte[] decryptedBytes = cipher.ProcessBlock(cipherBytes, 0, cipherBytes.Length);
            //string decryptedText = Encoding.UTF8.GetString(decryptedBytes);
            //Console.WriteLine("Decrypted message: " + decryptedText);

            byte[] privateKeyDerEnc = Utils.GetDerEncodedRSAPrivateKey(aliceKeyPair);
            string base64PubKey = Utils.GetBase64RSAPublicKey(aliceKeyPair);

            byte[] cipherBytes = Utils.EncryptRSAwithPublicKey(Encoding.UTF8.GetBytes(plaintext), base64PubKey);
            byte[] decPlainTextBytes = Utils.DecryptRSAwithPrivateKey(cipherBytes, privateKeyDerEnc);
            Console.WriteLine(Encoding.UTF8.GetString(decPlainTextBytes));

            Console.ReadKey();  

        }
    }
}
