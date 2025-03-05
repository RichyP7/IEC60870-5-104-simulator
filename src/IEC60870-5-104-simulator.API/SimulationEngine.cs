using AutoMapper;
using IEC60870_5_104_simulator.API;
using IEC60870_5_104_simulator.API.HealthChecks;
using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.Service;
using IEC60870_5_104_simulator.Infrastructure.Exceptions;
using Microsoft.Extensions.Options;

namespace IEC60870_5_104_simulator.Service
{
    public class SimulationEngine : BackgroundService
    {
        private readonly ILogger<SimulationEngine> _logger;
        private readonly Iec104SimulationOptions options;
        private readonly IIec104ConfigurationService datapointConfigService;
        private readonly IMapper mapper;
        private readonly ServerStartedHealthCheck healthCheck;
        private bool dataConfigurationDone = false;
        public SimulationState SimulationStatus { get; set; } = SimulationState.Stopped;

        private IIec104Service iecService { get; }
        private int cycleTimeMs;

        public SimulationEngine(ILogger<SimulationEngine> logger, IIec104Service iecservice, IOptions<Iec104SimulationOptions> options, IIec104ConfigurationService datapointconfig, IMapper mapper,
            ServerStartedHealthCheck healthCheck)
        {
            _logger = logger;
            this.iecService = iecservice;
            this.options = options.Value;
            this.cycleTimeMs = this.GetCycleTime();
            this.datapointConfigService = datapointconfig;
            this.mapper = mapper;
            this.healthCheck = healthCheck;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!dataConfigurationDone)
            {
                Configure();
            }
            await iecService.Start();
            healthCheck.ServerIsRunning = true;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogDebug("Worker running at: {time}", DateTimeOffset.Now);
                    await Task.Delay(cycleTimeMs);
                    await this.iecService.Simulate(datapointConfigService.DataPoints.Values);
                }
                catch (TaskCanceledException ex)
                {
                    _logger.LogWarning("Task was cancelled");
                    throw ex;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Simulation failed", ex.Message);
                }
            }
        }

        private int GetCycleTime()
        {
            return options.CycleTimeMs > 10 ? options.CycleTimeMs : 10;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            if (SimulationStatus == SimulationState.Running) throw new BadRequestException("Simulation is already running.");
            _logger.LogInformation("Started worker at: {time}", DateTimeOffset.Now);
            await base.StartAsync(cancellationToken);
            SimulationStatus = SimulationState.Running;
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (SimulationStatus == SimulationState.Stopped) throw new BadRequestException("Simulation is already stopped.");
            _logger.LogInformation("Stopped worker at: {time}", DateTimeOffset.Now);
            await base.StopAsync(cancellationToken);
            await this.iecService.Stop();
            SimulationStatus = SimulationState.Stopped;
        }
        private void Configure()
        {
            var commands = options.DataPointConfiguration.Commands;
            var measures = options.DataPointConfiguration.Measures;
            var commandDataPoints = this.mapper.Map<List<Iec104CommandDataPointConfig>>(commands);
            var resultMeasures = this.mapper.Map<List<Iec104DataPoint>>(measures);
            if (commands != null)
                AssignResponses(commands, commandDataPoints, resultMeasures);
            this.datapointConfigService.ConfigureDataPoints(commandDataPoints, resultMeasures);

            _logger.LogInformation("{NumberCommands} commands and {NumberMeasures} measurements got configured", commands?.Count, measures?.Count);
            resultMeasures.ForEach(v => _logger.LogInformation("Id:{Id} Ca {Ca} Oa {Oa} Type {type}", v.Id, v.Address.StationaryAddress, v.Address.ObjectAddress, v.Iec104DataType));
            commandDataPoints.ForEach(v => _logger.LogInformation("Id:{Id} Ca {Ca} Oa {Oa} Type:{type}, Resp: {response}", v.Id, v.Address.StationaryAddress, v.Address.ObjectAddress, v.Iec104DataType, v.SimulatedDataPoint?.Id));
            dataConfigurationDone = true;
        }

        private static void AssignResponses(List<Iec104SimulationOptions.CommandPointConfig> commands, List<Iec104CommandDataPointConfig> commandDataPoints, List<Iec104DataPoint> resultMeasures)
        {
            foreach (var item in commands)
            {
                if (!String.IsNullOrEmpty(item.ResponseId))
                {
                    var responseDataPoint = resultMeasures.SingleOrDefault(v => v.Id.Equals(item.ResponseId));
                    if (responseDataPoint == null)
                    {
                        throw new InvalidOperationException($"Invalid config for command: {item.Id}, ResponseId{item.ResponseId}");
                    }
                    var commandDataPoint = commandDataPoints.Single(v => v.Id.Equals(item.Id));
                    commandDataPoint.AssignResponseDataPoint(responseDataPoint);
                }
            }
        }

        public enum SimulationState
        {
            Stopped,
            Running
        }
    }
}