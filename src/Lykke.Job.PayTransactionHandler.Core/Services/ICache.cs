using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface ICache<T>
    {
        Task Add(T item);
        Task AddRange(IEnumerable<T> items);
        Task<IEnumerable<T>> Get();
        Task Remove(T item);
        Task Update(T item);
    }
}
