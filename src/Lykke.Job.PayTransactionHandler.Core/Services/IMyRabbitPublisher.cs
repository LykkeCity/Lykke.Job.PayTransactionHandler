using System.Threading.Tasks;
using Autofac;
using Common;
using Lykke.Job.PayTransactionHandler.Contract;

namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface IMyRabbitPublisher : IStartable, IStopable
    {
        Task PublishAsync(MyPublishedMessage message);
    }
}