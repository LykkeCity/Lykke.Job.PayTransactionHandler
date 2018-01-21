namespace Lykke.Job.PayTransactionHandler.Core.Domain.DiffService
{
    public class DiffResult<T>
    {
        public DiffState CompareState { get; set; }
        public T Object { get; set; }
    }
}
