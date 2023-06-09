using IEC60870_5_104_simulator.API;
using IEC60870_5_104_simulator.Infrastructure;
using Microsoft.Extensions.Options;

namespace IEC60870_5_104_simulator.Service
{
    public class SimulationEngine : BackgroundService
    {
        private readonly ILogger<SimulationEngine> _logger;
        private readonly SimulationOptions options;

        private IIec104Service iecService { get; }
        private int cycleTimeMs;

        public SimulationEngine(ILogger<SimulationEngine> logger, IIec104Service iecservice, IOptions<SimulationOptions> options)
        {
            _logger = logger;
            
            this.iecService = iecservice;
            this.options = options.Value;
            this.cycleTimeMs = this.GetCycleTime();
            
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await this.iecService.Start();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(cycleTimeMs);
                    await this.iecService.SimulateValues();
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning("Task was cancelled");
                    throw ex;
                }

            }
        }

        private int GetCycleTime()
        {
            return options.CycleTimeMs > 10 ? options.CycleTimeMs : 10;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
             _logger.LogInformation("Started worker at: {time}", DateTimeOffset.Now);
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