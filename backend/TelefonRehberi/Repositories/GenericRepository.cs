using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using TelefonRehberi.Models;
namespace TelefonRehberi.Repositories
{
    // Repositories/GenericRepository.cs
  

    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly UygulamaDbContext _context;
        private readonly DbSet<T> _dbSet;

        public GenericRepository(UygulamaDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<(List<T> Data, int TotalCount)> GetPagedAsync(
    Expression<Func<T, bool>> filter,
    int page,
    int pageSize)
        {
            var query = _context.Set<T>().Where(filter);

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<T?> GetByIdAsync(int id) => await _dbSet.FindAsync(id);

        public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.FirstOrDefaultAsync(predicate);

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null)
        {
            if (filter != null)
                return await _dbSet.Where(filter).ToListAsync();
            return await _dbSet.ToListAsync();
        }


        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate) =>
            await _dbSet.Where(predicate).ToListAsync();

        public async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }

}
