using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using lib60870;
using lib60870.CS101;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class MirroredResponseFactory : ICommandResponseFactory
    {
        private readonly IDatapointRegistry registry;
        private readonly IInformationObjectTemplate template;

        public MirroredResponseFactory(IDatapointRegistry registry, IInformationObjectTemplate template)
        {
            this.registry = registry;
            this.template = template;
        }

        public InformationObject Update(Iec104CommandDataPointConfig commandCfg, InformationObject sentCommand)
        {
            var responseDataPoint = commandCfg.SimulatedDataPoint
                ?? throw new InvalidOperationException($"Command '{commandCfg.Id}' has no response datapoint assigned");
            switch (responseDataPoint.Iec104DataType)
            {
                case Iec104DataTypes.M_ST_NA_1:
                case Iec104DataTypes.M_ST_TA_1:
                case Iec104DataTypes.M_ST_TB_1:
                    int stepValue = CreateAdjustedStepValue(sentCommand, responseDataPoint.Address);
                    return template.GetStepposition(responseDataPoint.Address.ObjectAddress, new IecIntValueObject(stepValue), responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_SP_NA_1:
                case Iec104DataTypes.M_SP_TA_1:
                case Iec104DataTypes.M_SP_TB_1:
                    bool spaValue = CreateMirroredSinglePointValue(sentCommand, responseDataPoint.Address);
                    return template.GetSinglePoint(responseDataPoint.Address.ObjectAddress, new IecSinglePointValueObject(spaValue), responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_DP_NA_1:
                case Iec104DataTypes.M_DP_TA_1:
                case Iec104DataTypes.M_DP_TB_1:
                    IecDoublePointValue value_M_DP_NA_1 = CreateDoublePointValue(sentCommand, responseDataPoint.Address);
                    return template.GetDoublePoint(responseDataPoint.Address.ObjectAddress, new IecDoublePointValueObject(value_M_DP_NA_1), responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_ME_NB_1:
                case Iec104DataTypes.M_ME_TB_1:
                case Iec104DataTypes.M_ME_TE_1:
                    int scaled = CreateMeasuredValueScaled(sentCommand, responseDataPoint.Address);
                    return template.GetMeasuredValueScaled(responseDataPoint.Address.ObjectAddress, new IecValueScaledObject(new ScaledValueRecord(scaled)), responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_ME_NC_1:
                case Iec104DataTypes.M_ME_TC_1:
                case Iec104DataTypes.M_ME_TF_1:
                    float valueFloat= CreateMeasuredValueShort(sentCommand, responseDataPoint.Address);
                    return template.GetMeasuredValueShort(responseDataPoint.Address.ObjectAddress, new IecValueFloatObject(valueFloat), responseDataPoint.Iec104DataType);
                case Iec104DataTypes.M_ME_NA_1:
                case Iec104DataTypes.M_ME_TA_1:
                case Iec104DataTypes.M_ME_ND_1:
                    float valueNormalized = CreateMeasuredValueNormalized(sentCommand, responseDataPoint.Address);
                    return template.GetMeasuredValueNormalized(responseDataPoint.Address.ObjectAddress, new IecValueFloatObject(valueNormalized), responseDataPoint.Iec104DataType);
                default:
                    throw new NotImplementedException($"{responseDataPoint.Iec104DataType} is not implemented");
            }
        }

        private float CreateMeasuredValueNormalized(InformationObject sentCommand, IecAddress address)
        {
            if (sentCommand is SetpointCommandNormalized dc)
            {
                return dc.NormalizedValue;
            }
            else
                throw new InvalidCastException($"type {sentCommand}, Oa:{sentCommand.ObjectAddress}is not a {nameof(SetpointCommandNormalized)}");
        }

        private float CreateMeasuredValueShort(InformationObject sentCommand, IecAddress address)
        {
            if (sentCommand is SetpointCommandShort dc)
            {
                return dc.Value;
            }
            else
                throw new InvalidCastException($"type {sentCommand}, Oa:{sentCommand.ObjectAddress}is not a {nameof(SetpointCommandShort)}");
        }

        private int CreateMeasuredValueScaled(InformationObject sentCommand, IecAddress address)
        {
            if (sentCommand is SetpointCommandScaled dc)
            {
                return dc.ScaledValue?.Value ?? throw new InvalidCastException("ScaledValue is zero");
            }
            else
                throw new InvalidCastException($"type {sentCommand}, Oa:{sentCommand.ObjectAddress}is not a SetpointCommandScaled");
        }

        private IecDoublePointValue CreateDoublePointValue(InformationObject sentCommand, IecAddress address)
        {
            if (sentCommand is DoubleCommand)
            {
                DoubleCommand dc = (DoubleCommand)sentCommand;
                var dcValue = (IecDoublePointValue)dc.State;
                this.registry.SetValue(address, new IecDoublePointValueObject(dcValue));
                return dcValue;
            }
            else
                throw new InvalidCastException($"type {sentCommand}, Oa:{sentCommand.ObjectAddress}is not a doublecommand");
        }
        private bool CreateMirroredSinglePointValue(InformationObject sentCommand, IecAddress address)
        {
            return ((IecSinglePointValueObject)registry.Get(address).Value).Value;
        }

        private CP56Time2a GetCP56Now()
        {
            return new lib60870.CP56Time2a(DateTime.Now);
        }

        private CP24Time2a GetCP24Now()
        {
            return new lib60870.CP24Time2a(DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
        }

        private int CreateAdjustedStepValue(InformationObject commands, IecAddress address)
        {
            if (commands is StepCommand stepc)
            {
                int delta = stepc.State switch
                {
                    StepCommandValue.HIGHER => 1,
                    StepCommandValue.LOWER => -1,
                    _ => 0
                };

                if (delta == 0)
                    return -1;

                return registry.AdjustStep(address, delta);
            }

            throw new InvalidCastException($"type {commands}, {commands.ObjectAddress}is not a stepcommand");
        }
    }
}