using IEC60870_5_104_simulator.Domain;

namespace IEC60870_5_104_simulator.Infrastructure.Simulation
{
    /// <summary>Random walk for analog (M_ME_* and M_ST_*) data points: steps the current value
    /// by a random delta bounded by <see cref="Domain.Iec104DataPoint.FluctuationRate"/>.
    /// Discrete types (M_SP_*, M_DP_*) are handled separately in <c>ObjectFactory</c>.</summary>
    internal sealed class RandomWalkStrategy : ISimulationStrategy
    {
        private readonly Random _random;

        internal RandomWalkStrategy(Random random) => _random = random;

        public double ComputeValue(Iec104DataPoint dp, double currentValue)
        {
            double step = (_random.NextDouble() * 2.0 - 1.0) * (dp.FluctuationRate ?? 1.0);
            return currentValue + step;
        }
    }
}
