using FileStorageApp.Server.Database;
using FileStorageApp.Server.Entity;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

namespace FileStorageApp.Server.Repositories
{
    public class FileTransferRepo
    {
        DataContext _context;
        public FileTransferRepo(DataContext context)
        {
            _context = context;
        }

        public async Task SaveFileTransfer(FileTransfer fileTransfer)
        {
            try
            {
                _context.FileTransfers.Add(fileTransfer);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception("Error saving file transfer");
            }
    
        }

        public async Task<FileTransfer?> GetFileTransferBySendIdRecIdFilename(int senderId, int recId, string fileName)
        {
            try
            {
                FileTransfer? ft =  await _context.FileTransfers.Where(x => x.SenderId == senderId && x.ReceiverId == recId && x.FileName == fileName).FirstOrDefaultAsync();
                return ft;
            }
            catch (Exception e)
            {
                throw new Exception("Error getting file transfer");
            }
        }

        public async Task<List<FileTransfer>?> GetFileTransferByRecId(int recId)
        {
            try
            {
                List<FileTransfer>? ft =  await _context.FileTransfers.Where(x => x.ReceiverId == recId).ToListAsync();
                return ft;
            }
            catch (Exception e)
            {
                throw new Exception("Error getting file transfer");
            }
        }

        public async Task DeleteFileTransfer(FileTransfer ft)
        {
            try
            {
                _context.FileTransfers.Remove(ft);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception("Error deleting file transfer");
            }
        }
    }
}
