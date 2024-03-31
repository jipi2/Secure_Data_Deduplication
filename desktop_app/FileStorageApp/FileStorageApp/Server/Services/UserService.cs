using CryptoLib;
using FileStorageApp.Server.Entity;
using FileStorageApp.Server.Repositories;
using FileStorageApp.Server.SecurityFolder;
using FileStorageApp.Shared;
using FileStorageApp.Shared.Dto;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;

namespace FileStorageApp.Server.Services
{
    public class UserService
    {
        public UserRepository _userRepo { get; set; }
        public RoleRepository _roleRepo { get; set; }
        public SecurityManager _secManager { get; set; }
        
        public UserService(UserRepository userRepository, SecurityManager secManager, RoleRepository roleRepository)
        {
            _userRepo = userRepository;
            _secManager = secManager;
            _roleRepo = roleRepository;
        }

        public async Task<Response> Register(RegisterUser regUser)
        {
            var user = _userRepo.GetUserbyEmail(regUser.Email).Result;
            if (user != null)
                throw new ExceptionModel("Email already exists", 1);
            if (!regUser.Password.Equals(regUser.Password))
                throw new ExceptionModel("Confirm passowrd field is different from password field", 1);

            byte[] salt = Utils.GenerateRandomBytes(16);

            var newUser = new Entity.User
            {
                FirstName = regUser.FirstName,
                LastName = regUser.LastName,
                Email = regUser.Email,
                Password = Utils.HashTextWithSalt(regUser.Password, salt),
                Salt = Utils.ByteToHex(salt),
                isDeleted = false,
                Roles = new List<Entity.Role>(),
                Base64RSAEncPrivateKey = regUser.rsaKeys.base64EncPrivKey,
                Base64RSAPublicKey = regUser.rsaKeys.base64PubKey
                //Files = new List<Entity.FileMetadata>()
            };
            newUser.Roles.Add(await _roleRepo.getRoleByName("client"));

            _userRepo.SaveUser(newUser);

            return (new Response { Succes = true, Message = "User registered successfully", AccessToken = _secManager.GetNewJwt(newUser) }); ;
        }

        public async Task<Response> AddProxy(RegisterProxyDto regProxy)
        {
            var user = _userRepo.GetUserbyEmail(regProxy.ProxyMail).Result;
            if(user != null)
                throw new ExceptionModel("Email already exists for this proxy", 1);

            byte[] salt = Utils.GenerateRandomBytes(16);

            var newProxy = new Entity.User
            {
                FirstName = regProxy.ProxyName,
                LastName = regProxy.ProxyName,
                Email = regProxy.ProxyMail,
                Password = Utils.HashTextWithSalt(regProxy.ProxyPassword, salt),
                Salt = Utils.ByteToHex(salt),
                isDeleted = false,
                Roles = new List<Entity.Role>(),
                //Files = new List<Entity.FileMetadata>()
            };
            newProxy.Roles.Add(await _roleRepo.getRoleByName("proxy"));
            _userRepo.SaveUser(newProxy);

            return (new Response { Succes = true, Message = "Proxy added successfully"}); ;
        }
        public async Task<Response> Login(LoginUser logUser)
        {
            var user = _userRepo.GetUserByEmail(logUser.Email).Result;
            if (user == null)
                throw new ExceptionModel("Login faild!", 1);
            if (!user.Password.Equals(Utils.HashTextWithSalt(logUser.password, Utils.HexToByte(user.Salt))))
                throw new ExceptionModel("Login faild!", 1);

            return (new Response { Succes = true, Message = "Login successfully", AccessToken = _secManager.GetNewJwt(user) });
        }

        public bool isUserAdmin(User user)
        {
            string roleName = _userRepo.GetUserRoleName(user);
            if (roleName == "admin")
                return true;
            return false;
        }

        public async Task<string> GetUserIdFromJWT(string jwt)
        {
            string id =  _secManager.GetUserIdFromJWT(jwt);
            return id;
        }

        public async Task<User>? GetUserById(string id)
        {
            try
            {
                return await _userRepo.GetUserById(Convert.ToInt32(id));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public async Task SaveServerDFKeysForUser(User user, AsymmetricCipherKeyPair serverKeys, string P, string G)
        {

            string serverPubKey = Utils.GetPemAsString(serverKeys.Public);
            string serverPrivKey = Utils.GetPemAsString(serverKeys.Private);

            await _userRepo.SaveServerDFKeysForUser(user, serverPubKey, serverPrivKey, P, G);
        }

        public async Task<string> GetPrivateKeyOfServerForUser(string userId)
        {
            User user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            return user.ServerDHPrivate == null ? null : user.ServerDHPrivate;
        }

        public async Task<Dictionary<string, string>> GetParametersForDF(string userId)
        {
            User user = await _userRepo.GetUserById(Convert.ToInt32(userId));
            Dictionary<string, string> stringParams = new Dictionary<string, string>();
            if (user.P == null || user.G == null)
                return stringParams;
            stringParams.Add("P", user.P);
            stringParams.Add("G", user.G);

            return stringParams;
        }

        public async Task SaveSymKeyForUser(string userId, byte[] key)
        {
            try
            {
                await _userRepo.SaveSymKey(Convert.ToInt32(userId), Convert.ToBase64String(key));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //public async Task AddFile(string userId, FileMetadata fileMeta)
        //{
        //    await _userRepo.AddFile(userId, fileMeta);
        //}

        public Task<string> GetUserEmail(string id)
        {
            var user = _userRepo.GetUserById(Convert.ToInt32(id)).Result;
            return Task.FromResult(user.Email);
        }

        public async Task<string> GetUserIdByEmail(string email)
        {
            var user = await _userRepo.GetUserbyEmail(email);
            if (user == null)
                return null;
            return user.Id.ToString();
        }

        public async Task<string?> GetUserPubKey(string email)
        {
            var user = await _userRepo.GetUserbyEmail(email);
            if (user == null)
                return null;
            return user.Base64RSAPublicKey;
        }

        public async Task<RsaDto?> GetRsaKeyPair(string id)
        {
            User? user = await _userRepo.GetUserById(Convert.ToInt32(id));
            if (user == null)
                throw new Exception("User does not exist!");
            if(user.Base64RSAPublicKey == null || user.Base64RSAEncPrivateKey == null)
                throw new Exception("User does not have RSA key pair!");
            RsaDto rsaDto = new RsaDto
            {
                base64PubKey = user.Base64RSAPublicKey,
                base64EncPrivKey = user.Base64RSAEncPrivateKey
            };
            return rsaDto;
        }
    }
}
