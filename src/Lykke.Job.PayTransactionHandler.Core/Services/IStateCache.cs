using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface IStateCache<T>
    {
        Task Add(T item);
        Task AddRange(IEnumerable<T> items);
        Task<IEnumerable<T>> Get();
        Task Remove(T item);
        Task Update(T item);
    }
}
