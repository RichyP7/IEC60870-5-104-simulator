using AutoMapper;
using IEC60870_5_104_simulator.API;
using IEC60870_5_104_simulator.API.HealthChecks;
using IEC60870_5_104_simulator.API.Hubs;
using IEC60870_5_104_simulator.API.Mapping;
using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.Service;
using static IEC60870_5_104_simulator.API.Iec104SimulationOptions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;

namespace IEC60870_5_104_simulator.Service
{
    public class SimulationEngine : BackgroundService
    {
        private readonly ILogger<SimulationEngine> logger;
        private readonly Iec104SimulationOptions options;
        private readonly IIec104ConfigurationService datapointConfigService;
        private readonly IMapper mapper;
        private readonly ServerStartedHealthCheck healthCheck;
        private readonly IHubContext<SimulationHub> hubContext;
        private bool dataConfigurationDone = false;
        public SimulationState SimulationStatus { get; set; } = SimulationState.Stopped;
        public DateTime StartedAt { get; private set; } = DateTime.UtcNow;

        private int _lastClientCount = -1;

        private IIec104Service iecService { get; }
        private readonly int cycleTimeMs;

        public SimulationEngine(ILogger<SimulationEngine> logger, IIec104Service iecservice, IOptions<Iec104SimulationOptions> options,
            IIec104ConfigurationService datapointconfig, IMapper mapper, ServerStartedHealthCheck healthCheck,
            IHubContext<SimulationHub> hubContext)
        {
            this.logger = logger;
            this.iecService = iecservice;
            this.options = options.Value;
            this.cycleTimeMs = this.GetCycleTime();
            this.datapointConfigService = datapointconfig;
            this.mapper = mapper;
            this.healthCheck = healthCheck;
            this.hubContext = hubContext;
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
                    logger.LogTrace("Worker running at: {Time}", DateTimeOffset.Now);
                    await Task.Delay(cycleTimeMs);
                    await iecService.SimulateCyclic(datapointConfigService.DataPoints.Values, cycleTimeMs);

                    // Push real-time snapshot to all connected UI clients
                    await PushFullSnapshotAsync();

                    // Push client count update only when it changes
                    int currentCount = iecService.GetActiveClientCount();
                    if (currentCount != _lastClientCount)
                    {
                        _lastClientCount = currentCount;
                        await hubContext.Clients.All.SendAsync("ClientCountUpdate", currentCount, stoppingToken);
                    }
                }
                catch (TaskCanceledException ex)
                {
                    logger.LogWarning(ex, "Task was cancelled");
                    throw;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Simulation failed {Message}", ex.Message);
                    throw;
                }
            }
        }

        private Task PushFullSnapshotAsync()
        {
            var snapshot = datapointConfigService.DataPoints.Values
                .Select(dp => new DataPointUpdateDto(
                    dp.Address.StationaryAddress,
                    dp.Address.ObjectAddress,
                    dp.Id ?? string.Empty,
                    dp.Iec104DataType.ToString(),
                    dp.Mode.ToString(),
                    dp.Value != null ? mapper.Map<IecValueDto>(dp.Value) : null,
                    dp.Frozen))
                .ToList();

            logger.LogInformation("Pushing full snapshot with {Count} datapoints to {ClientCount} clients", snapshot.Count, iecService.GetActiveClientCount());

            return hubContext.Clients.All.SendAsync("FullSnapshot", snapshot);
        }

        private int GetCycleTime()
        {
            return options.CycleTimeMs > 10 ? options.CycleTimeMs : 10;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            if (SimulationStatus == SimulationState.Running) throw new InvalidOperationException("Simulation is already running.");
            logger.LogInformation("Started worker at: {Time}", DateTimeOffset.Now);
            StartedAt = DateTime.UtcNow;
            await base.StartAsync(cancellationToken);
            SimulationStatus = SimulationState.Running;
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (SimulationStatus == SimulationState.Stopped) throw new InvalidOperationException("Simulation is already stopped.");
            logger.LogInformation("Stopped worker at: {Time}", DateTimeOffset.Now);
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
            ValidateProfiles(resultMeasures);
            ValidateLinkedPowerPoints(resultMeasures);
            this.datapointConfigService.ConfigureDataPoints(commandDataPoints, resultMeasures);

            logger.LogInformation("{NumberCommands} commands and {NumberMeasures} measurements got configured", commands?.Count, measures?.Count);
            resultMeasures.ForEach(v => logger.LogInformation("Id:{Id} Ca {Ca} Oa {Oa} Type {Type}", v.Id, v.Address.StationaryAddress, v.Address.ObjectAddress, v.Iec104DataType));
            commandDataPoints.ForEach(v => logger.LogInformation("Id: {Id} Ca {Ca} Oa {Oa} Type:{Type}, Resp: {Response}", v.Id, v.Address.StationaryAddress, v.Address.ObjectAddress, v.Iec104DataType, v.SimulatedDataPoint?.Id));
            dataConfigurationDone = true;
        }

        private void ValidateProfiles(List<Iec104DataPoint> measures)
        {
            foreach (var measure in measures.Where(m => m.Mode == SimulationMode.Profile))
            {
                if (measure.ProfileValues == null || measure.ProfileValues.Length == 0)
                    throw new InvalidOperationException(
                        $"Datapoint '{measure.Id}' has mode Profile but ProfileValues is empty. Provide at least one value.");
            }
        }

        private static void ValidateLinkedPowerPoints(List<Iec104DataPoint> measures)
        {
            var allIds = measures.Select(m => m.Id).ToHashSet();
            foreach (var measure in measures.Where(m =>
                !string.IsNullOrEmpty(m.LinkedDataPointId) &&
                (m.Mode == SimulationMode.EnergyCounter || m.Mode == SimulationMode.CounterOnDemand)))
            {
                if (!allIds.Contains(measure.LinkedDataPointId!))
                    throw new InvalidOperationException(
                        $"Datapoint '{measure.Id}' references unknown LinkedDataPointId '{measure.LinkedDataPointId}'. Check the Id matches an existing measure.");
            }
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
                        throw new InvalidOperationException($"Invalid config for command CA{item.Ca}_IOA{item.Oa}, ResponseId: {item.ResponseId}");
                    }
                    var commandDataPoint = commandDataPoints.Single(v =>
                        v.Address.StationaryAddress == item.Ca && v.Address.ObjectAddress == item.Oa);
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