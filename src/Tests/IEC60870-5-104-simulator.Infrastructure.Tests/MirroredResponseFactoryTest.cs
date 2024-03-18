using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using lib60870.CS101;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            factory.GetResponseInformationObject(cmd, new Mock<StepCommand>(10,StepCommandValue.HIGHER,true,1).Object);

            templateMock.Verify(v => v.GetStepposition(89, It.IsAny<IecValueObject>(), Iec104DataTypes.M_ST_NA_1));
        }
    }
}
