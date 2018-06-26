using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Job.EthereumCore.Contracts.Events.LykkePay;
using Lykke.Job.PayTransactionHandler.Core.Exceptions;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.RabbitMqBroker;

namespace Lykke.Job.PayTransactionHandler.RabbitSubscribers
{
    public class EthereumEventsSubscriber : IStartable, IStopable
    {
        private readonly ILog _log;
        private readonly RabbitMqSettings _settings;
        private RabbitMqSubscriber<TransferEvent> _subscriber;
        private readonly IEthereumTransferHandler _ethereumTransferHandler;

        public EthereumEventsSubscriber(
            ILog log,
            RabbitMqSettings settings,
            IEthereumTransferHandler ethereumTransferHandler)
        {
            _log = log?.CreateComponentScope(nameof(EthereumEventsSubscriber)) ??
                   throw new ArgumentNullException(nameof(log));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _ethereumTransferHandler = ethereumTransferHandler ??
                                       throw new ArgumentNullException(nameof(ethereumTransferHandler));
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForSubscriber(_settings.ConnectionString, _settings.EthereumEventsExchangeName,
                    "paytransactionhandler");

            settings.MakeDurable();

            _subscriber = new RabbitMqSubscriber<TransferEvent>(settings,
                    new ResilientErrorHandlingStrategy(_log, settings,
                        retryTimeout: TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_log, settings)))
                .SetMessageDeserializer(new JsonMessageDeserializer<TransferEvent>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .SetLogger(_log)
                .Start();
        }

        private async Task ProcessMessageAsync(TransferEvent arg)
        {
            _log.WriteInfo(nameof(ProcessMessageAsync),
                $"message = {arg.ToJson()}", "Got new message from ethereum core");

            try
            {
                await _ethereumTransferHandler.Handle(arg);
            }
            catch (UnknownErc20TokenException tokenEx)
            {
                _log.WriteError(nameof(ProcessMessageAsync), new {tokenEx.TokenAddress}, tokenEx);

                throw;
            }
            catch (UnknownErc20AssetException assetEx)
            {
                _log.WriteError(nameof(ProcessMessageAsync), new {assetEx.Asset}, assetEx);

                throw;
            }
            catch (UnexpectedEthereumTransferTypeException typeEx)
            {
                _log.WriteError(nameof(ProcessMessageAsync), new {transferType = typeEx.TransferType.ToString()},
                    typeEx);

                throw;
            }
            catch (UnexpectedEthereumEventTypeException eventTypeEx)
            {
                _log.WriteError(nameof(ProcessMessageAsync), new {eventType = eventTypeEx.EventType.ToString()},
                    eventTypeEx);

                throw;
            }
        }

        public void Dispose()
        {
            _subscriber?.Dispose();
        }

        public void Stop()
        {
            _subscriber.Stop();
        }
    }
}
