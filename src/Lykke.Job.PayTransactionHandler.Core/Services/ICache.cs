using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface ICache<T>
    {
        Task AddAsync(T item);
        Task AddRangeAsync(IEnumerable<T> items);
        Task<IEnumerable<T>> GetAsync();
        Task RemoveAsync(T item);
        Task UpdateAsync(T item);
    }
}
