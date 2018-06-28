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
using Lykke.Service.PayInternal.Client.Models.Transactions.Ethereum;

namespace Lykke.Job.PayTransactionHandler.Services.Ethereum
{
    public class EthereumTransferHandler : IEthereumTransferHandler
    {
        private readonly ILog _log;
        private readonly IPayInternalClient _payInternalClient;
        private readonly IAssetsService _assetsService;

        public EthereumTransferHandler(
            ILog log,
            IPayInternalClient payInternalClient,
            IAssetsService assetsService)
        {
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
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

            if (asset == null)
                throw new UnknownErc20AssetException(token.AssetId);

            switch (transferEvent.SenderType)
            {
                case SenderType.Customer:
                    var inboundTransactionRequest = Mapper.Map<RegisterInboundTxModel>(
                        transferEvent, opt =>
                        {
                            opt.Items["AssetId"] = asset.DisplayId;
                            opt.Items["AssetMultiplier"] = asset.MultiplierPower;
                            opt.Items["AssetAccuracy"] = asset.Accuracy;
                        });
                    await _payInternalClient.RegisterEthereumInboundTransactionAsync(inboundTransactionRequest);
                    break;

                case SenderType.EthereumCore:
                    switch (transferEvent.EventType)
                    {
                        case EventType.Started:
                            var outboundTransactionRequest = Mapper.Map<RegisterOutboundTxModel>(
                                transferEvent, opt =>
                                {
                                    opt.Items["AssetId"] = asset.DisplayId;
                                    opt.Items["AssetMultiplier"] = asset.MultiplierPower;
                                    opt.Items["AssetAccuracy"] = asset.Accuracy;
                                });
                            await _payInternalClient.RegisterEthereumOutboundTransactionAsync(
                                outboundTransactionRequest);
                            break;
                        case EventType.Failed:
                            await _payInternalClient.FailEthereumOutboundTransactionAsync(
                                Mapper.Map<FailOutboundTxModel>(transferEvent));
                            break;
                        case EventType.Completed:
                            await _payInternalClient.CompleteEthereumOutboundTransactionAsync(
                                Mapper.Map<CompleteOutboundTxModel>(transferEvent, opt =>
                                {
                                    opt.Items["AssetMultiplier"] = asset.MultiplierPower;
                                    opt.Items["AssetAccuracy"] = asset.Accuracy;
                                }));
                            break;
                        case EventType.NotEnoughFunds:
                            await _payInternalClient.FailEthereumOutboundTransactionAsync(
                                Mapper.Map<NotEnoughFundsOutboundTxModel>(transferEvent));
                            break;
                        default: throw new UnexpectedEthereumEventTypeException(transferEvent.EventType);
                    }

                    break;

                default:
                    throw new UnexpectedEthereumTransferTypeException(transferEvent.SenderType);
            }
        }
    }
}
