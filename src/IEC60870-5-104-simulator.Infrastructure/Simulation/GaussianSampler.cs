namespace IEC60870_5_104_simulator.Infrastructure.Simulation
{
    /// <summary>Box-Muller transform for sampling from a normal distribution.</summary>
    internal static class GaussianSampler
    {
        internal static double Sample(Random random, double mean, double stddev)
        {
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();
            double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + stddev * z;
        }
    }
}
