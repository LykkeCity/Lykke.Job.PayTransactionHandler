using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface ICacheMaintainer<T>
    {
        /// <summary>
        /// Warms up the cache 
        /// </summary>
        /// <returns></returns>
        Task WarmUpAsync();

        /// <summary>
        /// Clears outdated items from the cache
        /// </summary>
        /// <returns></returns>
        Task WipeAsync();

        /// <summary>
        /// Updates item in the cache
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        Task UpdateItemAsync(T item);

        /// <summary>
        /// Returns all the items from the cache
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAsync();
    }
}
