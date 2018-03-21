using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings;
using Lykke.Job.PayTransactionHandler.Services;
using Lykke.SettingsReader;
using Lykke.Job.PayTransactionHandler.PeriodicalHandlers;
using Lykke.Job.PayTransactionHandler.RabbitSubscribers;
using Lykke.Job.PayTransactionHandler.RabbitPublishers;
using Lykke.Job.PayTransactionHandler.Core.Settings;
using Lykke.Service.PayInternal.Client;
using Microsoft.Extensions.DependencyInjection;
using QBitNinja.Client;

namespace Lykke.Job.PayTransactionHandler.Modules
{
    public class JobModule : Module
    {
        private readonly AppSettings _settings;
        private readonly IReloadingManager<DbSettings> _dbSettingsManager;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
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
            // NOTE: Do not register entire settings in container, pass necessary settings to services which requires them
            // ex:
            // builder.RegisterType<QuotesPublisher>()
            //  .As<IQuotesPublisher>()
            //  .WithParameter(TypedParameter.From(_settings.Rabbit.ConnectionString))

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();
            RegisterPeriodicalHandlers(builder);
            RegisterRabbitMqSubscribers(builder);
            RegisterRabbitMqPublishers(builder);
            RegisterServiceClients(builder);
            RegisterDomainServices(builder);

            builder.Populate(_services);
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            // TODO: You should register each periodical handler in DI container as IStartable singleton and autoactivate it

            builder.RegisterType<WalletsScanHandler>()
                .As<IStartable>()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.WalletsScanPeriod))
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
        }

        private void RegisterRabbitMqPublishers(ContainerBuilder builder)
        {
            // TODO: You should register each publisher in DI container as publisher specific interface and as IStartable,
            // as singleton and do not autoactivate it

            builder.RegisterType<MyRabbitPublisher>()
                .As<IMyRabbitPublisher>()
                .As<IStartable>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.Rabbit.ConnectionString));
        }

        private void RegisterServiceClients(ContainerBuilder builder)
        {
            builder.RegisterType<PayInternalClient>()
                .WithParameter(TypedParameter.From(_settings.PayInternalServiceClient))
                .As<IPayInternalClient>();

            builder.RegisterInstance(new QBitNinjaClient(_settings.NinjaServiceClient.ServiceUrl))
                .AsSelf();
        }

        private void RegisterDomainServices(ContainerBuilder builder)
        {
            builder.RegisterType<WalletsScanService>()
                .As<IWalletsScanService>()
                .WithParameter(TypedParameter.From(_settings.PayTransactionHandlerJob.Bitcoin.Network))
                .SingleInstance();

            builder.RegisterType<InMemoryStorage<WalletState>>()
                .As<IStorage<WalletState>>()
                .SingleInstance();

            builder.RegisterType<WalletsStateCache>()
                .As<IWalletsStateCache>()
                .SingleInstance();

            builder.RegisterType<WalletsStateCacheManager>()
                .As<IWalletsStateCacheManager>()
                .SingleInstance();

            builder.RegisterType<TransactionStateDiffService>()
                .WithParameter(
                    TypedParameter.From(_settings.PayTransactionHandlerJob.Blockchain.ConfirmationsToSucceed))
                .As<IDiffService<BlockchainTransaction>>()
                .SingleInstance();
        }
    }
}
