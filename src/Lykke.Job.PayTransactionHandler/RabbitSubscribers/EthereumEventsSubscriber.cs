using System;
using System.Net;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Job.EthereumCore.Contracts.Events.LykkePay;
using Lykke.Job.PayTransactionHandler.Core.Exceptions;
using Lykke.Job.PayTransactionHandler.Core.Services;
using Lykke.Job.PayTransactionHandler.ErrorHandling;
using Lykke.RabbitMqBroker;
using Lykke.Service.PayInternal.Client.Exceptions;
using Refit;

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
                .CreateForSubscriber(_settings.EthereumConnectionString, _settings.EthereumEventsExchangeName,
                    "paytransactionhandler");

            settings.MakeDurable();

            var dlxErrorHandlingStrategy = new DeadQueueErrorHandlingStrategy(_log, settings);

            _subscriber = new RabbitMqSubscriber<TransferEvent>(settings,
                    new EthereumEventsErrorHandlingStrategy(
                        new ResilientErrorHandlingStrategy(_log, settings,
                            retryTimeout: TimeSpan.FromSeconds(10),
                            next: dlxErrorHandlingStrategy),
                        dlxErrorHandlingStrategy))
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
            catch (UnknownErc20TokenException e)
            {
                _log.WriteError(nameof(ProcessMessageAsync), new {e.TokenAddress}, e);

                throw;
            }
            catch (UnknownErc20AssetException e)
            {
                _log.WriteError(nameof(ProcessMessageAsync), new {e.Asset}, e);

                throw;
            }
            catch (UnexpectedEthereumTransferTypeException e)
            {
                _log.WriteError(nameof(ProcessMessageAsync), new {transferType = e.TransferType.ToString()}, e);

                throw;
            }
            catch (UnexpectedEthereumEventTypeException e)
            {
                _log.WriteError(nameof(ProcessMessageAsync), new {eventType = e.EventType.ToString()}, e);

                throw;
            }
            catch (DefaultErrorResponseException e) when (e.StatusCode == HttpStatusCode.BadRequest)
            {
                if (e.InnerException is ApiException apiException)
                {
                    _log.WriteError(nameof(ProcessMessageAsync), new
                    {
                        message = e.Error?.ErrorMessage ?? apiException.Content,
                        errors = e.Error?.ModelErrors
                    }, e);
                }

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
