using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using lib60870.CS101;
using IEC60870_5_104_simulator.Domain.Interfaces;
using System.Net;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class ObjectFactory : IInformationObjectFactory
    {
        private readonly IInformationObjectTemplate template;
        private readonly IIecValueRepository repository;
        private readonly Random random;

        public ObjectFactory(IInformationObjectTemplate template, IIecValueRepository valueRepository)
        {
            this.template = template;
            this.repository = valueRepository;
            random = new();
        }

        public InformationObject CreateRandomInformationObject(Iec104DataPoint responseDataPoint)
        {
            switch (responseDataPoint.Iec104DataType)
            {
                case Iec104DataTypes.M_ST_NA_1:
                case Iec104DataTypes.M_ST_TA_1:
                case Iec104DataTypes.M_ST_TB_1:
                    int stepValue = CreateAdjustedStepValue(responseDataPoint.Address);
                    return template.GetStepposition(responseDataPoint.Address.ObjectAddress, new IecIntValueObject(stepValue), responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_SP_NA_1:
                case Iec104DataTypes.M_SP_TA_1:
                case Iec104DataTypes.M_SP_TB_1:
                    bool spaValue = CreateSinglePointValue(responseDataPoint.Address);
                    return template.GetSinglePoint(responseDataPoint.Address.ObjectAddress, new IecSinglePointValueObject(spaValue), responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_DP_NA_1:
                case Iec104DataTypes.M_DP_TA_1:
                case Iec104DataTypes.M_DP_TB_1:
                    IecDoublePointValue value_M_DP_NA_1 = CreateDoublePointValue(responseDataPoint.Address);
                    return template.GetDoublePoint(responseDataPoint.Address.ObjectAddress, new IecDoublePointValueObject(value_M_DP_NA_1), responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_ME_NB_1:
                case Iec104DataTypes.M_ME_TB_1:
                case Iec104DataTypes.M_ME_TE_1:
                    int scaled = CreateMeasuredValueScaled(responseDataPoint.Address);
                    return template.GetMeasuredValueScaled(responseDataPoint.Address.ObjectAddress, new IecValueScaledObject(new ScaledValueRecord(scaled)), responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_ME_NC_1:
                case Iec104DataTypes.M_ME_TC_1:
                case Iec104DataTypes.M_ME_TF_1:
                    float valueFloat = CreateRandomFloat(responseDataPoint.Address);
                    return template.GetMeasuredValueShort(responseDataPoint.Address.ObjectAddress, new IecValueFloatObject(valueFloat), responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_ME_NA_1:
                case Iec104DataTypes.M_ME_TA_1:
                case Iec104DataTypes.M_ME_ND_1:
                    float valueNormalized = CreateRandomFloat(responseDataPoint.Address);
                    return template.GetMeasuredValueNormalized(responseDataPoint.Address.ObjectAddress, new IecValueFloatObject(valueNormalized), responseDataPoint.Iec104DataType);
                default:
                    throw new NotImplementedException($"{responseDataPoint.Iec104DataType} is not implemented");
            }
        }
        
        public InformationObject CreateInformationObjectWithValue(Iec104DataPoint responseDataPoint, IecValueObject value)
        {
            switch (responseDataPoint.Iec104DataType)
            {
                case Iec104DataTypes.M_ST_NA_1:
                case Iec104DataTypes.M_ST_TA_1:
                case Iec104DataTypes.M_ST_TB_1:
                    return template.GetStepposition(responseDataPoint.Address.ObjectAddress, value, responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_SP_NA_1:
                case Iec104DataTypes.M_SP_TA_1:
                case Iec104DataTypes.M_SP_TB_1:
                    return template.GetSinglePoint(responseDataPoint.Address.ObjectAddress, value, responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_DP_NA_1:
                case Iec104DataTypes.M_DP_TA_1:
                case Iec104DataTypes.M_DP_TB_1:
                    return template.GetDoublePoint(responseDataPoint.Address.ObjectAddress, (IecDoublePointValueObject)value, responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_ME_NB_1:
                case Iec104DataTypes.M_ME_TB_1:
                case Iec104DataTypes.M_ME_TE_1:
                    return template.GetMeasuredValueScaled(responseDataPoint.Address.ObjectAddress,(IecValueScaledObject)value, responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_ME_NC_1:
                case Iec104DataTypes.M_ME_TC_1:
                case Iec104DataTypes.M_ME_TF_1:
                    return template.GetMeasuredValueShort(responseDataPoint.Address.ObjectAddress, (IecValueFloatObject)value, responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_ME_NA_1:
                case Iec104DataTypes.M_ME_TA_1:
                case Iec104DataTypes.M_ME_ND_1:
                    return template.GetMeasuredValueNormalized(responseDataPoint.Address.ObjectAddress, (IecValueFloatObject)value, responseDataPoint.Iec104DataType);
                default:
                    throw new NotImplementedException($"{responseDataPoint.Iec104DataType} is not implemented");
            }
        }
        private float CreateRandomFloat(IecAddress address)
        {
            float myvalue = NextFloat(random);
            this.repository.SetObjectValue(address, new IecValueFloatObject(myvalue));
            return myvalue;
        }

        private int CreateMeasuredValueScaled(IecAddress address)
        {
            int myvalue = random.Next();
            this.repository.SetObjectValue(address, new IecIntValueObject( myvalue));
            return myvalue;
        }

        private bool CreateSinglePointValue(IecAddress address)
        {
            bool myvalue = random.NextDouble() >= 0.5;
            this.repository.SetSinglePoint(address, myvalue);
            return myvalue;
        }

        private IecDoublePointValue CreateDoublePointValue(IecAddress address)
        {
            IecDoublePointValue myvalue = random.NextDouble() >= 0.5 ? IecDoublePointValue.OFF : IecDoublePointValue.ON;
            this.repository.SetDoublePoint(address, myvalue);
            return myvalue;
            
        }
        private int CreateAdjustedStepValue(IecAddress address)
        {
            int myvalue=  random.Next(-64, 63);
            this.repository.SetStepValue(address, myvalue);
            return myvalue;
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