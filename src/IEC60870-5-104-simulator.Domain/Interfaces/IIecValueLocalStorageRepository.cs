using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain.Interfaces
{
    public interface IIecValueLocalStorageRepository
    {
        bool GetSinglePoint(IecAddress address);
        bool SetSinglePoint(IecAddress address, bool value);
        IecDoublePointValue GetDoublePoint(IecAddress address);
        IecDoublePointValue SetDoublePoint(IecAddress address, IecDoublePointValue value);
        int GetStepValue(IecAddress address);
        void SetStepValue(IecAddress address, int value);
    }
}