using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.PayTransactionHandler.Commands;
using Lykke.Job.PayTransactionHandler.Core.Exceptions;
using Lykke.Service.Operations.Client;
using Lykke.Service.Operations.Contracts;
using Lykke.Service.PayInternal.Client;
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
            [NotNull] ILog log,
            [NotNull] IPayInternalClient payInternalClient,
            [NotNull] IOperationsClient operationsClient,
            int confirmationsToSucceed)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _payInternalClient = payInternalClient ?? throw new ArgumentNullException(nameof(payInternalClient));
            _operationsClient = operationsClient ?? throw new ArgumentNullException(nameof(operationsClient));
            _confirmationsToSucceed = confirmationsToSucceed;
        }

        public async Task<CommandHandlingResult> Handle(CreateLykkePaymentTransactionCommand cmd,
            IEventPublisher eventPublisher)
        {
            _log.WriteInfo(nameof(CreateLykkePaymentTransactionCommand), cmd, string.Empty);

            OperationModel operation = await _operationsClient.Get(cmd.OperationId);

            if (operation == null)
                throw new LykkePaymentOperationNotFoundException(cmd.OperationId);

            var context = JsonConvert.DeserializeObject<PaymentContext>(operation.ContextJson);

            string operationId = cmd.OperationId.ToString("D");

            await _payInternalClient.CreateLykkePaymentTransactionAsync(new CreateLykkeTransactionRequest
            {
                Amount = context.Amount,
                AssetId = context.AssetId,
                IdentityType = TransactionIdentityType.Specific,
                Identity = operationId,
                OperationId = operationId,
                SourceWalletAddresses =
                    operation.ClientId.HasValue ? new[] {operation.ClientId.Value.ToString("D")} : null,
                Confirmations = _confirmationsToSucceed
            });

            return CommandHandlingResult.Ok();
        }
    }
}
