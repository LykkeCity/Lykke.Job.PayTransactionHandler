using Common;

namespace Lykke.Job.PayTransactionHandler.Core
{
    public static class LogExtensions
    {
        public static string ToDetails(this object src)
        {
            return $"Details: {src?.ToJson()}";
        }
    }
}
