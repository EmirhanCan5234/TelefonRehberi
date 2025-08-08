using Microsoft.EntityFrameworkCore;
using System;

using TelefonRehberi.Models;

namespace TelefonRehberi.Repositories
{
   
    public class LogRepository : GenericRepository<Log>, ILogRepository
    {
        private readonly UygulamaDbContext _context;

        public LogRepository(UygulamaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Log>> GetLastUserLogsAsync(int kullaniciId, int adet = 3)
        {
            return await _context.Logs
                .Where(x => x.KullaniciId == kullaniciId &&
                            (x.Islem == "Kişi Eklendi" || x.Islem == "Kişi Güncellendi" || x.Islem == "Kişi Silindi"||x.Islem=="Giriş Başarısız"))
                .OrderByDescending(x => x.Tarih)
                .Take(adet)
                .ToListAsync();
        }
    }

}
