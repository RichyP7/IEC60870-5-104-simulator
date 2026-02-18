using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.Domain.Interfaces
{
    public interface IProfileProvider
    {
        float GetNextValue(string profileName, IecAddress address);
        bool ProfileExists(string profileName);
        IEnumerable<string> GetProfileNames();
    }
}
