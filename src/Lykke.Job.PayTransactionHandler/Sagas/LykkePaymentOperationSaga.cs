using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Job.PayTransactionHandler.Commands;
using Lykke.Service.Operations.Contracts.Events;

namespace Lykke.Job.PayTransactionHandler.Sagas
{
    public class LykkePaymentOperationSaga
    {
        private readonly ILog _log;

        public LykkePaymentOperationSaga(
            [NotNull] ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
        }

        public void Handle(OperationCompletedEvent evt, ICommandSender sender)
        {
            _log.Info("Handle Lykke payment completed event", evt);

            sender.SendCommand(new CreateLykkePaymentTransactionCommand
            {
                OperationId = evt.Id
            }, BoundedContexts.Payments);
        }
    }
}
