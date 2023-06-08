using IEC60870_5_104_simulator.Domain;
using lib60870.CS101;
using System.Diagnostics;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class InformationObjectFactory : IInformationObjectFactory
    {

        public InformationObject GetInformationObject(Iec104DataPoint dataPoint)
        {
            switch (dataPoint.Iec104DataTypes)
            {
                case Iec104DataTypes.M_SP_NA_1:
                    return new SinglePointInformation(dataPoint.ObjectAdress, false, new QualityDescriptor());
                case Iec104DataTypes.M_DP_NA_1:
                    return new DoublePointInformation(dataPoint.ObjectAdress, DoublePointValue.ON, new QualityDescriptor());
                case Iec104DataTypes.M_ME_NA_1:
                    return new MeasuredValueScaled(dataPoint.ObjectAdress, Random.Shared.Next(), new QualityDescriptor());
                default:
                    throw new NotImplementedException("IecType is not implemented yet");
            }
            
        }
    }
}
