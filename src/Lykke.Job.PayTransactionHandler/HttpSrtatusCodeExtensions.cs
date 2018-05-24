using System.Net;

namespace Lykke.Job.PayTransactionHandler
{
    public static class HttpSrtatusCodeExtensions
    {
        public static bool Is4xx(this HttpStatusCode statusCode)
        {
            return (int) statusCode >= 400 && (int) statusCode < 500;
        }
    }
}
