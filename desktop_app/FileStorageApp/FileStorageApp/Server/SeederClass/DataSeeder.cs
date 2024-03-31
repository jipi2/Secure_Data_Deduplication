using CryptoLib;
using FileStorageApp.Server.Database;
using FileStorageApp.Server.Entity;
using FileStorageApp.Server.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace FileStorageApp.Server.SeederClass
{
    public class DataSeeder
    {
        private readonly DataContext _context;

        public DataSeeder(DataContext context)
        {
            _context = context;
        }

        public async Task SeedData()
        {
            await SeedRoles();
            await SeedAdmin();
            await SeedProxy();
        }

        private async Task SeedRoles()
        {
            if (!_context.Roles.Any())
            {
                var roles = new List<Entity.Role>
                {
                    new Entity.Role { RoleName = "admin" },
                    new Entity.Role { RoleName = "client" },
                    new Entity.Role { RoleName = "proxy" }
                };
                await _context.AddRangeAsync(roles);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedAdmin()
        {
            if (!_context.Users.Where(u => u.Roles.Any(r => r.RoleName == "admin")).Any())
            {
                var pass = "admin";
                byte[] salt = Utils.GenerateRandomBytes(16);
                var admin = new User
                {
                    LastName = "admin",
                    FirstName = "admin",
                    Password = Utils.HashTextWithSalt(pass, salt),
                    Email = "admin",
                    Salt = Utils.ByteToHex(salt),
                    Roles = new List<Entity.Role>()
                };
                admin.Roles.Add(await _context.Roles.Where(r => r.RoleName == "admin").FirstOrDefaultAsync());
                _context.Users.Add(admin);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedProxy()
        {
            if (!_context.Users.Where(u => u.Roles.Any(r => r.RoleName == "proxy")).Any())
            {
                var pass = "proxy";
                byte[] salt = Utils.GenerateRandomBytes(16);
                var proxy = new User
                {
                    LastName = "pythonProxy",
                    FirstName = "pythonProxy",
                    Password = Utils.HashTextWithSalt(pass, salt),
                    Email = "pythonProxy",
                    Salt = Utils.ByteToHex(salt),
                    Roles = new List<Entity.Role>()
                };
                proxy.Roles.Add(await _context.Roles.Where(r => r.RoleName == "proxy").FirstOrDefaultAsync());
                _context.Users.Add(proxy);
                await _context.SaveChangesAsync();
            }
        }
    }
}
