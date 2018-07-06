using System;
using System.Net;
using System.Threading;
using JetBrains.Annotations;
using Lykke.RabbitMqBroker;
using Lykke.Service.PayInternal.Client.Exceptions;

namespace Lykke.Job.PayTransactionHandler.ErrorHandling
{
    public class EthereumEventsErrorHandlingStrategy : IErrorHandlingStrategy
    {
        private readonly IErrorHandlingStrategy _defaultStrategy;
        private readonly IErrorHandlingStrategy _badRequestStrategy;

        public EthereumEventsErrorHandlingStrategy(
            [NotNull] IErrorHandlingStrategy defaultStrategy, 
            [NotNull] IErrorHandlingStrategy badRequestStrategy)
        {
            _defaultStrategy = defaultStrategy;
            _badRequestStrategy = badRequestStrategy;
        }

        public void Execute(Action handler, IMessageAcceptor ma, CancellationToken cancellationToken)
        {
            try
            {
                handler();
            }
            catch (DefaultErrorResponseException e) when (e.StatusCode == HttpStatusCode.BadRequest)
            {
                _badRequestStrategy.Execute(handler, ma, cancellationToken);
            }
            catch (Exception)
            {
                _defaultStrategy.Execute(handler, ma, cancellationToken);
            }
        }
    }
}
