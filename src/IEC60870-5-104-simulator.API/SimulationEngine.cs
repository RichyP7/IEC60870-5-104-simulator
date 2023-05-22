using IEC60870_5_104_simulator.Infrastructure;

namespace IEC60870_5_104_simulator.Service
{
    public class SimulationEngine : BackgroundService
    {
        private readonly ILogger<SimulationEngine> _logger;

        private IIec104Service iecService { get; }

        public SimulationEngine(ILogger<SimulationEngine> logger, IIec104Service iecservice)
        {
            _logger = logger;
            this.iecService = iecservice;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this.iecService.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(2000);
                    await this.iecService.SimulateValues();
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning("Task was cancelled");
                    throw ex;
                }

            }
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
             _logger.LogInformation("Started worker at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000, cancellationToken);
            await base.StartAsync(cancellationToken);
            return;
            
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopped worker at: {time}", DateTimeOffset.Now);
            await base.StopAsync(cancellationToken);
            await this.iecService.Stop();
            return;
        }
    }
}