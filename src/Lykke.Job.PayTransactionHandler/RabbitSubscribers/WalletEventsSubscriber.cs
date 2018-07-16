using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PayInternal.Contract;
using BlockchainType = Lykke.Job.PayTransactionHandler.Core.BlockchainType;

namespace Lykke.Job.PayTransactionHandler.RabbitSubscribers
{
    public class WalletEventsSubscriber : IStartable, IStopable
    {
        private readonly ILog _log;
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _settings;
        private RabbitMqSubscriber<NewWalletMessage> _subscriber;
        private readonly ICacheMaintainer<WalletState> _walletsCache;

        public WalletEventsSubscriber(
            [NotNull] ILogFactory logFactory, 
            [NotNull] RabbitMqSettings settings, 
            [NotNull] ICacheMaintainer<WalletState> walletsCache)
        {
            _log = logFactory.CreateLog(this);
            _logFactory = logFactory;
            _settings = settings;
            _walletsCache = walletsCache;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForSubscriber(_settings.ConnectionString, _settings.WalletsExchangeName, "paytransactionhandler");

            settings.MakeDurable();

            _subscriber = new RabbitMqSubscriber<NewWalletMessage>(_logFactory, settings,
                    new ResilientErrorHandlingStrategy(_logFactory, settings,
                        retryTimeout: TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_logFactory, settings)))
                .SetMessageDeserializer(new JsonMessageDeserializer<NewWalletMessage>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        private async Task ProcessMessageAsync(NewWalletMessage arg)
        {
            _log.Info("Got a message about new wallet", arg);

            await _walletsCache.SetItemAsync(new WalletState
            {
                Address = arg.Address,
                DueDate = arg.DueDate,
                Blockchain = Enum.Parse<BlockchainType>(arg.Blockchain.ToString()),
                Transactions = Enumerable.Empty<PaymentBcnTransaction>()
            });
        }

        public void Dispose()
        {
            _subscriber?.Dispose();
        }

        public void Stop()
        {
            _subscriber?.Stop();
        }
    }
}
