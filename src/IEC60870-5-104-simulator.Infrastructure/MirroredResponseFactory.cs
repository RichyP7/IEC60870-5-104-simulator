using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Infrastructure;
using lib60870.CS101;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class MirroredResponseFactory : ICommandResponseFactory
    {
        public InformationObject GetResponseInformationObject(Iec104CommandDataPointConfig commandCfg, InformationObject sentCommand)
        {
            switch (commandCfg.SimulatedDataPoint.Iec104DataType)
            {
                case Iec104DataTypes.M_ST_NA_1:
                    var stepValue = GetStepValue(sentCommand);
                    return new StepPositionInformation(commandCfg.Adress.ObjectAddress, (int)stepValue, true, new QualityDescriptor());
                default:
                    throw new NotImplementedException();
            }
        }

        private StepCommandValue GetStepValue(InformationObject commands)
        {
            if (commands is StepCommand)
            {
                return ((StepCommand)commands).State;
            }
            else
                throw new InvalidCastException("type is not a stepcommand");
        }
    }
}