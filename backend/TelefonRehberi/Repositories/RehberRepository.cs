namespace TelefonRehberi.Repositories
{
    using DocumentFormat.OpenXml.Spreadsheet;
    using Microsoft.EntityFrameworkCore;
    using TelefonRehberi.Models;

    public class RehberRepository : GenericRepository<Kisiler>, IRehberRepository
    {
        private readonly UygulamaDbContext _context;

        public RehberRepository(UygulamaDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Kisiler>> GetRehberByKullaniciIdAsync(int kullaniciId)
        {
            return await _context.Kisilers
                .Where(r => r.KullaniciId == kullaniciId)
                .ToListAsync();
        }
    }

}
