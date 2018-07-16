using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Job.PayTransactionHandler.Commands;
using Lykke.Job.PayTransactionHandler.Core.Exceptions;
using Lykke.Service.Operations.Client;
using Lykke.Service.Operations.Contracts;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInternal.Client.Exceptions;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using Newtonsoft.Json;

namespace Lykke.Job.PayTransactionHandler.Handlers
{
    public class LykkePaymentCommandHandler
    {
        private readonly ILog _log;
        private readonly IPayInternalClient _payInternalClient;
        private readonly IOperationsClient _operationsClient;
        private readonly int _confirmationsToSucceed;

        public LykkePaymentCommandHandler(
            [NotNull] ILogFactory logFactory,
            [NotNull] IPayInternalClient payInternalClient,
            [NotNull] IOperationsClient operationsClient,
            int confirmationsToSucceed)
        {
            _log = logFactory.CreateLog(this);
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _operationsClient = operationsClient ?? throw new ArgumentNullException(nameof(operationsClient));
            _confirmationsToSucceed = confirmationsToSucceed;
        }

        public async Task<CommandHandlingResult> Handle(CreateLykkePaymentTransactionCommand cmd,
            IEventPublisher eventPublisher)
        {
            _log.Info("Handle new Lykke payment command", cmd);

            OperationModel operation = await _operationsClient.Get(cmd.OperationId);

            if (operation == null)
                throw new LykkePaymentOperationNotFoundException(cmd.OperationId);

            var context = JsonConvert.DeserializeObject<PaymentContext>(operation.ContextJson);

            // handling only payment operations
            if (context == null)
                return CommandHandlingResult.Ok();

            string operationId = cmd.OperationId.ToString("D");

            var request = new CreateLykkeTransactionRequest
            {
                Amount = context.Amount,
                AssetId = context.AssetId,
                IdentityType = TransactionIdentityType.Specific,
                Identity = operationId,
                OperationId = operationId,
                SourceWalletAddresses =
                    operation.ClientId.HasValue ? new[] {operation.ClientId.Value.ToString("D")} : null,
                Confirmations = _confirmationsToSucceed
            };

            try
            {
                await _payInternalClient.CreateLykkePaymentTransactionAsync(request);
            }
            catch (DefaultErrorResponseException e)
            {
                if (e.StatusCode.Is4xx())
                {
                    _log.Error(e, context: request);

                    return CommandHandlingResult.Ok();
                }

                throw;
            }

            return CommandHandlingResult.Ok();
        }
    }
}
