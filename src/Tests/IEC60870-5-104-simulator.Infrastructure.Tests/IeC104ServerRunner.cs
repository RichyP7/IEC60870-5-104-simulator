using Castle.Core.Logging;
using lib60870.CS101;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit.Sdk;

namespace IEC60870_5_104_simulator.Infrastructure.Tests
{
    public class IeC104ServerRunnerTest
    {
        private Mock<IInformationObjectFactory> mockFactory;
        lib60870.CS104.Server testServer;
        private Iec104Service service;
        public IeC104ServerRunnerTest() 
        {
            mockFactory = new();
            testServer = new lib60870.CS104.Server();
            service = new Iec104Service(testServer, mockFactory.Object, NullLogger< Iec104Service>.Instance);
        }
        [Fact]
        public void SimulateDataTest()
        {
            mockFactory.Setup(v => v.GetInformationObject(It.IsAny<string>())).Returns(new DoublePointWithCP24Time2a(1000, DoublePointValue.OFF, new QualityDescriptor(), new lib60870.CP24Time2a()));
            
            service.SimulateValues();
            throw new Exception();

            mockFactory.Verify(v=> v.GetInformationObject(It.IsAny<string>()));
        }
    }
}