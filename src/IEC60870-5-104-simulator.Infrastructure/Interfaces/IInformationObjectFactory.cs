using IEC60870_5_104_simulator.Domain;
using lib60870.CS101;

namespace IEC60870_5_104_simulator.Infrastructure.Interfaces
{
    public interface IInformationObjectFactory
    {
        InformationObject GetInformationObject(Iec104DataPoint dataPoint);
        InformationObject GetInformationObjectWithStaticValue(Iec104DataPoint responseDataPoint);
    }
}