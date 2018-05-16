using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings;
using Lykke.Job.PayTransactionHandler.Services;
using Lykke.SettingsReader;
using Lykke.Job.PayTransactionHandler.PeriodicalHandlers;
using Lykke.Job.PayTransactionHandler.RabbitSubscribers;
using Lykke.Job.PayTransactionHandler.Core.Settings;
using Lykke.Service.PayInternal.Client;
using Microsoft.Extensions.DependencyInjection;
using QBitNinja.Client;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Services.Ethereum;
using Lykke.Job.PayTransactionHandler.Services.Transactions;
using Lykke.Job.PayTransactionHandler.Services.Wallets;
using Lykke.Service.Assets.Client;

namespace Lykke.Job.PayTransactionHandler.Modules
{
    public class JobModule : Module
    {
        private readonly AppSettings _settings;
        private readonly IReloadingManager<DbSettings> _dbSettingsManager;
        private readonly ILog _log;
        private readonly IServiceCollection _services;

        public JobModule(AppSettings settings, IReloadingManager<DbSettings> dbSettingsManager, ILog log)
        {
            _settings = settings;
            _log = log;
            _dbSettingsManager = dbSettingsManager;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .SingleInstance();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .SingleInstance();

            RegisterPeriodicalHandlers(builder);

            RegisterRabbitMqSubscribers(builder);

            RegisterServiceClients(builder);

            RegisterDomainServices(builder);

            builder.Populate(_services);
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<WalletsScanHandler>()
                .As<IStartable>()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.WalletsScanPeriod))
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<TransactionsScanHandler>()
                .As<IStartable>()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.TransactionsScanPeriod))
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<CacheOutdateHandler>()
                .As<IStartable>()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.CacheExpirationHandlingPeriod))
                .AutoActivate()
                .SingleInstance();
        }

        private void RegisterRabbitMqSubscribers(ContainerBuilder builder)
        {
            builder.RegisterType<WalletEventsSubscriber>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.Rabbit));

            builder.RegisterType<TransactionEventsSubscriber>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.Rabbit));

            builder.RegisterType<EthereumEventsSubscriber>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.Rabbit));
        }

        private void RegisterServiceClients(ContainerBuilder builder)
        {
            builder.RegisterType<PayInternalClient>()
                .WithParameter(TypedParameter.From(_settings.PayInternalServiceClient))
                .As<IPayInternalClient>();

            builder.RegisterInstance(new QBitNinjaClient(_settings.NinjaServiceClient.ServiceUrl))
                .AsSelf();

            builder.RegisterInstance<IAssetsService>(
                new AssetsService(new Uri(_settings.AssetsServiceClient.ServiceUrl)));
        }

        private void RegisterDomainServices(ContainerBuilder builder)
        {
            builder.RegisterType<WalletsScanService>()
                .Keyed<IScanService>(BlockchainScanType.Wallet)
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.Bitcoin.Network))
                .SingleInstance();

            builder.RegisterType<TransactionsScanService>()
                .Keyed<IScanService>(BlockchainScanType.Transaction)
                .SingleInstance();

            builder.RegisterType<BlockchainScanerProvider>()
                .As<IBlockchainScanerProvider>()
                .SingleInstance();

            builder.RegisterType<WalletsStateCacheMaintainer>()
                .As<ICacheMaintainer<WalletState>>()
                .SingleInstance();

            builder.RegisterType<TransactionStateCacheMaintainer>()
                .As<ICacheMaintainer<TransactionState>>()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.Blockchain.ConfirmationsToSucceed))
                .SingleInstance();

            builder.RegisterType<PaymentTxStateDiffService>()
                .WithParameter(
                    TypedParameter.From(_settings.PayTransactionHandlerJob.Blockchain.ConfirmationsToSucceed))
                .As<IDiffService<PaymentBcnTransaction>>()
                .SingleInstance();

            builder.RegisterType<TxStateDiffService>()
                .WithParameter(
                    TypedParameter.From(_settings.PayTransactionHandlerJob.Blockchain.ConfirmationsToSucceed))
                .As<IDiffService<BcnTransaction>>()
                .SingleInstance();

            builder.RegisterType<EthereumTransferHandler>()
                .WithParameter(
                    TypedParameter.From(_settings.PayTransactionHandlerJob.Blockchain.ConfirmationsToSucceed))
                .As<IEthereumTransferHandler>();
        }
    }
}
