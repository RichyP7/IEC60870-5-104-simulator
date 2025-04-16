using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using lib60870.CS101;
using Moq;

namespace IEC60870_5_104_simulator.Infrastructure.Tests
{
    public class ObjectFactoryTest
    {
        private ObjectFactory factory;
        private Mock<IInformationObjectTemplate> templateMock;
        private Mock<IIecValueRepository> templateRepository;
        public ObjectFactoryTest()
        {
            templateMock = new Mock<IInformationObjectTemplate>();
            templateRepository = new Mock<IIecValueRepository>();
            factory = new ObjectFactory( templateMock.Object, templateRepository.Object);
        }
        [Fact]
        public void GetInformationObjectTest_GetStepposition_Verify()
        {
            Iec104DataTypes testType = Iec104DataTypes.M_ST_NA_1;
            Iec104DataPoint dp = new Iec104DataPoint(new Domain.ValueTypes.IecAddress(123, 89), testType);
            var item = factory.CreateRandomInformationObject(dp);

            templateMock.Verify(v => v.GetStepposition(89, It.IsAny<IecValueObject>(), testType));
        }
        [Fact]
        public void GetInformationObjectTest_GetDoublePoint_Verify()
        {
            Iec104DataTypes testType = Iec104DataTypes.M_DP_TA_1;
            Iec104DataPoint dp = new Iec104DataPoint(new Domain.ValueTypes.IecAddress(123, 89), testType);
            var item = factory.CreateRandomInformationObject(dp);

            templateMock.Verify(v => v.GetDoublePoint(89, It.IsAny<IecDoublePointValueObject>(), testType));
        }
    }
}
