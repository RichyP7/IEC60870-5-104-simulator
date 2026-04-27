using IEC60870_5_104_simulator.Domain;
using lib60870.CS101;
using lib60870.CS104;

namespace IEC60870_5_104_simulator.Infrastructure.Interfaces
{
    public interface IIec104CommandHandler
    {
        bool HandleAsdu(object parameter, IMasterConnection connection, ASDU asdu);
        Task Simulate(Iec104DataPoint dataPoint);
    }
}
