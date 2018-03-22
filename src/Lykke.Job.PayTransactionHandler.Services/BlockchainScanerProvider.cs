using System;
using Autofac.Features.Indexed;
using Lykke.Job.PayTransactionHandler.Core;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.Services
{
    public class BlockchainScanerProvider : IBlockchainScanerProvider
    {
        private readonly IIndex<BlockchainScanType, IScanService> _scanServices;

        public BlockchainScanerProvider(IIndex<BlockchainScanType, IScanService> scanServices)
        {
            _scanServices = scanServices;
        }

        public IScanService Get(BlockchainScanType scanType)
        {
            if (!_scanServices.TryGetValue(scanType, out var service))
            {
                throw new InvalidOperationException($"Scan service of type [{scanType}] not found");
            }

            return service;
        }
    }
}
