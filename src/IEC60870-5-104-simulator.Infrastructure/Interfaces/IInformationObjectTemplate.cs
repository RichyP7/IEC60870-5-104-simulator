using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using lib60870.CS101;

namespace IEC60870_5_104_simulator.Infrastructure.Interfaces
{
    public interface IInformationObjectTemplate
    {
        InformationObject GetDoublePoint(int objectAddress, IecDoublePointValueObject iecDoublePointValueObject, Iec104DataTypes iec104DataType);
        InformationObject GetMeasuredValueScaled(int objectAddress, IecIntValueObject iecIntValueObject, Iec104DataTypes iec104DataType);
        InformationObject GetMeasuredValueShort(int objectAddress, IecValueShortObject iecValueShortObject, Iec104DataTypes iec104DataType);
        InformationObject GetSinglePoint(int objectAddress, IecValueObject iecIntValueObject, Iec104DataTypes iec104DataType);
        InformationObject GetStepposition(int objectAddress, IecValueObject value, Iec104DataTypes type);
    }
}
