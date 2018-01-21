using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface IStorage<T>
    {
        Task<T> Get(string id);
        Task Set(string id, T model);
        Task<IEnumerable<T>> Get();
        Task<bool> Remove(string id);
    }
}
