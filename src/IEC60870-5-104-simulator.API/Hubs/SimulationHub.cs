using Microsoft.AspNetCore.SignalR;

namespace IEC60870_5_104_simulator.API.Hubs
{
    /// <summary>
    /// SignalR hub for real-time SCADA dashboard updates.
    /// Client connects to /hubs/simulation.
    /// </summary>
    public class SimulationHub : Hub
    {
        // All server→client pushes happen via IHubContext<SimulationHub>;
        // client-to-server methods can be added here if bidirectional control is needed.
    }

    // -------------------------------------------------------------------------
    // Server→Client message contracts
    // -------------------------------------------------------------------------

    public record DataPointUpdateDto(
        int StationaryAddress,
        int ObjectAddress,
        string Id,
        string Iec104DataType,
        string Mode,
        object? Value,
        bool Frozen);

    public record ScenarioStateDto(string Name, string Status, int RemainingMs);

    public record SimulationStatusDto(
        double UptimeSeconds,
        string EngineState,
        int ActiveIecClients);

    /// <summary>Pushed after every simulation cycle that enqueued datapoint values for IEC-104 clients.</summary>
    public record CycleSentDto(
        int SentCount,
        int ActiveIecClients,
        DateTime TimestampUtc);
}
