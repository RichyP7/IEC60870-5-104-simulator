using Castle.Core.Logging;
using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Service;
using lib60870.CS101;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit.Sdk;

namespace IEC60870_5_104_simulator.Infrastructure.Tests
{
    public class IeC104ServerRunnerTest
    {
        private Mock<IInformationObjectFactory> mockFactory;
        private Mock<ICommandResponseFactory> mockCommandFactory;
        private Mock<IValueSimulatorFactory> mockValueFactory;
        lib60870.CS104.Server testServer;
        private Iec104Service service;
        public IeC104ServerRunnerTest() 
        {
            mockFactory = new();
            mockCommandFactory = new();
            mockValueFactory = new();
            testServer = new lib60870.CS104.Server();
            service = new Iec104Service(testServer, mockFactory.Object, mockCommandFactory.Object,NullLogger<Iec104Service>.Instance,new  Iec104ConfigurationService(), mockValueFactory.Object ); ;
        }
        [Fact]
        public void SimulateDataTest()
        {
            mockFactory.Setup(v => v.GetInformationObject(It.IsAny<Iec104DataPointConfig>())).Returns(new DoublePointWithCP24Time2a(1000, DoublePointValue.OFF, new QualityDescriptor(), new lib60870.CP24Time2a()));
            
            service.SimulateValues();

            mockFactory.Verify(v=> v.GetInformationObject(It.IsAny<Iec104DataPointConfig>()));
        }
    }
}