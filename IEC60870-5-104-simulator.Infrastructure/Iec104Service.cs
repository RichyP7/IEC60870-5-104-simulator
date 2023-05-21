using lib60870.CS101;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class Iec104Service : IIec104Service
    {
        private lib60870.CS104.Server server;

        public IInformationObjectFactory factory { get; }

        public Iec104Service(lib60870.CS104.Server server, IInformationObjectFactory factory)
        {
            this.server = server;
            this.factory = factory;
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
            return Task.CompletedTask;
        }

        private ASDU CreateAsdu()
        {
            return new ASDU(server.GetApplicationLayerParameters(), CauseOfTransmission.PERIODIC, false, false, 1, 1, false);
        }
    }
}