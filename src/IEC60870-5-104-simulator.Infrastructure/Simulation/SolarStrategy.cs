using IEC60870_5_104_simulator.Domain;

namespace IEC60870_5_104_simulator.Infrastructure.Simulation
{
    /// <summary>Sinusoidal daytime solar generation profile.
    /// Peak output at solar noon (12:00), zero before 06:00 and after 18:00.
    /// Gaussian noise is added proportional to <see cref="Domain.Iec104DataPoint.FluctuationRate"/>.</summary>
    internal sealed class SolarStrategy : ISimulationStrategy
    {
        private readonly Random _random;

        internal SolarStrategy(Random random) => _random = random;

        public double ComputeValue(Iec104DataPoint dp, double currentValue)
        {
            double hour = DateTime.Now.Hour + DateTime.Now.Minute / 60.0;
            double solarFactor = Math.Max(0.0, Math.Sin(Math.PI * (hour - 6.0) / 12.0));
            double noiseStddev = (dp.FluctuationRate ?? 0.05) * Math.Abs(dp.BaseValue ?? 1.0);
            double noise = GaussianSampler.Sample(_random, 0.0, noiseStddev);
            return (dp.BaseValue ?? 0.0) * solarFactor + noise;
        }
    }
}
