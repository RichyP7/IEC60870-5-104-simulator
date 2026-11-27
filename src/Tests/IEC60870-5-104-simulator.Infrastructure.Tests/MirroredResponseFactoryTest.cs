using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using lib60870.CS101;
using Moq;

namespace IEC60870_5_104_simulator.Infrastructure.Tests
{
    public class MirroredResponseFactoryTest
    {
        private static readonly IecAddress ResponseAddress = new(123, 89);

        private MirroredResponseFactory factory;
        private Mock<IInformationObjectTemplate> templateMock;
        private Mock<IDatapointRegistry> registryMock;

        public MirroredResponseFactoryTest()
        {
            registryMock = new Mock<IDatapointRegistry>();
            registryMock
                .Setup(r => r.Get(ResponseAddress))
                .Returns(new Iec104DataPoint(ResponseAddress, Iec104DataTypes.M_ST_NA_1));

            templateMock = new Mock<IInformationObjectTemplate>();
            factory = new MirroredResponseFactory(registryMock.Object, templateMock.Object);
        }

        private static Iec104CommandDataPointConfig StepCommandConfig()
        {
            var cmd = new Iec104CommandDataPointConfig(new IecAddress(123, 567), Iec104DataTypes.M_ST_NA_1);
            cmd.SimulatedDataPoint = new Iec104DataPoint(ResponseAddress, Iec104DataTypes.M_ST_NA_1);
            return cmd;
        }

        [Fact]
        public void GetResponseInformationObjectTest()
        {
            var cmd = StepCommandConfig();
            factory.Update(cmd, new Mock<StepCommand>(10, StepCommandValue.HIGHER, true, 1).Object);

            templateMock.Verify(v => v.GetStepposition(89, It.IsAny<IecValueObject>(), Iec104DataTypes.M_ST_NA_1));
        }

        [Fact]
        public void Update_StepHigher_AsksRegistryToRaiseByOne()
        {
            var cmd = StepCommandConfig();
            var mock = new Mock<StepCommand>(10, StepCommandValue.HIGHER, true, 10);

            factory.Update(cmd, mock.Object);

            registryMock.Verify(r => r.AdjustStep(ResponseAddress, 1));
        }

        [Fact]
        public void Update_StepLower_AsksRegistryToLowerByOne()
        {
            var cmd = StepCommandConfig();
            var mock = new Mock<StepCommand>(10, StepCommandValue.LOWER, true, 10);

            factory.Update(cmd, mock.Object);

            registryMock.Verify(r => r.AdjustStep(ResponseAddress, -1));
        }
    }
}
