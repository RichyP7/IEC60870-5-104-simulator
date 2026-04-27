using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.Service;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Globalization;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class ScenarioService : IScenarioService
    {
        private readonly IReadOnlyList<ScenarioDefinition> _definitions;
        private readonly IIecValueRepository _repository;
        private readonly IIec104Service _iec104Service;
        private readonly IScenarioEventPublisher _eventPublisher;
        private readonly ILogger<ScenarioService> _logger;

        // Per-scenario concurrency guard: SemaphoreSlim(1,1) prevents concurrent runs of the same scenario
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.OrdinalIgnoreCase);
        private readonly ConcurrentDictionary<string, ScenarioState> _states = new(StringComparer.OrdinalIgnoreCase);

        public ScenarioService(
            IReadOnlyList<ScenarioDefinition> definitions,
            IIecValueRepository repository,
            IIec104Service iec104Service,
            IScenarioEventPublisher eventPublisher,
            ILogger<ScenarioService> logger)
        {
            _definitions = definitions;
            _repository = repository;
            _iec104Service = iec104Service;
            _eventPublisher = eventPublisher;
            _logger = logger;

            foreach (var def in definitions)
            {
                _locks[def.Name] = new SemaphoreSlim(1, 1);
                _states[def.Name] = new ScenarioState(def.Name, ScenarioStatus.Idle, 0);
            }
        }

        public IEnumerable<ScenarioState> GetStates() => _states.Values;

        public IEnumerable<ScenarioDefinition> GetDefinitions() => _definitions;

        public async Task<bool> TriggerAsync(string name, CancellationToken ct = default)
        {
            var def = _definitions.FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (def == null)
            {
                _logger.LogWarning("Scenario '{Name}' not found", name);
                return false;
            }

            var sem = _locks.GetOrAdd(def.Name, _ => new SemaphoreSlim(1, 1));
            if (!await sem.WaitAsync(0, ct))
            {
                _logger.LogWarning("Scenario '{Name}' is already running — concurrent trigger rejected", def.Name);
                return false;
            }

            // Non-blocking fire-and-forget; caller gets immediate response
            _ = Task.Run(async () =>
            {
                try
                {
                    await RunScenarioAsync(def, ct);
                }
                finally
                {
                    sem.Release();
                }
            }, CancellationToken.None);

            return true;
        }

        private async Task RunScenarioAsync(ScenarioDefinition def, CancellationToken ct)
        {
            _logger.LogInformation("Scenario '{Name}' started ({Steps} fault steps, {RecoveryMs}ms recovery)",
                def.Name, def.Steps.Count, def.RecoveryMs);

            int totalMs = def.Steps.Sum(s => s.DelayMs) + def.RecoveryMs;
            var startedAt = DateTime.UtcNow;

            UpdateAndPublish(def.Name, ScenarioStatus.Running, totalMs);

            try
            {
                foreach (var step in def.Steps)
                {
                    if (ct.IsCancellationRequested) break;
                    await Task.Delay(step.DelayMs, ct);

                    int remaining = Math.Max(0, totalMs - (int)(DateTime.UtcNow - startedAt).TotalMilliseconds);
                    _logger.LogInformation("[{Name}] {Description}", def.Name, step.Description);
                    await ApplyStep(step);
                    UpdateAndPublish(def.Name, ScenarioStatus.Running, remaining);
                }

                // Wait before recovery
                await Task.Delay(def.RecoveryMs, ct);

                _logger.LogInformation("Scenario '{Name}': executing auto-recovery ({Steps} steps)", def.Name, def.RecoverySteps.Count);
                foreach (var step in def.RecoverySteps)
                    await ApplyStep(step);

                UpdateAndPublish(def.Name, ScenarioStatus.Completed, 0);
                _logger.LogInformation("Scenario '{Name}' completed", def.Name);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Scenario '{Name}' was cancelled — attempting best-effort recovery", def.Name);
                await BestEffortRecovery(def);
                UpdateAndPublish(def.Name, ScenarioStatus.Failed, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scenario '{Name}' failed — attempting best-effort recovery", def.Name);
                await BestEffortRecovery(def);
                UpdateAndPublish(def.Name, ScenarioStatus.Failed, 0);
            }
        }

        private async Task ApplyStep(ScenarioStep step)
        {
            var address = new IecAddress(step.Ca, step.Oa);
            try
            {
                var dataPoint = _repository.GetDataPointValue(address);
                var value = ParseValue(step.ValueStr, dataPoint);
                _repository.SetObjectValue(address, value);

                if (step.Freeze)
                    _repository.FreezeDataPoint(address);
                else
                    _repository.UnfreezeDataPoint(address);

                // Re-fetch so the dataPoint object has the updated value before sending
                var updated = _repository.GetDataPointValue(address);
                _ = _iec104Service.Simulate(updated);

                // Push real-time update to UI clients immediately (don't wait for next cyclic snapshot)
                await _eventPublisher.PublishDataPointChanged(updated);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Scenario step skipped: Ca={Ca} Oa={Oa} not found", step.Ca, step.Oa);
            }
        }

        private async Task BestEffortRecovery(ScenarioDefinition def)
        {
            foreach (var step in def.RecoverySteps)
            {
                try { await ApplyStep(step); }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Best-effort recovery step failed: Ca={Ca} Oa={Oa}", step.Ca, step.Oa);
                }
            }
        }

        private void UpdateAndPublish(string name, ScenarioStatus status, int remainingMs)
        {
            _states[name] = new ScenarioState(name, status, remainingMs);
            _ = _eventPublisher.PublishScenarioUpdate(_states[name]);
        }

        // -----------------------------------------------------------------------
        // Value parsing — same rules as IecValueLocalStorageRepository.AddDataPoint
        // -----------------------------------------------------------------------
        private static IecValueObject ParseValue(string valueStr, Iec104DataPoint dp)
        {
            var dt = dp.Iec104DataType;
            if (dt.IsSinglePoint())        return new IecSinglePointValueObject(bool.Parse(valueStr));
            if (dt.IsDoublePoint())        return new IecDoublePointValueObject(Enum.Parse<IecDoublePointValue>(valueStr, ignoreCase: true));
            if (dt.IsStepPosition())       return new IecIntValueObject(int.Parse(valueStr));
            if (dt.IsScaledMeasurement())  return new IecValueScaledObject(new ScaledValueRecord(int.Parse(valueStr)));
            if (dt.IsFloatValue())         return new IecValueFloatObject(float.Parse(valueStr, CultureInfo.InvariantCulture));
            throw new NotSupportedException($"Type {dp.Iec104DataType} not supported in scenario steps");
        }
    }
}
