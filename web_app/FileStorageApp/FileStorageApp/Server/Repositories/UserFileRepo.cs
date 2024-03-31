using FileStorageApp.Server.Database;
using FileStorageApp.Server.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace FileStorageApp.Server.Repositories
{
    public class UserFileRepo
    {
        DataContext _context;
        public UserFileRepo(DataContext context)
        {
            _context = context;
        }

        public async Task SaveUserFile(UserFile userFile)
        {
            try
            {
                _context.UserFiles.Add(userFile);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public async Task<UserFile?> GetUserFileByUserIdAndFileName(int id, string fileName)
        {
            UserFile? userFile = await _context.UserFiles.Where(f => f.UserId == id && f.FileName == fileName).FirstOrDefaultAsync();
            if (userFile == null)
            {
                return null;
            }
            return userFile;
        }

        public async Task DeleteUserFile(UserFile userFile)
        {
            _context.UserFiles.Remove(userFile);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserFile>?> GetUserFileByUserId(int id)
        {
            List<UserFile>? list = await _context.UserFiles.Where(u => u.UserId == id).ToListAsync();
            if (list == null)
                return null;
            return list;
        }

        public async Task UpdateUserFile(UserFile uf)
        {
            _context.UserFiles.Update(uf);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserFile>?> GetUserFilesByFileId(int? id)
        {
            List<UserFile>? list = await _context.UserFiles.Where(f => f.FileId == id).ToListAsync();
            if (list == null)
                return null;
            return list;
        }
    }
}
