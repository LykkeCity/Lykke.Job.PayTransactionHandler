﻿using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Lykke.Job.PayTransactionHandler.Core;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Services;
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
using Lykke.Service.Operations.Client;
using Lykke.SettingsReader;

namespace Lykke.Job.PayTransactionHandler.Modules
{
    public class JobModule : Module
    {
        private readonly AppSettings _settings;
        private readonly IServiceCollection _services;

        public JobModule(IReloadingManager<AppSettings> settingsManager)
        {
            _settings = settingsManager.CurrentValue;
            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
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
                .As<IStartable, IStopable>()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.WalletsScanPeriod))
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<TransactionsScanHandler>()
                .As<IStartable, IStopable>()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.TransactionsScanPeriod))
                .AutoActivate()
                .SingleInstance();

            builder.RegisterType<CacheOutdateHandler>()
                .As<IStartable, IStopable>()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.CacheExpirationHandlingPeriod))
                .AutoActivate()
                .SingleInstance();
        }

        private void RegisterRabbitMqSubscribers(ContainerBuilder builder)
        {
            builder.RegisterType<WalletEventsSubscriber>()
                .As<IStartable, IStopable>()
                .AutoActivate()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.Rabbit));

            builder.RegisterType<TransactionEventsSubscriber>()
                .As<IStartable, IStopable>()
                .AutoActivate()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.Rabbit));

            builder.RegisterType<EthereumEventsSubscriber>()
                .As<IStartable, IStopable>()
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

            builder.RegisterOperationsClient(_settings.OperationsServiceClient.ServiceUrl);
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
                .As<IEthereumTransferHandler>();
        }
    }
}
