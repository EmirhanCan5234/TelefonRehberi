
using TelefonRehberi.Models;

namespace TelefonRehberi.Repositories
{
    public interface ILogRepository : IGenericRepository<Log>
    {
        Task<List<Log>> GetLastUserLogsAsync(int kullaniciId, int adet = 3);
    }

}
