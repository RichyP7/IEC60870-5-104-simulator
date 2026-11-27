using System.Collections.Concurrent;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain.Service
{
    /// <summary>
    /// The single owner of datapoints. Implements both registry interfaces so that the
    /// command→response link resolves against the same address space as the measures.
    /// </summary>
    /// <remarks>
    /// Reads hand out live objects (the tick reads once per datapoint per cycle); writes are
    /// only ever performed by this class. See ADR 0001.
    /// </remarks>
    public class DatapointRegistry : IDatapointRegistry
    {
        private readonly ConcurrentDictionary<IecAddress, Iec104DataPoint> measures = new();

        public bool Add(Iec104DataPoint datapoint)
        {
            ArgumentNullException.ThrowIfNull(datapoint);
            if (!measures.TryAdd(datapoint.Address, datapoint))
                return false;

            ApplyInitValue(datapoint);
            return true;
        }

        public bool Remove(IecAddress address)
        {
            return measures.TryRemove(address, out _);
        }

        public bool Contains(IecAddress address)
        {
            return measures.ContainsKey(address);
        }

        public Iec104DataPoint Get(IecAddress address)
        {
            if (measures.TryGetValue(address, out var datapoint))
                return datapoint;

            throw new KeyNotFoundException(
                $"No datapoint for Ca: {address.StationaryAddress} Oa: {address.ObjectAddress}");
        }

        public void SetValue(IecAddress address, IecValueObject value)
        {
            ArgumentNullException.ThrowIfNull(value);
            Get(address).Value = value;
        }

        public void SetMode(IecAddress address, SimulationMode mode)
        {
            Get(address).Mode = mode;
        }

        public int AdjustStep(IecAddress address, int delta)
        {
            var datapoint = Get(address);
            if (datapoint.Value is not IecIntValueObject current)
                throw new InvalidCastException(
                    $"Datapoint Ca: {address.StationaryAddress} Oa: {address.ObjectAddress} holds " +
                    $"{datapoint.Value?.GetType().Name ?? "no value"}, not a step position.");

            int adjusted = Math.Clamp(
                current.Value + delta,
                StepPositionConst.STEP_POSITION_MIN,
                StepPositionConst.STEP_POSITION_MAX);

            datapoint.Value = new IecIntValueObject(adjusted);
            return adjusted;
        }

        public void SetFrozen(IecAddress address, bool frozen)
        {
            Get(address).Frozen = frozen;
        }

        public IReadOnlyList<Iec104DataPoint> Snapshot()
        {
            return measures.Values.ToList();
        }

        /// <summary>
        /// Seeds a newly added datapoint's value from its configured init string when it has none.
        /// Semantics are carried over unchanged from the previous repository (ADR 0001, commit 2).
        /// </summary>
        private static void ApplyInitValue(Iec104DataPoint datapoint)
        {
            if (datapoint.Value is not null)
                return;

            datapoint.Value = InitValueParser.Parse(datapoint.InitString, datapoint.Iec104DataType);
        }
    }
}
