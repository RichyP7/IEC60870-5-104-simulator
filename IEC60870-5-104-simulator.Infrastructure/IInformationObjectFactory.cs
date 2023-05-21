using lib60870.CS101;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public interface IInformationObjectFactory
    {
        InformationObject GetInformationObject(string name);
    }
}