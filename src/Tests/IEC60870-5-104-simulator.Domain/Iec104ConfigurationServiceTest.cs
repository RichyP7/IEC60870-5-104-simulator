using IEC60870_5_104_simulator.Domain.Service;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain.UnitTests
{
    public class Iec104ConfigurationServiceTest
    {
        [Fact]
        public void ConfigureDataPoints_AddsCommand()
        {
            var service = new Iec104ConfigurationService();
            var commands = new List<Iec104CommandDataPointConfig>
            {
                new(new IecAddress(11, 122), Iec104DataTypes.C_CD_NA_1)
            };

            service.ConfigureDataPoints(commands);

            Assert.Single(service.CommandDataPoints);
        }

        [Fact]
        public void ConfigureDataPoints_RejectsDuplicateCommandAddress()
        {
            var service = new Iec104ConfigurationService();
            var commands = new List<Iec104CommandDataPointConfig>
            {
                new(new IecAddress(11, 122), Iec104DataTypes.C_CD_NA_1),
                new(new IecAddress(11, 122), Iec104DataTypes.C_CD_NA_1)
            };

            Assert.Throws<ArgumentException>(() => service.ConfigureDataPoints(commands));
        }

        [Fact]
        public void GetCommand_ThrowsForUnknownAddress()
        {
            var service = new Iec104ConfigurationService();

            Assert.Throws<KeyNotFoundException>(() => service.GetCommand(new IecAddress(11, 122)));
        }
    }
}
