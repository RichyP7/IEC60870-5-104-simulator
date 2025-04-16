using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using lib60870.CS101;

namespace IEC60870_5_104_simulator.Infrastructure.Interfaces
{
    public interface IInformationObjectFactory
    {
        InformationObject CreateRandomInformationObject(Iec104DataPoint dataPoint);
        InformationObject CreateInformationObjectWithValue(Iec104DataPoint responseDataPoint, IecValueObject value);
    }
}