using System;
using MessagePack;

namespace Lykke.Job.PayTransactionHandler.Commands
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class CreateLykkePaymentTransactionCommand
    {
        public Guid OperationId { get; set; }
    }
}
