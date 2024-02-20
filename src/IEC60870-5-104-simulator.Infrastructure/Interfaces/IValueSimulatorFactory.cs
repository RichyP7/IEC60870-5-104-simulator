using IEC60870_5_104_simulator.Domain;
using lib60870.CS101;

namespace IEC60870_5_104_simulator.Infrastructure.Interfaces
{
    public interface IValueSimulatorFactory
    {
        InformationObject SimulateValues(Iec104DataPoint datapoints);
    }
}