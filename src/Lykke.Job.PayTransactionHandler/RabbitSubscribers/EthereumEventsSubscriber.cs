using System;
using System.Net;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.PayTransactionHandler.Core.Settings.JobSettings;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Job.EthereumCore.Contracts.Events.LykkePay;
using Lykke.Job.PayTransactionHandler.Core;
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
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _settings;
        private RabbitMqSubscriber<TransferEvent> _subscriber;
        private readonly IEthereumTransferHandler _ethereumTransferHandler;

        public EthereumEventsSubscriber(
            [NotNull] ILogFactory logFactory,
            [NotNull] RabbitMqSettings settings,
            [NotNull] IEthereumTransferHandler ethereumTransferHandler)
        {
            _log = logFactory.CreateLog(this);
            _logFactory = logFactory ?? throw new ArgumentNullException(nameof(logFactory));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _ethereumTransferHandler = ethereumTransferHandler ??
                                       throw new ArgumentNullException(nameof(ethereumTransferHandler));
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings.CreateForSubscriber(
                _settings.EthereumConnectionString, 
                _settings.EthereumEventsExchangeName,
                _settings.EthereumEventsQueueName);

            settings.MakeDurable();

            var dlxErrorHandlingStrategy = new DeadQueueErrorHandlingStrategy(_logFactory, settings);

            _subscriber = new RabbitMqSubscriber<TransferEvent>(_logFactory, settings,
                    new EthereumEventsErrorHandlingStrategy(
                        new ResilientErrorHandlingStrategy(_logFactory, settings,
                            retryTimeout: TimeSpan.FromSeconds(10),
                            next: dlxErrorHandlingStrategy),
                        dlxErrorHandlingStrategy))
                .SetMessageDeserializer(new JsonMessageDeserializer<TransferEvent>())
                .SetMessageReadStrategy(new MessageReadQueueStrategy())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        private async Task ProcessMessageAsync(TransferEvent arg)
        {
            _log.Info("Got new message from ethereum core", arg.ToDetails());

            try
            {
                await _ethereumTransferHandler.Handle(arg);
            }
            catch (UnknownErc20TokenException e)
            {
                _log.Error(e, context: e.TokenAddress);

                throw;
            }
            catch (UnknownErc20AssetException e)
            {
                _log.Error(e, context: e.Asset);

                throw;
            }
            catch (UnexpectedEthereumTransferTypeException e)
            {
                _log.Error(e, context: e.TransferType.ToString());

                throw;
            }
            catch (UnexpectedEthereumEventTypeException e)
            {
                _log.Error(e, context: e.EventType.ToString());

                throw;
            }
            catch (DefaultErrorResponseException e) when (e.StatusCode == HttpStatusCode.BadRequest)
            {
                if (e.InnerException is ApiException apiException)
                {
                    _log.Error(e, context: new
                    {
                        message = e.Error?.ErrorMessage ?? apiException.Content,
                        errors = e.Error?.ModelErrors
                    }.ToDetails());
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
