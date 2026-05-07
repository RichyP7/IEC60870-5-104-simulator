using IEC60870_5_104_simulator.Domain;

namespace IEC60870_5_104_simulator.Infrastructure.Simulation
{
    /// <summary>Positive half-sine wave with a configurable period.
    /// Rises from zero, peaks at half-period, returns to zero, then stays zero for the second half.
    /// Gaussian noise is added proportional to <see cref="Domain.Iec104DataPoint.FluctuationRate"/>.
    /// Period is set via <see cref="Domain.Iec104DataPoint.WavePeriodSeconds"/> (default 86 400 s = 24 h).</summary>
    internal sealed class PeriodicWaveStrategy : ISimulationStrategy
    {
        private readonly Random _random;

        internal PeriodicWaveStrategy(Random random) => _random = random;

        public double ComputeValue(Iec104DataPoint dp, double currentValue)
        {
            double periodMs = (dp.WavePeriodSeconds ?? 86_400.0) * 1000.0;
            double elapsedMs = DateTime.Now.TimeOfDay.TotalMilliseconds % periodMs;
            double factor = Math.Max(0.0, Math.Sin(Math.PI * elapsedMs / periodMs));
            double smoothValue = (dp.BaseValue ?? 0.0) * factor;
            double perCycleDelta = Math.Abs(smoothValue - currentValue);
            double noiseStddev = (dp.FluctuationRate ?? 0.0) * perCycleDelta;
            double noise = GaussianSampler.Sample(_random, 0.0, noiseStddev);
            return smoothValue + noise;
        }
    }
}
