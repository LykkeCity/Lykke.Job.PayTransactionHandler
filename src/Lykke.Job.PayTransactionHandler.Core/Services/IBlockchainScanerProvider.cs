namespace Lykke.Job.PayTransactionHandler.Core.Services
{
    public interface IBlockchainScanerProvider
    {
        IScanService Get(string scanType);
    }
}
