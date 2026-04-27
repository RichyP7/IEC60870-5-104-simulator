using AutoMapper;
using IEC60870_5_104_simulator.API.Hubs;
using IEC60870_5_104_simulator.API.Mapping;
using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace IEC60870_5_104_simulator.API.Services
{
    /// <summary>
    /// Publishes scenario state events to connected SignalR clients via SimulationHub.
    /// Lives in the API layer so Infrastructure stays free of ASP.NET Core dependencies.
    /// </summary>
    public class SignalRScenarioEventPublisher : IScenarioEventPublisher
    {
        private readonly IHubContext<SimulationHub> _hub;
        private readonly IMapper _mapper;

        public SignalRScenarioEventPublisher(IHubContext<SimulationHub> hub, IMapper mapper)
        {
            _hub = hub;
            _mapper = mapper;
        }

        public Task PublishScenarioUpdate(ScenarioState state)
        {
            var dto = new ScenarioStateDto(state.Name, state.Status.ToString(), state.RemainingMs);
            return _hub.Clients.All.SendAsync("ScenarioUpdate", dto);
        }

        public Task PublishDataPointChanged(Iec104DataPoint dataPoint)
        {
            var valueDto = dataPoint.Value != null ? _mapper.Map<IecValueDto>(dataPoint.Value) : null;
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
