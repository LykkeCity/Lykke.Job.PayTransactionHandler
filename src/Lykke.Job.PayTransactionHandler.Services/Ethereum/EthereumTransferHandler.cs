using System;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Job.EthereumCore.Contracts.Enums.LykkePay;
using Lykke.Job.EthereumCore.Contracts.Events.LykkePay;
using Lykke.Job.PayTransactionHandler.Core.Exceptions;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Models.Transactions;

namespace Lykke.Job.PayTransactionHandler.Services.Ethereum
{
    public class EthereumTransferHandler : IEthereumTransferHandler
    {
        private readonly ILog _log;
        private readonly IPayInternalClient _payInternalClient;
        private readonly int _confirmationsToSucceed;
        private readonly IAssetsService _assetsService;

        public EthereumTransferHandler(
            ILog log,
            IPayInternalClient payInternalClient,
            int confirmationsToSucceed,
            IAssetsService assetsService)
        {
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _confirmationsToSucceed = confirmationsToSucceed;
            _assetsService = assetsService;
            _log = log?.CreateComponentScope(nameof(EthereumTransferHandler)) ??
                   throw new ArgumentNullException(nameof(EthereumTransferHandler));
        }

        public async Task Handle(TransferEvent transferEvent)
        {
            if (transferEvent == null) return;

            Erc20Token token = await _assetsService.Erc20TokenGetByAddressAsync(transferEvent.TokenAddress);

            if (token == null)
                throw new UnknownErc20TokenException(transferEvent.TokenAddress);

            Asset asset = await _assetsService.AssetGetAsync(token.AssetId);

            switch (transferEvent.SenderType)
            {
                // successful payment transaction
                case SenderType.Customer:

                    var createTransactionRequest = Mapper.Map<CreateTransactionRequest>(
                        transferEvent, opt =>
                        {
                            opt.Items["AssetId"] = token.AssetId;
                            opt.Items["ConfirmationsToSucceed"] = _confirmationsToSucceed;
                            opt.Items["AssetMultiplier"] = asset?.MultiplierPower ?? 0;
                            opt.Items["AssetAccuracy"] = asset?.Accuracy ?? 0;
                        });

                    await _payInternalClient.CreatePaymentTransactionAsync(createTransactionRequest);

                    break;

                // transfer from deposit contract
                case SenderType.EthereumCore:

                    var updateTransactionRequest = Mapper.Map<UpdateTransactionRequest>(transferEvent,
                        opt =>
                        {
                            opt.Items["ConfirmationsToSucceed"] = _confirmationsToSucceed;
                            opt.Items["AssetMultiplier"] = asset?.MultiplierPower ?? 0;
                            opt.Items["AssetAccuracy"] = asset?.Accuracy ?? 0;
                        });

                    await _payInternalClient.UpdateTransactionAsync(updateTransactionRequest);

                    break;

                default:
                    throw new UnexpectedEthereumTransferType(transferEvent.SenderType);
            }
        }
    }
}
