using System.Threading.Tasks;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface IWalletsScanService
    {
        Task Execute();
    }
}
