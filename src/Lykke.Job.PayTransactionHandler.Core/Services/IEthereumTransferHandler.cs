using System.Threading.Tasks;
using Lykke.Job.EthereumCore.Contracts.Events.LykkePay;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface IEthereumTransferHandler
    {
        Task Handle(TransferEvent transferEvent);
    }
}
