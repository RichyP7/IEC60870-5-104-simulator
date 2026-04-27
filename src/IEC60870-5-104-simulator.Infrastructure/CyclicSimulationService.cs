using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using lib60870.CS101;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace IEC60870_5_104_simulator.Infrastructure
{
    internal class CyclicSimulationService : ICyclicSimulationService
    {
        private readonly IASDUDispatcher _dispatcher;
        private readonly IInformationObjectFactory _factory;
        private readonly IIecValueRepository _repository;
        private readonly ILogger<CyclicSimulationService> _logger;

        // Per-datapoint profile cycle index (wraps on each tick)
        private readonly ConcurrentDictionary<IecAddress, int> _profileIndices = new();

        private const int MaxInformationObjectsPerASDU = 20;

        public CyclicSimulationService(
            IASDUDispatcher dispatcher,
            IInformationObjectFactory factory,
            IIecValueRepository repository,
            ILogger<CyclicSimulationService> logger)
        {
            _dispatcher = dispatcher;
            _factory = factory;
            _repository = repository;
            _logger = logger;
        }

        public void SimulateCyclicValues(IEnumerable<Iec104DataPoint> datapoints, int cycleTimeMs)
        {
            var allPoints = datapoints.ToList();

            // Build an O(1) lookup by datapoint ID so counter phases can find linked power points
            // without a full linear scan on every tick.
            var pointsById = allPoints
                .Where(dp => dp.Id != null)
                .ToDictionary(dp => dp.Id!);

            // ── Phase 1: Non-counter cyclic modes (RandomWalk, Periodic, Profile, Gaussian, Solar, Wind)
            // The factory methods persist the computed value to the repository as a side-effect,
            // so that counter modes reading LinkedPowerPointId in Phase 2/3 see fresh values.
            var nonCounterCyclic = GetNonCounterCyclicDataPoints(allPoints);

            // ── Phase 2: EnergyCounter — compute using fresh linked-point values, send periodically.
            var energyCounters = allPoints
                .Where(dp => !dp.Frozen && dp.Mode == SimulationMode.EnergyCounter).ToList();

            // Build the initial ASDU dictionary for all points that will be transmitted this cycle.
            var allPeriodicPoints = nonCounterCyclic.Concat(energyCounters).ToList();
            var asduLists = CreateDistinctAsdus(allPeriodicPoints)
                .ToDictionary(kv => kv.Key, kv => new List<ASDU> { kv.Value });

            foreach (var dp in nonCounterCyclic)
                AddInformationObjectToAsdus(asduLists, dp);

            foreach (var dp in energyCounters)
            {
                IecValueObject newValue = AccumulateCounter(dp, pointsById, cycleTimeMs);
                AppendToAsdus(asduLists, dp, _factory.CreateInformationObjectWithValue(dp, newValue));
            }

            // ── Phase 3: CounterOnDemand — accumulate silently each cycle using fresh power values,
            // but never include in periodic ASDUs (transmitted only on interrogation).
            foreach (var dp in allPoints.Where(dp => !dp.Frozen && dp.Mode == SimulationMode.CounterOnDemand))
                AccumulateCounter(dp, pointsById, cycleTimeMs); // side-effect: persists accumulated value to repo

            _dispatcher.Send(asduLists.SelectMany(kv => kv.Value));
        }

        /// <summary>
        /// Computes the next counter value using the linked power point's current reading,
        /// clamps to configured bounds, persists to the repository, and returns the new value object.
        /// </summary>
        private IecValueObject AccumulateCounter(
            Iec104DataPoint dp,
            Dictionary<string, Iec104DataPoint> pointsById,
            int cycleTimeMs)
        {
            // Read current accumulated value from repo.
            double current = ExtractNumericValue(_repository.GetDataPointValue(dp.Address).Value) ?? dp.BaseValue ?? 0.0;

            // Resolve the linked power point using the pre-built O(1) dictionary.
            double powerKw = 0.0;
            if (!string.IsNullOrEmpty(dp.LinkedPowerPointId))
            {
                if (pointsById.TryGetValue(dp.LinkedPowerPointId, out var powerPoint))
                {
                    double? resolved = ExtractNumericValue(powerPoint.Value);
                    if (!resolved.HasValue)
                        throw new InvalidOperationException(
                            $"Counter '{dp.Id}': linked point '{dp.LinkedPowerPointId}' has a non-numeric value type. Fix the datapoint configuration.");
                    powerKw = resolved.Value;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Counter '{dp.Id}': LinkedPowerPointId '{dp.LinkedPowerPointId}' does not exist in the current datapoints. Fix the datapoint configuration.");
                }
            }

            double cycleHours = cycleTimeMs / 1000.0 / 3600.0;
            double newValue = current + (powerKw / 1000.0) * cycleHours;
            newValue = Math.Clamp(newValue, dp.MinValue ?? double.MinValue, dp.MaxValue ?? double.MaxValue);

            IecValueObject valueObj = WrapCounterValue(dp, newValue);
            _repository.SetObjectValue(dp.Address, valueObj);
            return valueObj;
        }

        /// <summary>Extracts a numeric double from any supported IecValueObject, or null for unsupported types.</summary>
        private static double? ExtractNumericValue(IecValueObject? value) => value switch
        {
            IecValueFloatObject fv => fv.Value,
            IecValueScaledObject sv => sv.Value.Value,
            IecIntValueObject iv => iv.Value,
            _ => null
        };

        /// <summary>Wraps a computed counter value into the correct IecValueObject for the datapoint's type.</summary>
        private static IecValueObject WrapCounterValue(Iec104DataPoint dp, double value) =>
            dp.Iec104DataType.IsScaledMeasurement()
                ? new IecValueScaledObject(new ScaledValueRecord((int)value))
                : new IecValueFloatObject((float)value);

        /// <summary>Appends an already-computed InformationObject to the correct ASDU list, splitting when full.</summary>
        private void AppendToAsdus(
            Dictionary<(int Ca, Iec104DataTypes TypeId), List<ASDU>> asduLists,
            Iec104DataPoint dataPoint,
            InformationObject ioa)
        {
            var list = asduLists[(dataPoint.Address.StationaryAddress, dataPoint.Iec104DataType)];
            var currentAsdu = list.Last();
            if (currentAsdu.NumberOfElements >= MaxInformationObjectsPerASDU)
            {
                var newAsdu = _dispatcher.CreateAsdu(currentAsdu.Ca, CauseOfTransmission.PERIODIC);
                list.Add(newAsdu);
                currentAsdu = newAsdu;
            }
            currentAsdu.AddInformationObject(ioa);
        }

        /// <summary>
        /// Creates an InformationObject for <paramref name="dataPoint"/>, persisting the new value to
        /// the repository, and appends it to the appropriate ASDU in <paramref name="asduLists"/>.
        /// Splits into a new ASDU when the current one is full.
        /// </summary>
        private void AddInformationObjectToAsdus(Dictionary<(int Ca, Iec104DataTypes TypeId), List<ASDU>> asduLists, Iec104DataPoint dataPoint)
        {
            var list = asduLists[(dataPoint.Address.StationaryAddress, dataPoint.Iec104DataType)];
            var currentAsdu = list.Last();

            if (currentAsdu.NumberOfElements >= MaxInformationObjectsPerASDU)
            {
                var newAsdu = _dispatcher.CreateAsdu(currentAsdu.Ca, CauseOfTransmission.PERIODIC);
                list.Add(newAsdu);
                currentAsdu = newAsdu;
            }

            InformationObject ioa = dataPoint.Mode switch
            {
                SimulationMode.Periodic => _factory.CreateInformationObjectWithValue(
                    dataPoint, _repository.GetDataPointValue(dataPoint.Address).Value),
                SimulationMode.Profile => CreateProfileInformationObject(dataPoint),
                _ => _factory.CreateRandomInformationObject(dataPoint)
            };
            currentAsdu.AddInformationObject(ioa);
        }

        /// <summary>
        /// Advances the profile index for this datapoint and returns the next value from its inline
        /// ProfileValues array. Persists the value to the repository and wraps the index.
        /// Falls back to the current repository value when ProfileValues is empty.
        /// </summary>
        private InformationObject CreateProfileInformationObject(Iec104DataPoint dataPoint)
        {
            var values = dataPoint.ProfileValues;
            if (values == null || values.Length == 0)
                return _factory.CreateInformationObjectWithValue(
                    dataPoint, _repository.GetDataPointValue(dataPoint.Address).Value);

            int index = _profileIndices.AddOrUpdate(
                dataPoint.Address, 0,
                (_, current) => (current + 1) % values.Length);
            float profileValue = values[index];
            IecValueObject value = CreateValueObjectFromProfile(dataPoint.Iec104DataType, profileValue);
            _repository.SetObjectValue(dataPoint.Address, value);
            return _factory.CreateInformationObjectWithValue(dataPoint, value);
        }

        private static IEnumerable<Iec104DataPoint> GetNonCounterCyclicDataPoints(IEnumerable<Iec104DataPoint> datapoints)
        {
            // EnergyCounter and CounterOnDemand are intentionally excluded here — they must be
            // computed in a separate phase after all linked power-point values are fresh in the repo.
            return datapoints.Where(dp =>
                !dp.Frozen &&
                dp.Mode is SimulationMode.RandomWalk
                         or SimulationMode.Periodic
                         or SimulationMode.Profile
                         or SimulationMode.GaussianNoise
                         or SimulationMode.Solar
                         or SimulationMode.Wind);
        }

        private IEnumerable<KeyValuePair<(int Ca, Iec104DataTypes TypeId), ASDU>> CreateDistinctAsdus(IEnumerable<Iec104DataPoint> datapoints)
        {
            List<KeyValuePair<(int Ca, Iec104DataTypes TypeId), ASDU>> asdusPerTypeandCa = new();
            foreach (var groupByStationAndType in
                    datapoints
                    .GroupBy(x => new { x.Address?.StationaryAddress, x.Iec104DataType })
                    .Select(g => new { station = g.First().Address.StationaryAddress, iectype = g.First().Iec104DataType }))
            {
                ASDU newAsdu = _dispatcher.CreateAsdu(groupByStationAndType.station, CauseOfTransmission.PERIODIC);
                asdusPerTypeandCa.Add(new KeyValuePair<(int, Iec104DataTypes), ASDU>((groupByStationAndType.station, groupByStationAndType.iectype), newAsdu));
            }
            return asdusPerTypeandCa;
        }

        private static IecValueObject CreateValueObjectFromProfile(Iec104DataTypes dataType, float profileValue)
        {
            if (dataType.IsFloatValue())        return new IecValueFloatObject(profileValue);
            if (dataType.IsScaledMeasurement()) return new IecValueScaledObject(new ScaledValueRecord((int)profileValue));
            if (dataType.IsStepPosition())      return new IecIntValueObject((int)profileValue);
            throw new NotSupportedException($"PredefinedProfile mode is not supported for data type {dataType}");
        }
    }
}
