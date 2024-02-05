using IEC60870_5_104_simulator.Domain.Service;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace IEC60870_5_104_simulator.API.HealthChecks
{
    public class ServerStartedHealthCheck : IHealthCheck
    {
        private volatile bool _isReady;

        public bool ServerIsRunning
        {
            get => _isReady;
            set => _isReady = value;
        }


        public Task<HealthCheckResult> CheckHealthAsync(
                HealthCheckContext context, CancellationToken cancellationToken = default)
        {

            if (ServerIsRunning)
            {
                return Task.FromResult(
                    HealthCheckResult.Healthy("Startup completed."));
            }
            return Task.FromResult(
                new HealthCheckResult(
                    context.Registration.FailureStatus, "Startup not finished."));
        }
    }
}
