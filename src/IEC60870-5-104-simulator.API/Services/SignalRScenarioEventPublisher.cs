using IEC60870_5_104_simulator.API.Hubs;
using IEC60870_5_104_simulator.API.Mapping;
using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace IEC60870_5_104_simulator.API.Services
{
    /// <summary>
    /// Publishes scenario state events to connected SignalR clients via SimulationHub.
    /// </summary>
    public class SignalRScenarioEventPublisher : IScenarioEventPublisher
    {
        private readonly IHubContext<SimulationHub> _hub;

        public SignalRScenarioEventPublisher(IHubContext<SimulationHub> hub)
        {
            _hub = hub;
        }

        public Task PublishScenarioUpdate(ScenarioState state)
        {
            var dto = new ScenarioStateDto(state.Name, state.Status.ToString(), state.RemainingMs);
            return _hub.Clients.All.SendAsync("ScenarioUpdate", dto);
        }

        public Task PublishDataPointChanged(Iec104DataPoint dataPoint)
        {
            var valueDto = dataPoint.Value != null ? dataPoint.Value.ToDto() : null;
            var dto = new DataPointUpdateDto(
                dataPoint.Address.StationaryAddress,
                dataPoint.Address.ObjectAddress,
                dataPoint.Id ?? string.Empty,
                dataPoint.Iec104DataType.ToString(),
                dataPoint.Mode.ToString(),
                valueDto,
                dataPoint.Frozen);
            return _hub.Clients.All.SendAsync("DataPointChanged", dto);
        }
    }
}
