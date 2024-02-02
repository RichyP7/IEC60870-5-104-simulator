using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Service;
using lib60870.CS101;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace IEC60870_5_104_simulator.Infrastructure
{
    public class Iec104Service : IIec104Service
    {
        private lib60870.CS104.Server server;
        private readonly ICommandResponseFactory responseFactory;
        private readonly ILogger<Iec104Service> logger;
        private IIec104ConfigurationService configuration;
        private readonly IValueSimulatorFactory valueFactory;
        private List<InformationObject> objectsToSimulate;

        public IInformationObjectFactory factory { get; }

        public Iec104Service(lib60870.CS104.Server server, IInformationObjectFactory factory, ICommandResponseFactory responseFactory, ILogger<Iec104Service> logger, IIec104ConfigurationService configuration, IValueSimulatorFactory simulatorProfile)
        {
            this.server = server;
            this.factory = factory;
            this.responseFactory = responseFactory;
            this.logger = logger;
            this.configuration = configuration;
            this.valueFactory = simulatorProfile;
            objectsToSimulate = new List<InformationObject>();
        }

        public Task Start()
        {
            SetupIecDataPointList();
            server.SetASDUHandler(AsduSendMirrorAcknowledgements, null);

            this.server.Start();
            return Task.CompletedTask;
        }

        private void SetupIecDataPointList()
        {
            objectsToSimulate.Clear();
            foreach (var datapoint in configuration.DataPoints)
            {
                var infoObject = factory.GetInformationObject(datapoint.Value);
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
        /// <summary>
        /// Handler for Iec receiver
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="connection"></param>
        /// <param name="asdu"></param>
        /// <returns></returns>
        private bool AsduSendMirrorAcknowledgements(object parameter, IMasterConnection connection, ASDU asdu)
        {

            if (IsNonCommandType(asdu))
                return false;
            AcknowledgeAllCommands(asdu);
            List<InformationObject> responses = GetGeneratedResponses(asdu);
            SendGeneratedResponses(responses);
            return true;
        }

        private void AcknowledgeAllCommands(ASDU asdu)
        {
            asdu.Cot = CauseOfTransmission.ACTIVATION_TERMINATION;
            asdu.IsNegative = false;
            server.EnqueueASDU(asdu);
        }

        private List<InformationObject> GetGeneratedResponses(ASDU asdu)
        {
            List<InformationObject> responseInformationObjects = new();
            for (int i = 0; i < asdu.NumberOfElements; i++)
            {
                InformationObject ioa = asdu.GetElement(i);
                IecAddress searchAddress = new IecAddress(asdu.Ca, ioa.ObjectAddress);
                if (this.configuration.CheckCommandExisting(searchAddress))
                {
                    logger.LogDebug($"Command OA:'{ioa.ObjectAddress}' StationCA:{asdu.Ca}  has been configured ");
                    Iec104CommandDataPointConfig commandConfig = this.configuration.GetCommand(searchAddress);
                    responseInformationObjects.Add(responseFactory.GetResponseInformationObject(commandConfig, ioa));
                }
            }
            return responseInformationObjects;
        }

        private bool IsNonCommandType(ASDU asdu)
        {
            return (int)asdu.TypeId < 45 || (int)asdu.TypeId > 107;
        }

        private ASDU CreateAsdu()
        {
            return new ASDU(server.GetApplicationLayerParameters(), CauseOfTransmission.SPONTANEOUS, false, false, 1, 1, false);
        }
        private void SendGeneratedResponses(List<InformationObject> responses)
        {
            if (responses.Count > 0)
            {
                ASDU newAsduWithResponses = CreateAsdu();
                responses.ForEach(v => newAsduWithResponses.AddInformationObject(v));
                server.EnqueueASDU(newAsduWithResponses);
            }
        }
    }
}