using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain.Interfaces
{
    /// <summary>
    /// Owns which measures exist and what their values are. The only writer of a datapoint.
    /// </summary>
    /// <remarks>
    /// <see cref="Get"/> and <see cref="Snapshot"/> hand out the live stored objects so the tick
    /// does not allocate per datapoint per cycle. Callers may read them freely and must never
    /// mutate them: every write goes through a method on this interface. See ADR 0001.
    /// </remarks>
    public interface IDatapointRegistry
    {
        /// <summary>Adds a measure. Returns false when the address is already taken.</summary>
        bool Add(Iec104DataPoint datapoint);

        /// <summary>Removes a measure. Returns false when the address is not present.</summary>
        bool Remove(IecAddress address);

        bool Contains(IecAddress address);

        /// <exception cref="KeyNotFoundException">No measure at <paramref name="address"/>.</exception>
        Iec104DataPoint Get(IecAddress address);

        /// <exception cref="KeyNotFoundException">No measure at <paramref name="address"/>.</exception>
        void SetValue(IecAddress address, IecValueObject value);

        /// <exception cref="KeyNotFoundException">No measure at <paramref name="address"/>.</exception>
        void SetMode(IecAddress address, SimulationMode mode);

        /// <summary>
        /// Shifts a step-position value by <paramref name="delta"/>, clamped to the IEC-104 step
        /// range. Returns the value actually stored.
        /// </summary>
        /// <exception cref="KeyNotFoundException">No measure at <paramref name="address"/>.</exception>
        /// <exception cref="InvalidCastException">The stored value is not a step position.</exception>
        int AdjustStep(IecAddress address, int delta);

        /// <exception cref="KeyNotFoundException">No measure at <paramref name="address"/>.</exception>
        void SetFrozen(IecAddress address, bool frozen);

        /// <summary>A materialised list, so a concurrent add cannot tear an iteration.</summary>
        IReadOnlyList<Iec104DataPoint> Snapshot();
    }
}
