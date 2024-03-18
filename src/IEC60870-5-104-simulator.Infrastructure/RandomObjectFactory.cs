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
        //Todo: implement remaining types

        public InformationObject GetInformationObject(Iec104DataPoint responseDataPoint)
        {
            int oa = responseDataPoint.Address.ObjectAddress;
            switch (responseDataPoint.Iec104DataType)
            {
                case Iec104DataTypes.M_ST_NA_1:
                    int stepValue = CreateAdjustedStepValue(responseDataPoint.Address);
                    return new StepPositionInformation(oa, (int)stepValue, true, new QualityDescriptor());case Iec104DataTypes.M_DP_TA_1:
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
