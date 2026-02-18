using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using System.Collections.Concurrent;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class ProfileProvider : IProfileProvider
    {
        private readonly Dictionary<string, float[]> _profiles;
        private readonly ConcurrentDictionary<IecAddress, int> _currentIndex = new();

        public ProfileProvider(Dictionary<string, float[]> profiles)
        {
            _profiles = profiles;
        }

        public IEnumerable<string> GetProfileNames()
        {
            return _profiles.Keys;
        }

        public bool ProfileExists(string profileName)
        {
            return _profiles.ContainsKey(profileName);
        }

        public float GetNextValue(string profileName, IecAddress address)
        {
            if (!_profiles.TryGetValue(profileName, out var values))
                throw new KeyNotFoundException($"Profile '{profileName}' not found");

            int index = _currentIndex.AddOrUpdate(address, 0, (_, current) => (current + 1) % values.Length);
            return values[index];
        }
    }
}
