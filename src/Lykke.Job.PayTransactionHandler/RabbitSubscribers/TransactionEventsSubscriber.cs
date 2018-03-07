﻿using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Domain.TransactionStateCache;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.PayTransactionHandler.RabbitSubscribers
{
    //todo: replace this classs with the one from contracts package
    public class NewTransactionMessage
    {

    }

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
            //todo
            await _transactionsCache.Add(new TransactionState
            {
                Transaction = null
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
