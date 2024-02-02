using IEC60870_5_104_simulator.Domain.Service;
using Xunit.Sdk;

namespace IEC60870_5_104_simulator.Domain.UnitTests
{
    public class Iec104ConfigurationServiceTest
    {
        Iec104ConfigurationService service;

        public Iec104ConfigurationServiceTest()
        {
            this.service = new Iec104ConfigurationService();
        }
        [Fact]
        public void ConfigureDataPointsTest_Add_CountNumber()
        {
            List<Iec104CommandDataPointConfig> commandsToAdd = new List<Iec104CommandDataPointConfig>();
            commandsToAdd.Add(new Iec104CommandDataPointConfig(new IecAddress(11,122),Iec104DataTypes.C_CD_NA_1));

            this.service.ConfigureDataPoints(commandsToAdd, new List<Iec104DataPointConfig>());

            Assert.True(service.commandDataPoints.Count == 1);
        }
    }
}