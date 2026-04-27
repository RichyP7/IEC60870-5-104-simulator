using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain
{
    public class Iec104DataPoint
    {
        public string Id { get; set; }
        public IecAddress Address { get; set; }
        public Iec104DataTypes Iec104DataType { get; set; }
        public IecValueObject Value { get; set; }
        public SimulationMode Mode { get; set; }
        public string InitString { get; set; }
        public float[]? ProfileValues { get; set; }
        public int ProfileIndex { get; set; }

        // Simulation realism metadata
        public double? BaseValue { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public double? FluctuationRate { get; set; }
        public string? LinkedPowerPointId { get; set; }

        // Frozen flag: when true, cyclic simulation skips this point (e.g. during fault scenario)
        public bool Frozen { get; set; }

        public Iec104DataPoint(IecAddress address, Iec104DataTypes iec104DataType)
        {
            Address = address;
            this.Iec104DataType = iec104DataType;
        }

        public Iec104DataPoint()
        {
        }
    }
    public enum SimulationMode
    {
        /// <summary>Value is fixed. Sent spontaneously (e.g. on startup) and on interrogation only — never on the periodic cycle.</summary>
        Static,
        /// <summary>Fixed value repeated on every cycle with COT=PERIODIC.</summary>
        Periodic,
        /// <summary>Value changes via a random walk on every cycle.</summary>
        RandomWalk,
        /// <summary>Gaussian noise around BaseValue, bounded by MinValue/MaxValue. Sent every cycle.</summary>
        GaussianNoise,
        /// <summary>Sinusoidal daytime solar generation profile. Sent every cycle.</summary>
        Solar,
        /// <summary>Bounded random-walk wind power simulation. Sent every cycle.</summary>
        Wind,
        /// <summary>Accumulating energy counter transmitted on every cycle (COT=PERIODIC).</summary>
        EnergyCounter,
        /// <summary>Counter that accumulates silently each cycle but is only transmitted on interrogation (GI/CI).</summary>
        CounterOnDemand,
        /// <summary>Value follows a predefined float array profile. Sent every cycle.</summary>
        Profile,
        /// <summary>Value mirrors an incoming command ASDU. Sent as command acknowledgement.</summary>
        CommandResponse,
    }
}
