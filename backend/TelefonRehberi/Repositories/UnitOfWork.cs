using System.Threading.Tasks;
using TelefonRehberi.Models;
using TelefonRehberi.Repositories;

namespace TelefonRehberi.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly UygulamaDbContext _context;

        public IGenericRepository<Kisiler> Kisiler { get; }
        public IGenericRepository<User> Users { get; }

        public UnitOfWork(UygulamaDbContext context,
                          IGenericRepository<Kisiler> kisiler,
                          IGenericRepository<User> users)
        {
            _context = context;
            Kisiler = kisiler;
            Users = users;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
