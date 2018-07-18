using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.Common;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PayInternal.Contract;
using BlockchainType = Lykke.Job.PayTransactionHandler.Core.BlockchainType;

namespace Lykke.Job.PayTransactionHandler.RabbitSubscribers
{
    public class TransactionEventsSubscriber: IStartable, IStopable
    {
        private readonly ILog _log;
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _settings;
        private RabbitMqSubscriber<NewTransactionMessage> _subscriber;
        private readonly ICacheMaintainer<TransactionState> _transactionsCache;

        public TransactionEventsSubscriber(
            [NotNull] ILogFactory logFactory,
            [NotNull] RabbitMqSettings settings, 
            [NotNull] ICacheMaintainer<TransactionState> transactionsCache)
        {
            _log = logFactory.CreateLog(this);
            _logFactory = logFactory;
            _settings = settings;
            _transactionsCache = transactionsCache;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForSubscriber(_settings.LykkePayConnectionString, _settings.TransactionsExchangeName, "paytransactionhandler");

            settings.MakeDurable();

            _subscriber = new RabbitMqSubscriber<NewTransactionMessage>(_logFactory, settings,
                    new ResilientErrorHandlingStrategy(_logFactory, settings,
                        retryTimeout: TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_logFactory, settings)))
                .SetMessageDeserializer(new JsonMessageDeserializer<NewTransactionMessage>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        private async Task ProcessMessageAsync(NewTransactionMessage arg)
        {
            _log.Info("Got a message about new transaction", arg);

            await _transactionsCache.SetItemAsync(new TransactionState
            {
                Transaction = new BcnTransaction
                {
                    Id = arg.Id,
                    Amount = arg.Amount,
                    Confirmations = arg.Confirmations,
                    BlockId = arg.BlockId,
                    Blockchain = Enum.Parse<BlockchainType>(arg.Blockchain.ToString()),
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
