using AutoMapper;
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
        private readonly IMapper mapper;
        private readonly IIecValueRepository repo;

        public DataPointValueService(IIec104Service service, IMapper mapper, IIecValueRepository repo)
        {
            this.service = service;
            this.mapper = mapper;
            this.repo = repo;
        }
        public Task SendNewIecValue(Iec104DataPointDto dto)
        {
            var domainObject = mapper.Map<Iec104DataPoint>(dto);
            return service.Simulate(domainObject);
        }
        public Task<Iec104DataPointDto> GetCurrentValue(IecAddress address)
        {
            Iec104DataPoint dpvalue= this.repo.GetDataPointValue(address);
            return Task.FromResult(mapper.Map<Iec104DataPointDto>(dpvalue));
        }
    }
}
