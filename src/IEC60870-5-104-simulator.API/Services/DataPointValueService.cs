using IEC60870_5_104_simulator.API.Mapping;
using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.Service;
using IEC60870_5_104_simulator.Domain.ValueTypes;

namespace IEC60870_5_104_simulator.API.Services
{
    public class DataPointValueService
    {
        private readonly IIec104Service service;
        private readonly IDatapointRegistry registry;

        public DataPointValueService(IIec104Service service, IDatapointRegistry registry)
        {
            this.service = service;
            this.registry = registry;
        }
        public Task SendNewIecValue(Iec104DataPointDto dto)
        {
            var domainObject = dto.ToDomain();
            return service.Simulate(domainObject);
        }
        public Task<Iec104DataPointDto> GetCurrentValue(IecAddress address)
        {
            Iec104DataPoint dpvalue = this.registry.Get(address);
            return Task.FromResult(dpvalue.ToDto());
        }
    }
}
