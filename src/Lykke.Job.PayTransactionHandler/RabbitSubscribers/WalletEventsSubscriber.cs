using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.WalletsStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PayInternal.Contract;

namespace Lykke.Job.PayTransactionHandler.RabbitSubscribers
{
    public class WalletEventsSubscriber : IStartable, IStopable
    {
        private readonly ILog _log;
        private readonly RabbitMqSettings _settings;
        private RabbitMqSubscriber<NewWalletMessage> _subscriber;
        private readonly ICache<WalletState> _walletsCache;

        public WalletEventsSubscriber(ILog log, RabbitMqSettings settings, ICache<WalletState> walletsCache)
        {
            _log = log;
            _settings = settings;
            _walletsCache = walletsCache;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForSubscriber(_settings.ConnectionString, _settings.WalletsExchangeName, "paytransactionhandler");

            settings.MakeDurable();

            _subscriber = new RabbitMqSubscriber<NewWalletMessage>(settings,
                    new ResilientErrorHandlingStrategy(_log, settings,
                        retryTimeout: TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_log, settings)))
                .SetMessageDeserializer(new JsonMessageDeserializer<NewWalletMessage>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .SetLogger(_log)
                .Start();
        }

        private async Task ProcessMessageAsync(NewWalletMessage arg)
        {
            await _walletsCache.AddAsync(new WalletState
            {
                Address = arg.Address,
                DueDate = arg.DueDate,
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
