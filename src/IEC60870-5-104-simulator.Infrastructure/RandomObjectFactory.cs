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
        private readonly IInformationObjectTemplate template;
        private readonly Random random;

        public RandomObjectFactory(IInformationObjectTemplate template)
        {
            this.template = template;
            random = new();
        }

        public InformationObject GetInformationObject(Iec104DataPoint responseDataPoint)
        {
            int oa = responseDataPoint.Address.ObjectAddress;
            switch (responseDataPoint.Iec104DataType)
            {
                case Iec104DataTypes.M_ST_NA_1:
                case Iec104DataTypes.M_ST_TA_1:
                case Iec104DataTypes.M_ST_TB_1:
                    int stepValue = CreateAdjustedStepValue();
                    return template.GetStepposition(responseDataPoint.Address.ObjectAddress, new IecIntValueObject(stepValue), responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_SP_NA_1:
                case Iec104DataTypes.M_SP_TA_1:
                case Iec104DataTypes.M_SP_TB_1:
                    bool spaValue = CreateSinglePointValue();
                    return template.GetSinglePoint(responseDataPoint.Address.ObjectAddress, new IecSinglePointValueObject(spaValue), responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_DP_NA_1:
                case Iec104DataTypes.M_DP_TA_1:
                case Iec104DataTypes.M_DP_TB_1:
                    IecDoublePointValue value_M_DP_NA_1 = CreateDoublePointValue();
                    return template.GetDoublePoint(responseDataPoint.Address.ObjectAddress, new IecDoublePointValueObject(value_M_DP_NA_1), responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_ME_NB_1:
                case Iec104DataTypes.M_ME_TB_1:
                case Iec104DataTypes.M_ME_TE_1:
                    int scaled = CreateMeasuredValueScaled();
                    return template.GetMeasuredValueScaled(responseDataPoint.Address.ObjectAddress, new IecIntValueObject(scaled), responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_ME_NC_1:
                case Iec104DataTypes.M_ME_TC_1:
                case Iec104DataTypes.M_ME_TF_1:
                    float valueFloat = CreateRandomFloat();
                    return template.GetMeasuredValueShort(responseDataPoint.Address.ObjectAddress, new IecValueFloatObject(valueFloat), responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_ME_NA_1:
                case Iec104DataTypes.M_ME_TA_1:
                case Iec104DataTypes.M_ME_ND_1:
                    float valueNormalized = CreateRandomFloat();
                    return template.GetMeasuredValueNormalized(responseDataPoint.Address.ObjectAddress, new IecValueFloatObject(valueNormalized), responseDataPoint.Iec104DataType);
                default:
                    throw new NotImplementedException($"{responseDataPoint.Iec104DataType} is not implemented");
            }
        }

        private float CreateRandomFloat()
        {
            return NextFloat(random);
        }

        private int CreateMeasuredValueScaled()
        {
            return random.Next();
        }

        private bool CreateSinglePointValue()
        {
            return random.NextDouble() >= 0.5;
        }

        private IecDoublePointValue CreateDoublePointValue()
        {
            return random.NextDouble() >= 0.5 ? IecDoublePointValue.OFF : IecDoublePointValue.ON;
        }
        private int CreateAdjustedStepValue()
        {
            return random.Next(-64, 63);
        }
        static float NextFloat(Random random)
        {
            double mantissa = (random.NextDouble() * 2.0) - 1.0;
            // choose -149 instead of -126 to also generate subnormal floats (*)
            double exponent = Math.Pow(2.0, random.Next(-126, 128));
            return (float)(mantissa * exponent);
        }
    }
}