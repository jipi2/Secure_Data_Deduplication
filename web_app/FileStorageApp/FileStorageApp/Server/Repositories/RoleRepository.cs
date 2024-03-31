using FileStorageApp.Server.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileStorageApp.Server.Repositories
{

    public class RoleRepository : Controller
    {
        DataContext _context;
        public RoleRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Entity.Role> getRoleByName(string name)
        {
            var role = await _context.Roles.Where(r => r.RoleName.Equals(name)).FirstOrDefaultAsync();
            if(role ==  null)
            {
                return null;
            }
            return role;
        }
    }
}
