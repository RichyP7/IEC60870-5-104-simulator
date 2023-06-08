using IEC60870_5_104_simulator.Domain;
using lib60870.CS101;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class Iec104Service : IIec104Service
    {
        private lib60870.CS104.Server server;
        private readonly ILogger<Iec104Service> logger;
        private Iec104DataPointConfiguration configuration;
        private List<InformationObject> objectsToSimulate;

        public IInformationObjectFactory factory { get; }

        public Iec104Service(lib60870.CS104.Server server, IInformationObjectFactory factory, ILogger<Iec104Service> logger, Iec104DataPointConfiguration configuration)
        {
            this.server = server;
            this.factory = factory;
            this.logger = logger;
            this.configuration = configuration;
            objectsToSimulate = new List<InformationObject>();
        }

        public Task Start(Iec104DataPointConfiguration configuration)
        {
            this.configuration = configuration;
            SetupIecDataPointList(this.configuration);
            this.server.Start();
            return Task.CompletedTask;
        }

        private void SetupIecDataPointList(Iec104DataPointConfiguration configuration)
        {
            objectsToSimulate.Clear();
            foreach(var datapoint in configuration.GetDataPointList())
            {
                var infoObject = factory.GetInformationObject(datapoint);
                objectsToSimulate.Add(infoObject);
            }
            if(this.objectsToSimulate.Count ==0)
            {
                throw new InvalidOperationException("Empty configuration list provided");
            }
        }

        public Task Stop()
        {
            this.server.Stop();
            return Task.CompletedTask;
        }

        public Task SimulateValues()
        {
            ASDU newAsdu = CreateAsdu();
            foreach (var iecConfigObject in this.configuration.GetDataPointList())
            {
                InformationObject iOa= factory.GetInformationObject(iecConfigObject);
                newAsdu.AddInformationObject(iOa);
                server.EnqueueASDU(newAsdu);
            }
            logger.LogDebug("Enqeued {asdu} items", newAsdu.NumberOfElements);
            return Task.CompletedTask;
        }

        private ASDU CreateAsdu()
        {
            return new ASDU(server.GetApplicationLayerParameters(), CauseOfTransmission.PERIODIC, false, false, 1, 1, false);
        }
    }
}