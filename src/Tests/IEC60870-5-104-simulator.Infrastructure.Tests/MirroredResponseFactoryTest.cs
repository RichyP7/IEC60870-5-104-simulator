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
        private MirroredResponseFactory factory;
        private Mock<IInformationObjectTemplate> templateMock;
        private Mock<IIecValueRepository> storageMock;
        private Mock<InformationObject> infoObjectMock;
        public MirroredResponseFactoryTest()
        {
            storageMock = new Mock<IIecValueRepository>();

            templateMock = new Mock<IInformationObjectTemplate>();
            factory = new MirroredResponseFactory(storageMock.Object, templateMock.Object);

        }
        [Fact]
        public void GetResponseInformationObjectTest()
        {
            var cmd = new Iec104CommandDataPointConfig(new Domain.ValueTypes.IecAddress(123, 567), Iec104DataTypes.M_ST_NA_1);
            cmd.SimulatedDataPoint = new Iec104DataPoint(new Domain.ValueTypes.IecAddress(123, 89), Iec104DataTypes.M_ST_NA_1);
            factory.Update(cmd, new Mock<StepCommand>(10, StepCommandValue.HIGHER, true, 1).Object);

            templateMock.Verify(v => v.GetStepposition(89, It.IsAny<IecValueObject>(), Iec104DataTypes.M_ST_NA_1));
        }
        [Theory]
        [InlineData(20)]
        [InlineData(0)]
        [InlineData(-5)]
        [InlineData(62)]
        public void GetResponseInformationObjectTest_VerifyIncrease(int value)
        {
            var cmd = new Iec104CommandDataPointConfig(new Domain.ValueTypes.IecAddress(123, 567), Iec104DataTypes.M_ST_NA_1);
            cmd.SimulatedDataPoint = new Iec104DataPoint(new Domain.ValueTypes.IecAddress(123, 89), Iec104DataTypes.M_ST_NA_1);
            Mock<StepCommand> mock = new Mock<StepCommand>(10, StepCommandValue.HIGHER, true, 10);
            storageMock.Setup(v => v.GetStepValue(It.IsAny<IecAddress>())).Returns(value);
            factory.Update(cmd, mock.Object);

            storageMock.Verify(v => v.SetStepValue(It.IsAny<IecAddress>(), value +1));
        }
        [Theory]
        [InlineData(20)]
        [InlineData(0)]
        [InlineData(-5)]
        public void Update_VerifyDecrease(int value)
        {
            var cmd = new Iec104CommandDataPointConfig(new Domain.ValueTypes.IecAddress(123, 567), Iec104DataTypes.M_ST_NA_1);
            cmd.SimulatedDataPoint = new Iec104DataPoint(new Domain.ValueTypes.IecAddress(123, 89), Iec104DataTypes.M_ST_NA_1);
            Mock<StepCommand> mock = new Mock<StepCommand>(10, StepCommandValue.LOWER, true, 10);
            storageMock.Setup(v => v.GetStepValue(It.IsAny<IecAddress>())).Returns(value);
            factory.Update(cmd, mock.Object);

            storageMock.Verify(v => v.SetStepValue(It.IsAny<IecAddress>(), value -1));
        }

        [Theory]
        [InlineData(65, true)]
        [InlineData(-64,false)]
        public void Update_CheckLimit_VerifyNoIncrease(int value, bool increase)
        {
            var cmd = new Iec104CommandDataPointConfig(new Domain.ValueTypes.IecAddress(123, 567), Iec104DataTypes.M_ST_NA_1);
            cmd.SimulatedDataPoint = new Iec104DataPoint(new Domain.ValueTypes.IecAddress(123, 89), Iec104DataTypes.M_ST_NA_1);
            Mock<StepCommand> mock = new Mock<StepCommand>(10, increase ? StepCommandValue.HIGHER : StepCommandValue.LOWER, true, 10);
            storageMock.Setup(v => v.GetStepValue(It.IsAny<IecAddress>())).Returns(value);
            factory.Update(cmd, mock.Object);

            storageMock.Verify(v => v.SetStepValue(It.IsAny<IecAddress>(), value));
        }
    }
}
