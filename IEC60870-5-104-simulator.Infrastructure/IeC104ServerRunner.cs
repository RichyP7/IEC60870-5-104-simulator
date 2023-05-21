using lib60870.CS101;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class IeC104ServerRunner : IIeC104ServerRunner
    {
        private lib60870.CS104.Server server;
        public IeC104ServerRunner()
        {
            server = new();
            server.DebugOutput = false;
            server.MaxQueueSize = 100;
            server.SetLocalPort(2404);

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
            SinglePointInformation spi = new SinglePointInformation(1000, true, new QualityDescriptor());
            ASDU newAsdu = CreateAsdu();
            newAsdu.AddInformationObject(spi);
            server.EnqueueASDU(newAsdu);
            return Task.CompletedTask;
        }

        private ASDU CreateAsdu()
        {
            return new ASDU(server.GetApplicationLayerParameters(), CauseOfTransmission.PERIODIC, false, false, 1, 1, false);
        }
    }
}