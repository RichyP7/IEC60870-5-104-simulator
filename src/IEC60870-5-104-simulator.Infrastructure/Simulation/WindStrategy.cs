using IEC60870_5_104_simulator.Domain;

namespace IEC60870_5_104_simulator.Infrastructure.Simulation
{
    /// <summary>Bounded random-walk wind power: each cycle steps the current value by a random
    /// delta bounded by <see cref="Domain.Iec104DataPoint.FluctuationRate"/>.</summary>
    internal sealed class WindStrategy : ISimulationStrategy
    {
        private readonly Random _random;

        internal WindStrategy(Random random) => _random = random;

        public double ComputeValue(Iec104DataPoint dp, double currentValue)
        {
            double step = (_random.NextDouble() * 2.0 - 1.0) * (dp.FluctuationRate ?? 1.0);
            return currentValue + step;
        }
    }
}
