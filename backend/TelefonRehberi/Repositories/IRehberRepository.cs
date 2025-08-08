namespace TelefonRehberi.Repositories
{
    using DocumentFormat.OpenXml.Spreadsheet;
    using TelefonRehberi.Models;

    public interface IRehberRepository : IGenericRepository<Kisiler>
    {
        Task<List<Kisiler>> GetRehberByKullaniciIdAsync(int kullaniciId);
    }

}
