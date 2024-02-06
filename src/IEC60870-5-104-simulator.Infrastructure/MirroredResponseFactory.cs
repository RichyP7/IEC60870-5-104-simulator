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
        private readonly IIecValueLocalStorageRepository repository;

        public MirroredResponseFactory(IIecValueLocalStorageRepository repository)
        {
            this.repository = repository;
        }

        public InformationObject GetResponseInformationObject(Iec104CommandDataPointConfig commandCfg, InformationObject sentCommand)
        {
            var responseDataPoint = commandCfg.SimulatedDataPoint;
            switch (responseDataPoint.Iec104DataType)
            {
                case Iec104DataTypes.M_ST_NA_1:
                    int stepValue = CreateAdjustedStepValue(sentCommand, responseDataPoint.Address);
                    return new StepPositionInformation(responseDataPoint.Address.ObjectAddress, (int)stepValue, true, new QualityDescriptor());
                case Iec104DataTypes.M_ST_TA_1:
                    int stepValuetime = CreateAdjustedStepValue(sentCommand, responseDataPoint.Address);
                    return new StepPositionWithCP24Time2a(responseDataPoint.Address.ObjectAddress, (int)stepValuetime, true, new QualityDescriptor(), GetCP24Now());
                case Iec104DataTypes.M_ST_TB_1:
                    int stepValuetime56 = CreateAdjustedStepValue(sentCommand, responseDataPoint.Address);
                    return new StepPositionWithCP56Time2a(responseDataPoint.Address.ObjectAddress, (int)stepValuetime56, true, new QualityDescriptor(), GetCP56Now());
                case Iec104DataTypes.M_SP_NA_1:
                    bool spaValue = CreateMirroredSinglePointValue(sentCommand, responseDataPoint.Address);
                    return new SinglePointInformation(responseDataPoint.Address.ObjectAddress,spaValue, new QualityDescriptor());
                case Iec104DataTypes.M_SP_TA_1:
                    bool value_M_SP_TA_1 = CreateMirroredSinglePointValue(sentCommand, responseDataPoint.Address);
                    return new SinglePointWithCP24Time2a(responseDataPoint.Address.ObjectAddress, value_M_SP_TA_1, new QualityDescriptor(), GetCP24Now());
                case Iec104DataTypes.M_SP_TB_1:
                    bool value_M_SP_TB_1 = CreateMirroredSinglePointValue(sentCommand, responseDataPoint.Address);
                    return new SinglePointWithCP56Time2a(responseDataPoint.Address.ObjectAddress, value_M_SP_TB_1, new QualityDescriptor(), GetCP56Now());
                case Iec104DataTypes.M_DP_NA_1:
                    DoublePointValue value_M_DP_NA_1 = CreateDoublePointValue(sentCommand, responseDataPoint.Address);
                    return new DoublePointInformation(responseDataPoint.Address.ObjectAddress, value_M_DP_NA_1, new QualityDescriptor());
                case Iec104DataTypes.M_DP_TA_1:
                    DoublePointValue value_M_DP_TA_1 = CreateDoublePointValue(sentCommand, responseDataPoint.Address);
                    return new DoublePointWithCP24Time2a(responseDataPoint.Address.ObjectAddress, value_M_DP_TA_1, new QualityDescriptor(), GetCP24Now());
                case Iec104DataTypes.M_DP_TB_1:
                    DoublePointValue value_M_DP_TB_1 = CreateDoublePointValue(sentCommand, responseDataPoint.Address);
                    return new DoubleCommandWithCP56Time2a(responseDataPoint.Address.ObjectAddress,(int)value_M_DP_TB_1,false,0, GetCP56Now());
                case Iec104DataTypes.M_ME_NA_1:
                    return new MeasuredValueScaled(responseDataPoint.Address.ObjectAddress, Random.Shared.Next(), new QualityDescriptor());
                default:
                    throw new NotImplementedException($"{responseDataPoint.Iec104DataType} is not implemented");
            }
        }

        private DoublePointValue CreateDoublePointValue(InformationObject sentCommand, IecAddress address)
        {
            if (sentCommand is DoubleCommand)
            {
                DoubleCommand dc = (DoubleCommand)sentCommand;
                var dcValue = (IecDoublePointValue)dc.State;
                this.repository.SetDoublePoint(address, dcValue);
                return (DoublePointValue)dcValue;
            }
            else
                throw new InvalidCastException($"type {sentCommand}, Oa:{sentCommand.ObjectAddress}is not a doublecommand");
        }
        private bool CreateMirroredSinglePointValue(InformationObject sentCommand, IecAddress address)
        {
            return repository.GetSinglePoint(address);
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

            if (commands is StepCommand)
            {
                StepCommand stepc = (StepCommand)commands;
                int stepState;
                if (stepc.State == StepCommandValue.HIGHER)
                {
                    stepState = repository.GetStepValue(address);
                    stepState++;
                }
                else if (stepc.State == StepCommandValue.LOWER)
                {
                    stepState = repository.GetStepValue(address);
                    stepState--;
                }
                else
                    return -1;
                repository.SetStepValue(address, stepState);
                return stepState;
            }
            else
                throw new InvalidCastException($"type {commands}, {commands.ObjectAddress}is not a stepcommand");
        }
    }
}