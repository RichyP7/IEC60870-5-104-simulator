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
        private readonly IValueSimulatorFactory valueFactory;
        private List<InformationObject> objectsToSimulate;

        public IInformationObjectFactory factory { get; }

        public Iec104Service(lib60870.CS104.Server server, IInformationObjectFactory factory, ILogger<Iec104Service> logger, Iec104DataPointConfiguration configuration, IValueSimulatorFactory simulatorProfile)
        {
            this.server = server;
            this.factory = factory;
            this.logger = logger;
            this.configuration = configuration;
            this.valueFactory = simulatorProfile;
            objectsToSimulate = new List<InformationObject>();
        }

        public Task Start(Iec104DataPointConfiguration configuration)
        {
            this.configuration = configuration;
            SetupIecDataPointList(this.configuration);
            server.SetASDUHandler(asduHandler, null);

            this.server.Start();
            return Task.CompletedTask;
        }

        private void SetupIecDataPointList(Iec104DataPointConfiguration configuration)
        {
            objectsToSimulate.Clear();
            foreach (var datapoint in configuration.GetDataPointList())
            {
                var infoObject = factory.GetInformationObject(datapoint);
                objectsToSimulate.Add(infoObject);
            }
            if (this.objectsToSimulate.Count == 0)
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
            valueFactory.SimulateValues(this.objectsToSimulate);
            ASDU newAsdu = CreateAsdu();
            foreach (InformationObject typeddataPoints in objectsToSimulate.Where(v => v.Type.Equals(TypeID.M_SP_NA_1)))
            {
                newAsdu.AddInformationObject(typeddataPoints);
            }
            server.EnqueueASDU(newAsdu);
            logger.LogDebug("Enqeued {asdu} items", newAsdu.NumberOfElements);
            return Task.CompletedTask;
        }
        private bool asduHandler(object parameter, IMasterConnection connection, ASDU asdu)
        {
            if (asdu.TypeId == TypeID.C_RC_NA_1)
            {
                logger.LogDebug("StepCommandd");
                StepCommand sc = (StepCommand)asdu.GetElement(0);
                ASDU newAsdu = CreateAsdu();
                asdu.Cot = CauseOfTransmission.ACTIVATION_TERMINATION;
                asdu.IsNegative = false;
                asdu.AddInformationObject(sc);
                server.EnqueueASDU(asdu);

                return true;
            }
            return false;
        }

        private ASDU CreateAsdu()
        {
            return new ASDU(server.GetApplicationLayerParameters(), CauseOfTransmission.PERIODIC, false, false, 1, 1, false);
        }
    }
}