using FileStorageApp.Server.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace FileStorageApp.Server.Services
{
    public class RoleService : Controller
    {
        public RoleRepository _roleRepo { get; set; }
        public RoleService(RoleRepository roleRepository)
        {
            _roleRepo = roleRepository;
        }

        public async Task<Entity.Role> GetRoleByName(string name)
        {
            return await _roleRepo.getRoleByName(name);
        }

    }
}
