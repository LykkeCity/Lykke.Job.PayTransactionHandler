using System;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.PayTransactionHandler.Commands;
using Lykke.Service.Operations.Contracts.Events;

namespace Lykke.Job.PayTransactionHandler.Sagas
{
    public class LykkePaymentOperationSaga
    {
        private readonly ILog _log;

        public LykkePaymentOperationSaga([NotNull] ILog log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void Handle(OperationCompletedEvent evt, ICommandSender sender)
        {
            _log.WriteInfo(nameof(OperationCompletedEvent), evt.ToJson(), string.Empty);

            sender.SendCommand(new CreateLykkePaymentTransactionCommand
            {
                OperationId = evt.Id
            }, BoundedContexts.Payments);
        }
    }
}
