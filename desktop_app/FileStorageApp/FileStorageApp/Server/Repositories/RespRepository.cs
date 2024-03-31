using FileStorageApp.Server.Database;
using FileStorageApp.Server.Entity;
using Microsoft.EntityFrameworkCore;

namespace FileStorageApp.Server.Repositories
{
    public class RespRepository
    {
        DataContext _context;

        public RespRepository(DataContext context)
        {
            _context = context;
        }

        public async Task SaveResp(Resp resp)
        {
            _context.Resps.Add(resp);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateResp(Resp resp)
        {
            _context.Resps.Update(resp);
            await _context.SaveChangesAsync();
        }
        public async Task SaveMultipleResps(List<Resp> respList)
        {
            await _context.AddRangeAsync(respList);
            await _context.SaveChangesAsync();
        }

        public async Task<Resp?> GetRespById(int id)
        {
            Resp resp = await _context.Resps.Where(r => r.Id == id).FirstOrDefaultAsync();
            return resp;
        }
    }
}
