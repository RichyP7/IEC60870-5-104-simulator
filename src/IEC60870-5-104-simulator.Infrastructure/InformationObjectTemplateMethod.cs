using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using lib60870;
using lib60870.CS101;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IEC60870_5_104_simulator.Infrastructure
{
    internal class InformationObjectTemplateMethod : IInformationObjectTemplate
    {
        public InformationObject GetStepposition(int objectAddress, IecValueObject value, Iec104DataTypes type)
        {
            return type switch
            {
                Iec104DataTypes.M_ST_NA_1 => new StepPositionInformation(objectAddress, (int)value.GetValue(), true, new QualityDescriptor()),
                Iec104DataTypes.M_ST_TA_1 => new StepPositionWithCP24Time2a(objectAddress, (int)value.GetValue(), true, new QualityDescriptor(), GetCP24Now()),
                Iec104DataTypes.M_ST_TB_1 => new StepPositionWithCP56Time2a(objectAddress, (int)value.GetValue(), true, new QualityDescriptor(), GetCP56Now()),
                _ => throw new NotImplementedException("no stepposition for this type"),
            };
        }
        private CP56Time2a GetCP56Now()
        {
            return new lib60870.CP56Time2a(DateTime.Now);
        }

        private CP24Time2a GetCP24Now()
        {
            return new lib60870.CP24Time2a(DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
        }

        public InformationObject GetSinglePoint(int objectAddress, IecValueObject value, Iec104DataTypes type)
        {
            return type switch
            {
                Iec104DataTypes.M_SP_NA_1 => new SinglePointInformation(objectAddress, (bool)value.GetValue(), new QualityDescriptor()),
                Iec104DataTypes.M_SP_TA_1 => new SinglePointWithCP24Time2a(objectAddress, (bool)value.GetValue(), new QualityDescriptor(), GetCP24Now()),
                Iec104DataTypes.M_SP_TB_1 => new SinglePointWithCP56Time2a(objectAddress, (bool)value.GetValue(), new QualityDescriptor(), GetCP56Now()),
                _ => throw new NotImplementedException("no singlepoint for this type"),
            };
        }

        public InformationObject GetDoublePoint(int objectAddress, IecDoublePointValueObject value, Iec104DataTypes type)
        {
            return type switch
            {
                Iec104DataTypes.M_DP_NA_1 => new DoublePointInformation(objectAddress, (DoublePointValue)value.GetValue(), new QualityDescriptor()),
                Iec104DataTypes.M_DP_TA_1 => new DoublePointWithCP24Time2a(objectAddress, (DoublePointValue)value.GetValue(), new QualityDescriptor(), GetCP24Now()),
                Iec104DataTypes.M_DP_TB_1 => new DoublePointWithCP56Time2a(objectAddress, (DoublePointValue)value.GetValue(), new QualityDescriptor(), GetCP56Now()),
                _ => throw new NotImplementedException("no doublepoint for this type"),
            };

        }

        public InformationObject GetMeasuredValueScaled(int objectAddress, IecIntValueObject value, Iec104DataTypes type)
        {
            return type switch
            {
                Iec104DataTypes.M_ME_NB_1 => new MeasuredValueScaled(objectAddress, (int)value.GetValue(), new QualityDescriptor()),
                Iec104DataTypes.M_ME_TB_1 => new MeasuredValueScaledWithCP24Time2a(objectAddress, (int)value.GetValue(), new QualityDescriptor(), GetCP24Now()),
                Iec104DataTypes.M_ME_TE_1 => new MeasuredValueScaledWithCP56Time2a(objectAddress, (int)value.GetValue(), new QualityDescriptor(), GetCP56Now()),
                _ => throw new NotImplementedException($"no measuredvaluescaled for this type {type}"),
            };
        }

        public InformationObject GetMeasuredValueShort(int objectAddress, IecValueShortObject value, Iec104DataTypes type)
        {
            return type switch
            {
                Iec104DataTypes.M_ME_NC_1 => new MeasuredValueShort(objectAddress, (float)value.GetValue(), new QualityDescriptor()),
                Iec104DataTypes.M_ME_TC_1 => new MeasuredValueShortWithCP24Time2a(objectAddress, (int)value.GetValue(), new QualityDescriptor(), GetCP24Now()),
                Iec104DataTypes.M_ME_TF_1 => new MeasuredValueShortWithCP56Time2a(objectAddress, (int)value.GetValue(), new QualityDescriptor(), GetCP56Now()),
                _ => throw new NotImplementedException($"no {nameof(MeasuredValueShort)} for this type {type}"),
            };
        }
    }
}
