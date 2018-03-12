using System.Collections.Generic;
using Lykke.Job.PayTransactionHandler.Core.Domain.Health;
using Lykke.Job.PayTransactionHandler.Core.Services;

namespace Lykke.Job.PayTransactionHandler.Services
{
    // NOTE: See https://lykkex.atlassian.net/wiki/spaces/LKEWALLET/pages/35755585/Add+your+app+to+Monitoring
    public class HealthService : IHealthService
    {
        public string GetHealthViolationMessage()
        {
            return null;
        }

        public IEnumerable<HealthIssue> GetHealthIssues()
        {
            var issues = new HealthIssuesCollection();

            return issues;
        }
    }
}
