using lib60870.CS101;
using Microsoft.Extensions.Logging;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class Iec104Service : IIec104Service
    {
        private lib60870.CS104.Server server;
        private readonly ILogger<Iec104Service> logger;

        public IInformationObjectFactory factory { get; }

        public Iec104Service(lib60870.CS104.Server server, IInformationObjectFactory factory, ILogger<Iec104Service> logger)
        {
            this.server = server;
            this.factory = factory;
            this.logger = logger;
        }

        public Task Start()
        {
            this.server.Start();
            return Task.CompletedTask;
        }
        public Task Stop()
        {
            this.server.Stop();
            return Task.CompletedTask;
        }

        public Task SimulateValues()
        {
            var infoObject = factory.GetInformationObject("any");        
            ASDU newAsdu = CreateAsdu();
            newAsdu.AddInformationObject(infoObject);
            server.EnqueueASDU(newAsdu);
            logger.LogDebug("Enqeued {asdu} items", newAsdu.NumberOfElements);

            return Task.CompletedTask;
        }

        private ASDU CreateAsdu()
        {
            return new ASDU(server.GetApplicationLayerParameters(), CauseOfTransmission.PERIODIC, false, false, 1, 1, false);
        }
    }
}