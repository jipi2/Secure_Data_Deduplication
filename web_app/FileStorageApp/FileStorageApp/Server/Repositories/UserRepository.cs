using FileStorageApp.Server.Database;
using FileStorageApp.Shared;
using FileStorageApp.Shared.Dto;
using Microsoft.EntityFrameworkCore;
using CryptoLib;
using FileStorageApp.Server.Services;
using FileStorageApp.Server.Entity;
using Azure.Core;
using FileStorageApp.Server.SecurityFolder;
using FileStorageApp.Client.Pages;
using Microsoft.Extensions.Logging;

namespace FileStorageApp.Server.Repositories
{
    public class UserRepository
    {
        DataContext _context;
        RoleService _roleService;
        SecurityManager _securityManager;

        private readonly int saltLength = 16;
        public UserRepository(DataContext context, RoleService roleService, SecurityManager securityManager)
        {
            _context = context;
            _roleService = roleService;
            _securityManager = securityManager; 
        }

        public async Task<Response> Register(RegisterUser regUser)
        {
            var user = await _context.Users.Where(u => u.Email.ToLower().Equals(regUser.Email)).FirstOrDefaultAsync();
            if (user != null)
                return (new Response { Succes = false, Message = "Email already exists" });
            if (!regUser.Password.Equals(regUser.Password))
                return (new Response { Succes = false, Message = "Confirm passowrd field is different from password field" });

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
            };
            newUser.Roles.Add( await _roleService.GetRoleByName("client"));
            
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return (new Response { Succes = true, Message = "User registered successfully" , AccessToken = _securityManager.GetNewJwt(newUser)});
        }

        public async Task<Response> Login(LoginUser logUser)
        {
            var user = await _context.Users.Include(u => u.Roles).Where(u => u.Email.ToLower().Equals(logUser.Email)).FirstOrDefaultAsync();
            if (user == null)
                return (new Response { Succes = false, Message = "Login faild" });
            if (!user.Password.Equals(Utils.HashTextWithSalt(logUser.password, Utils.HexToByte(user.Salt))))
                return (new Response { Succes = false, Message = "Login faild" });

            return (new Response { Succes = true, Message = "Login successfully", AccessToken = _securityManager.GetNewJwt(user)});
        }

        public async Task SaveUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            var user = await _context.Users.Include(u => u.Roles).Where(u => u.Email.ToLower().Equals(email.ToLower())).FirstOrDefaultAsync();
            if(user == null) return null;
            return user;
        }
        public string GetUserRoleName(User user)
        {
            return user.Roles[0].RoleName;
        }

        public async Task<User> GetUserById(int id)
        {
            try
            {
                return await _context.Users.Include(u => u.Roles).Where(u => u.Id == id).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("User with id: " + id + " not found");
                return null;
            }
        }

        public async Task<User?> GetUserbyEmail(string Email)
        {
            var user = await _context.Users.Where(u => u.Email.ToLower().Equals(Email)).FirstOrDefaultAsync();
            return user;
        }

        public async Task SaveServerDFKeysForUser(User user, string serverPublicKey, string serverPrivateKey, string P, string G)
        {
            user.ServerDHPublic = serverPublicKey;
            user.ServerDHPrivate = serverPrivateKey;
            user.P = P;
            user.G = G;
            await _context.SaveChangesAsync();
        }

        public async Task SaveSymKey(int userId, string symKey)
        {
            User user = await GetUserById(userId);
            user.SymKey = symKey;
            await _context.SaveChangesAsync();
        }

        //public async Task AddFile(string userId,FileMetadata fileMeta)
        //{
        //    User user = await GetUserById(Convert.ToInt32(userId));
        //    if(user.Files == null)
        //        user.Files = new List<FileMetadata>();
        //    user.Files.Add(fileMeta);
        //    await _context.SaveChangesAsync();
        //}

        public async Task<List<FilesNameDate>> GetFilesNameDate(string userId)
        {
            return null;
        }
    }
}
