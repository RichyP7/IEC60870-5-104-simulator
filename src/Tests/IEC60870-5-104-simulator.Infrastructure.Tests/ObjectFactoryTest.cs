using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Service;
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
        private DatapointRegistry registry;

        public ObjectFactoryTest()
        {
            templateMock = new Mock<IInformationObjectTemplate>();
            registry = new DatapointRegistry();
            factory = new ObjectFactory(templateMock.Object, registry);
        }

        private Iec104DataPoint Register(Iec104DataTypes type)
        {
            var datapoint = new Iec104DataPoint(new IecAddress(123, 89), type);
            registry.Add(datapoint);
            return datapoint;
        }

        [Fact]
        public void GetInformationObjectTest_GetStepposition_Verify()
        {
            Iec104DataTypes testType = Iec104DataTypes.M_ST_NA_1;
            factory.CreateRandomInformationObject(Register(testType));

            templateMock.Verify(v => v.GetStepposition(89, It.IsAny<IecValueObject>(), testType));
        }

        [Fact]
        public void GetInformationObjectTest_GetDoublePoint_Verify()
        {
            Iec104DataTypes testType = Iec104DataTypes.M_DP_TA_1;
            factory.CreateRandomInformationObject(Register(testType));

            templateMock.Verify(v => v.GetDoublePoint(89, It.IsAny<IecDoublePointValueObject>(), testType));
        }
    }
}
