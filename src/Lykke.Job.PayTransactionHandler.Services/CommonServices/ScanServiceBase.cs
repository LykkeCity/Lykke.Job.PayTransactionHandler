using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.DiffService;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Service.PayInternal.Client;
using QBitNinja.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.PayTransactionHandler.Services.CommonServices
{
    public abstract class ScanServiceBase<T> : IScanService
    {
        protected readonly IPayInternalClient PayInternalClient;
        protected readonly QBitNinjaClient QBitNinjaClient;
        protected readonly ILog Log;

        protected const int BatchPieceSize = 15;

        protected ScanServiceBase(
            IPayInternalClient payInternalClient,
            QBitNinjaClient qBitNinjaClient,
            ILog log)
        {
            PayInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            QBitNinjaClient = qBitNinjaClient ?? throw new ArgumentNullException(nameof(qBitNinjaClient));
            Log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public abstract Task Execute();

        public abstract Task CreateTransaction(T tx);

        public abstract Task UpdateTransaction(T tx);

        public abstract Task<IEnumerable<T>> GetBcnTransactions(IEnumerable<string> data);

        protected async Task Broadcast(IEnumerable<DiffResult<T>> updated)
        {
            foreach (var diffResult in updated)
            {
                var tx = diffResult.Object;

                switch (diffResult.CompareState)
                {
                    case DiffState.New:

                        await CreateTransaction(tx);

                        break;

                    case DiffState.Updated:

                        await UpdateTransaction(tx);

                        break;

                    default: throw new Exception("Unknown transactions diff state");
                }
            }
        }
    }
}
