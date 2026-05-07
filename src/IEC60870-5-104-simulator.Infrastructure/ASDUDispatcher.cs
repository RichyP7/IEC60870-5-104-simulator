using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using lib60870.CS101;
using lib60870.CS104;
using Microsoft.Extensions.Logging;

namespace IEC60870_5_104_simulator.Infrastructure
{
    internal class ASDUDispatcher : IASDUDispatcher
    {
        private readonly lib60870.CS104.Server _server;
        private readonly IInformationObjectFactory _factory;
        private readonly IIecValueRepository _repository;
        private readonly ILogger<ASDUDispatcher> _logger;

        private const int MaxInformationObjectsPerASDU = 20;

        public ASDUDispatcher(
            lib60870.CS104.Server server,
            IInformationObjectFactory factory,
            IIecValueRepository repository,
            ILogger<ASDUDispatcher> logger)
        {
            _server = server;
            _factory = factory;
            _repository = repository;
            _logger = logger;
        }

        public ASDU CreateAsdu(int ca, CauseOfTransmission cot)
        {
            return new ASDU(_server.GetApplicationLayerParameters(), cot, false, false, 1, ca, false);
        }

        public IEnumerable<ASDU> BuildAsdus(IEnumerable<Iec104DataPoint> datapoints, CauseOfTransmission cot)
        {
            var groups = datapoints
                .GroupBy(dp => new { dp.Address.StationaryAddress, dp.Iec104DataType })
                .ToList();

            var result = new List<ASDU>();
            foreach (var group in groups)
            {
                ASDU asdu = CreateAsdu(group.Key.StationaryAddress, cot);
                foreach (var dp in group)
                {
                    if (asdu.NumberOfElements >= MaxInformationObjectsPerASDU)
                    {
                        result.Add(asdu);
                        asdu = CreateAsdu(group.Key.StationaryAddress, cot);
                    }
                    asdu.AddInformationObject(
                        _factory.CreateInformationObjectWithValue(dp, _repository.GetDataPointValue(dp.Address).Value));
                }
                result.Add(asdu);
            }
            return result;
        }

        public void Send(IEnumerable<ASDU> asdus)
        {
            _logger.LogInformation("Number of elements to send: {number}", asdus.Sum(v => v.NumberOfElements));
            foreach (var toSend in asdus.Where(a => a.NumberOfElements > 0))
            {
                Send(toSend);
            }
        }

        public void Send(ASDU toSend)
        {
            _server.EnqueueASDU(toSend);
            _logger.LogInformation("Enqueued {Asdu} items on station {Ca}", toSend.NumberOfElements, toSend.Ca);
        }
    }
}
