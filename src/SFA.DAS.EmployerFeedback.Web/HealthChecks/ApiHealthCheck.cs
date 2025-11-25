using Microsoft.Extensions.Diagnostics.HealthChecks;
using SFA.DAS.EmployerFeedback.Infrastructure.Api.OuterApi;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerFeedback.Web.HealthChecks
{
    [ExcludeFromCodeCoverage]
    public class ApiHealthCheck : IHealthCheck
    {
        private readonly IEmployerFeedbackOuterApi _outerApi;

        public ApiHealthCheck(IEmployerFeedbackOuterApi outerApi)
        {
            _outerApi = outerApi;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
        {
            var description = "Ping of Employer Feedback Web outer API";

            try
            {
                await _outerApi.Ping();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(description, ex);
            }

            return HealthCheckResult.Healthy(description, new Dictionary<string, object>());
        }
    }
}