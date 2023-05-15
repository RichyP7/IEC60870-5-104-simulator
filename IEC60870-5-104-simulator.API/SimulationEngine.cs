namespace IEC60870_5_104_simulator.Service
{
    public class SimulationEngine : BackgroundService
    {
        private readonly ILogger<SimulationEngine> _logger;

        public SimulationEngine(ILogger<SimulationEngine> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(1000, stoppingToken);
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning("Task was cancelled");
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
            await Task.Delay(1000, cancellationToken);
            await base.StopAsync(cancellationToken);
            return;
        }
    }
}