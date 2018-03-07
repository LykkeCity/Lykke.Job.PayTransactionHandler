using System;
using Autofac.Features.Indexed;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.Services.CommonServices
{
    public class BlockchainScanerProvider : IBlockchainScanerProvider
    {
        private readonly IIndex<string, IScanService> _scanServices;

        public BlockchainScanerProvider(IIndex<string, IScanService> scanServices)
        {
            _scanServices = scanServices;
        }

        public IScanService Get(string scanType)
        {
            if (!_scanServices.TryGetValue(scanType, out var service))
            {
                throw new InvalidOperationException($"Scan service of type [{scanType}] not found");
            }

            return service;
        }
    }
}
