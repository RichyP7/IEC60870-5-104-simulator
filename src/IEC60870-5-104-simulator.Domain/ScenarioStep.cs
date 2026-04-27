namespace IEC60870_5_104_simulator.Domain
{
    /// <summary>
    /// A single step in a fault scenario timeline.
    /// ValueStr is parsed at runtime using the same rules as InitValue (e.g. "true", "OFF", "110.5").
    /// </summary>
    public record ScenarioStep(
        int DelayMs,
        int Ca,
        int Oa,
        string ValueStr,
        bool Freeze,
        string Description);
}
