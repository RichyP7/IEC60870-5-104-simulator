using IEC60870_5_104_simulator.Domain;

namespace IEC60870_5_104_simulator.Infrastructure.Interfaces
{
    public interface ICyclicSimulationService
    {
        void SimulateCyclicValues(IEnumerable<Iec104DataPoint> datapoints, int cycleTimeMs);
    }
}
