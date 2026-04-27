using IEC60870_5_104_simulator.Domain;
using lib60870.CS101;

namespace IEC60870_5_104_simulator.Infrastructure.Interfaces
{
    public interface IASDUDispatcher
    {
        ASDU CreateAsdu(int ca, CauseOfTransmission cot);
        IEnumerable<ASDU> BuildAsdus(IEnumerable<Iec104DataPoint> datapoints, CauseOfTransmission cot);
        void Send(ASDU asdu);
        void Send(IEnumerable<ASDU> asdus);
    }
}
