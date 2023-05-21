using lib60870.CS101;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class InformationObjectFactory : IInformationObjectFactory
    {
        public InformationObject GetInformationObject(string name)
        {
            SinglePointInformation spi = new SinglePointInformation(1000, true, new QualityDescriptor());
            return spi;
        }
    }
}