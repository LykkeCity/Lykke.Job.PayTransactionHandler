using System.Threading.Tasks;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}