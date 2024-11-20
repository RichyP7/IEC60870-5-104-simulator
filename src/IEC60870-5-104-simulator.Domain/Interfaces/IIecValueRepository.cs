using System.Collections.Concurrent;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain.Interfaces
{
    public interface IIecValueRepository
    {
        bool GetSinglePoint(IecAddress address);
        void SetSinglePoint(IecAddress address, bool value);
        IecDoublePointValue GetDoublePoint(IecAddress address);
        void SetDoublePoint(IecAddress address, IecDoublePointValue value);
        int GetStepValue(IecAddress address);
        void SetStepValue(IecAddress address, int value);
        void AddDataPoint(IecAddress address, Iec104DataPoint newdatapoint);
        ConcurrentDictionary<IecAddress, Iec104DataPoint> GetAllDataPoints();
        void DeleteDataPoint(IecAddress address);
    }
}