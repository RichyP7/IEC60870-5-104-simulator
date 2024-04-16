using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using lib60870.CS101;
using Moq;

namespace IEC60870_5_104_simulator.Infrastructure.Tests
{
    public class RandomObjectFactoryTest
    {
        private RandomObjectFactory factory;
        private Mock<IInformationObjectTemplate> templateMock;
        public RandomObjectFactoryTest()
        {
            templateMock = new Mock<IInformationObjectTemplate>();
            factory = new RandomObjectFactory( templateMock.Object);
        }
        [Fact]
        public void GetInformationObjectTest_GetStepposition_Verify()
        {
            Iec104DataTypes testType = Iec104DataTypes.M_ST_NA_1;
            Iec104DataPoint dp = new Iec104DataPoint(new Domain.ValueTypes.IecAddress(123, 89), testType);
            var item = factory.GetInformationObject(dp);

            templateMock.Verify(v => v.GetStepposition(89, It.IsAny<IecValueObject>(), testType));
        }
        [Fact]
        public void GetInformationObjectTest_GetDoublePoint_Verify()
        {
            Iec104DataTypes testType = Iec104DataTypes.M_DP_TA_1;
            Iec104DataPoint dp = new Iec104DataPoint(new Domain.ValueTypes.IecAddress(123, 89), testType);
            var item = factory.GetInformationObject(dp);

            templateMock.Verify(v => v.GetDoublePoint(89, It.IsAny<IecDoublePointValueObject>(), testType));
        }
    }
}
