using IEC60870_5_104_simulator.Domain;
using lib60870.CS101;
using System.Diagnostics;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class InformationObjectFactory : IInformationObjectFactory
    {

        public InformationObject GetInformationObject(Iec104DataPointConfig dataPoint)
        {
            switch (dataPoint.Iec104DataType)
            {
                case Iec104DataTypes.M_SP_NA_1:
                    return new SinglePointInformation(dataPoint.Address.ObjectAddress, false, new QualityDescriptor());
                case Iec104DataTypes.M_DP_NA_1:
                    return new DoublePointInformation(dataPoint.Address.ObjectAddress, DoublePointValue.ON, new QualityDescriptor());
                case Iec104DataTypes.M_ME_NA_1:
                    return new MeasuredValueScaled(dataPoint.Address.ObjectAddress, Random.Shared.Next(), new QualityDescriptor());
                case Iec104DataTypes.M_ST_NA_1://todo it is no measurment
                    return new StepCommand(dataPoint.Address.ObjectAddress, StepCommandValue.HIGHER, false, 0);
;                default:
                    throw new NotImplementedException("IecType is not implemented yet");
            }
            
        }
    }
}
