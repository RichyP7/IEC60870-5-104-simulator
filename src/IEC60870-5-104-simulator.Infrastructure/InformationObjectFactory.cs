using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using lib60870;
using lib60870.CS101;
using System.Diagnostics;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class InformationObjectFactory : IInformationObjectFactory
    {

        public InformationObject GetInformationObject(Iec104DataPoint dataPoint)
        {
            switch (dataPoint.Iec104DataType)
            {

                case Iec104DataTypes.M_ST_NA_1:
                    return new StepPositionInformation(dataPoint.Address.ObjectAddress, 0, true, new QualityDescriptor());
                case Iec104DataTypes.M_ST_TA_1:
                    return new StepPositionWithCP24Time2a(dataPoint.Address.ObjectAddress, 0, true, new QualityDescriptor(), GetCP24Now());
                case Iec104DataTypes.M_ST_TB_1:
                    return new StepPositionWithCP56Time2a(dataPoint.Address.ObjectAddress, 06, true, new QualityDescriptor(), GetCP56Now());
                case Iec104DataTypes.M_SP_NA_1:
                    return new SinglePointInformation(dataPoint.Address.ObjectAddress, true, new QualityDescriptor());
                case Iec104DataTypes.M_SP_TA_1:
                    return new SinglePointWithCP24Time2a(dataPoint.Address.ObjectAddress, true, new QualityDescriptor(), GetCP24Now());
                case Iec104DataTypes.M_SP_TB_1:
                    return new SinglePointWithCP56Time2a(dataPoint.Address.ObjectAddress, true, new QualityDescriptor(), GetCP56Now());
                case Iec104DataTypes.M_DP_NA_1:
                    return new DoublePointInformation(dataPoint.Address.ObjectAddress, DoublePointValue.OFF, new QualityDescriptor());
                case Iec104DataTypes.M_DP_TA_1:
                    return new DoublePointWithCP24Time2a(dataPoint.Address.ObjectAddress, DoublePointValue.OFF, new QualityDescriptor(), GetCP24Now());
                case Iec104DataTypes.M_DP_TB_1:
                    return new DoubleCommandWithCP56Time2a(dataPoint.Address.ObjectAddress, 1, false, 0, GetCP56Now());
                case Iec104DataTypes.M_ME_NA_1:
                    return new MeasuredValueScaled(dataPoint.Address.ObjectAddress, Random.Shared.Next(), new QualityDescriptor());
                default:
                    throw new NotImplementedException($"{dataPoint.Iec104DataType} is not implemented");
            }
        }

        private CP56Time2a GetCP56Now()
        {
            return new lib60870.CP56Time2a(DateTime.Now);
        }

        private static CP24Time2a GetCP24Now()
        {
            return new lib60870.CP24Time2a(DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
        }
    }
}
