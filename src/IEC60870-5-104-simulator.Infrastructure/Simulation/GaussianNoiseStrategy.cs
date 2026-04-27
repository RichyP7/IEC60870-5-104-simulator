using IEC60870_5_104_simulator.Domain;

namespace IEC60870_5_104_simulator.Infrastructure.Simulation
{
    /// <summary>Gaussian noise around <see cref="Domain.Iec104DataPoint.BaseValue"/> with standard deviation
    /// controlled by <see cref="Domain.Iec104DataPoint.FluctuationRate"/>.</summary>
    internal sealed class GaussianNoiseStrategy : ISimulationStrategy
    {
        private readonly Random _random;

        internal GaussianNoiseStrategy(Random random) => _random = random;

        public double ComputeValue(Iec104DataPoint dp, double currentValue)
        {
            double mean = dp.BaseValue ?? 0.0;
            double stddev = dp.FluctuationRate ?? 1.0;
            return GaussianSampler.Sample(_random, mean, stddev);
        }
    }
}
