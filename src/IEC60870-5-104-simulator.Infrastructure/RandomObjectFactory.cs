using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using lib60870;
using lib60870.CS101;
using System;
using System.Diagnostics;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class RandomObjectFactory : IInformationObjectFactory
    {

        public InformationObject GetInformationObject(Iec104DataPoint responseDataPoint)
        {
            int oa = responseDataPoint.Address.ObjectAddress;
            switch (responseDataPoint.Iec104DataType)
            {
                case Iec104DataTypes.M_ST_NA_1:
                    int stepValue = CreateAdjustedStepValue(responseDataPoint.Address);
                    return new StepPositionInformation(oa, (int)stepValue, true, new QualityDescriptor());
                //case Iec104DataTypes.M_ST_TA_1:
                //    int stepValuetime = CreateAdjustedStepValue(sentCommand, responseDataPoint.Address);
                //    return new StepPositionWithCP24Time2a(oa, (int)stepValuetime, true, new QualityDescriptor(), GetCP24Now());
                //case Iec104DataTypes.M_ST_TB_1:
                //    int stepValuetime56 = CreateAdjustedStepValue(sentCommand, responseDataPoint.Address);
                //    return new StepPositionWithCP56Time2a(oa, (int)stepValuetime56, true, new QualityDescriptor(), GetCP56Now());
                //case Iec104DataTypes.M_SP_NA_1:
                //    bool spaValue = CreateMirroredSinglePointValue(sentCommand, responseDataPoint.Address);
                //    return new SinglePointInformation(oa, spaValue, new QualityDescriptor());
                //case Iec104DataTypes.M_SP_TA_1:
                //    bool value_M_SP_TA_1 = CreateMirroredSinglePointValue(sentCommand, responseDataPoint.Address);
                //    return new SinglePointWithCP24Time2a(oa, value_M_SP_TA_1, new QualityDescriptor(), GetCP24Now());
                //case Iec104DataTypes.M_SP_TB_1:
                //    bool value_M_SP_TB_1 = CreateMirroredSinglePointValue(sentCommand, responseDataPoint.Address);
                //    return new SinglePointWithCP56Time2a(oa, value_M_SP_TB_1, new QualityDescriptor(), GetCP56Now());
                //case Iec104DataTypes.M_DP_NA_1:
                //    DoublePointValue value_M_DP_NA_1 = CreateDoublePointValue(sentCommand, responseDataPoint.Address);
                //    return new DoublePointInformation(oa, value_M_DP_NA_1, new QualityDescriptor());
                case Iec104DataTypes.M_DP_TA_1:
                    DoublePointValue value_M_DP_TA_1 = CreateDoublePointValue();
                    return new DoublePointWithCP24Time2a(oa, value_M_DP_TA_1, new QualityDescriptor(), GetCP24Now());
                case Iec104DataTypes.M_DP_TB_1:
                    DoublePointValue value_M_DP_TB_1 = CreateDoublePointValue();
                    return new DoubleCommandWithCP56Time2a(oa, (int)value_M_DP_TB_1, false, 0, GetCP56Now());
                case Iec104DataTypes.M_ME_NA_1:
                    return new MeasuredValueScaled(oa, Random.Shared.Next(), new QualityDescriptor());
                default:
                    throw new NotImplementedException($"{responseDataPoint.Iec104DataType} is not implemented");
            }
        }

        private DoublePointValue CreateDoublePointValue()
        {
            Random random = new();
            return random.NextDouble() >= 0.5 ? DoublePointValue.OFF : DoublePointValue.ON;
        }
        private int CreateAdjustedStepValue(IecAddress address)
        {
            Random random = new();
            return (int)random.NextInt64(1, 5);
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
