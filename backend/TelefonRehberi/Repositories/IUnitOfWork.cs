using TelefonRehberi.Models;
using System.Threading.Tasks;
namespace TelefonRehberi.Repositories
{
    public interface IUnitOfWork
    {
        IGenericRepository<Kisiler> Kisiler { get; }
        IGenericRepository<User> Users { get; }

        Task SaveAsync();
        Task CompleteAsync();
    }
}
