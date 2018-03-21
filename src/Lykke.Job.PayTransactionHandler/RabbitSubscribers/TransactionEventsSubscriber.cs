using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PayInternal.Contract;

namespace Lykke.Job.PayTransactionHandler.RabbitSubscribers
{
    public class TransactionEventsSubscriber: IStartable, IStopable
    {
        private readonly ILog _log;
        private readonly RabbitMqSettings _settings;
        private RabbitMqSubscriber<NewTransactionMessage> _subscriber;
        private readonly ICache<TransactionState> _transactionsCache;

        public TransactionEventsSubscriber(ILog log, RabbitMqSettings settings, ICache<TransactionState> transactionsCache)
        {
            _log = log;
            _settings = settings;
            _transactionsCache = transactionsCache;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForSubscriber(_settings.ConnectionString, _settings.TransactionsExchangeName, "paytransactionhandler");

            settings.MakeDurable();

            _subscriber = new RabbitMqSubscriber<NewTransactionMessage>(settings,
                    new ResilientErrorHandlingStrategy(_log, settings,
                        retryTimeout: TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_log, settings)))
                .SetMessageDeserializer(new JsonMessageDeserializer<NewTransactionMessage>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .SetLogger(_log)
                .Start();
        }

        private async Task ProcessMessageAsync(NewTransactionMessage arg)
        {
            await _transactionsCache.AddAsync(new TransactionState
            {
                Transaction = new BcnTransaction
                {
                    Id = arg.Id,
                    Amount = arg.Amount,
                    Confirmations = arg.Confirmations,
                    BlockId = arg.BlockId,
                    Blockchain = arg.Blockchain,
                    AssetId = arg.AssetId
                },
                DueDate = arg.DueDate
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
