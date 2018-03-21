using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface IStorage<T>
    {
        Task<T> GetAsync(string id);
        Task SetAsync(string id, T model);
        Task<IEnumerable<T>> GetAsync();
        Task<bool> RemoveAsync(string id);
    }
}
