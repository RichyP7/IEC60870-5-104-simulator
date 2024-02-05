using IEC60870_5_104_simulator.Domain.Service;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IEC60870_5_104_simulator.API.HealthChecks
{
    public class ConnectionEstablishedHealthCheck : IHealthCheck
    {
        private readonly IIec104Service service;

        public ConnectionEstablishedHealthCheck(IIec104Service service)
        {
            this.service = service;
        }

        public Task<HealthCheckResult> CheckHealthAsync(
                HealthCheckContext context, CancellationToken cancellationToken = default)
        {

            if (this.service.ConnectionEstablished())
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("A healthy result."));
            }

            return Task.FromResult(HealthCheckResult.Degraded("No Connection alive"));
        }
    }
}
