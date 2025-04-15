using AutoMapper;
using IEC60870_5_104_simulator.API.Mapping;
using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Service;

namespace IEC60870_5_104_simulator.API.Services
{
    public class DataPointValueService
    {
        private readonly IIec104Service service;
        private readonly IMapper mapper;

        public DataPointValueService(IIec104Service service, IMapper mapper)
        {
            this.service = service;
            this.mapper = mapper;
        }
        public Task SendNewIecValue(Iec104DataPointDto dto)
        {
            var domainObject = mapper.Map<Iec104DataPoint>(dto);
            return service.Simulate(domainObject);
        }
    }
}
