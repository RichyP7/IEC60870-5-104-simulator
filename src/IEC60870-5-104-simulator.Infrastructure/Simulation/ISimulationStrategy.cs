using IEC60870_5_104_simulator.Domain;

namespace IEC60870_5_104_simulator.Infrastructure.Simulation
{
    /// <summary>
    /// Computes a raw numeric value for an analog data point for a given simulation mode.
    /// The caller is responsible for clamping and wrapping the result into an InformationObject.
    /// </summary>
    internal interface ISimulationStrategy
    {
        /// <param name="dp">The data point being simulated.</param>
        /// <param name="currentValue">The last persisted numeric value for the point.</param>
        /// <returns>Unclamped raw value for this simulation cycle.</returns>
        double ComputeValue(Iec104DataPoint dp, double currentValue);
    }
}
