using System.Linq.Expressions;

namespace TelefonRehberi.Repositories
{

    public interface IGenericRepository<T> where T : class
    {
        Task<(List<T> Data, int TotalCount)> GetPagedAsync(
    Expression<Func<T, bool>> filter,
    int page,
    int pageSize
);

        Task<T?> GetByIdAsync(int id);
        Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        void Update(T entity);
        void Remove(T entity);

        Task SaveAsync();
    }

}
