using System;
using System.Collections.Generic;
using Lykke.Job.PayTransactionHandler.Core.Domain.DiffService;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface IDiffService<T> where T : IEquatable<T>
    {
        IEnumerable<DiffResult<T>> Diff(IEnumerable<T> initialState, IEnumerable<T> updatedState);
    }
}
