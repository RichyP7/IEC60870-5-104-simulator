using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using lib60870.CS101;
using lib60870.CS104;
using Microsoft.Extensions.Logging;

namespace IEC60870_5_104_simulator.Infrastructure
{
    internal class Iec104CommandHandler : IIec104CommandHandler
    {
        private readonly lib60870.CS104.Server _server;
        private readonly IASDUDispatcher _dispatcher;
        private readonly IIec104ConfigurationService _configuration;
        private readonly ICommandResponseFactory _responseFactory;
        private readonly IInformationObjectFactory _factory;
        private readonly ILogger<Iec104CommandHandler> _logger;

        public Iec104CommandHandler(
            lib60870.CS104.Server server,
            IASDUDispatcher dispatcher,
            IIec104ConfigurationService configuration,
            ICommandResponseFactory responseFactory,
            IInformationObjectFactory factory,
            ILogger<Iec104CommandHandler> logger)
        {
            _server = server;
            _dispatcher = dispatcher;
            _configuration = configuration;
            _responseFactory = responseFactory;
            _factory = factory;
            _logger = logger;
        }

        public Task Simulate(Iec104DataPoint dataPoint)
        {
            var ioa = _factory.CreateInformationObjectWithValue(dataPoint, dataPoint.Value);
            var asdu = _dispatcher.CreateAsdu(dataPoint.Address.StationaryAddress, CauseOfTransmission.SPONTANEOUS);
            asdu.AddInformationObject(ioa);
            _dispatcher.Send(asdu);
            return Task.CompletedTask;
        }

        public bool HandleAsdu(object parameter, IMasterConnection connection, ASDU asdu)
        {
            try
            {
                if (IsNonCommandType(asdu))
                    return false;
                AcknowledgeConfiguredCommands(asdu);
                List<InformationObject> responses = GetGeneratedResponses(asdu);
                SendGeneratedResponses(responses, asdu.Ca);
                return true;
            }
            catch (KeyNotFoundException kex)
            {
                _logger.LogWarning(kex, "Command processing failed for {Ca}", asdu.Ca);
                asdu.Cot = CauseOfTransmission.UNKNOWN_INFORMATION_OBJECT_ADDRESS;
                asdu.IsNegative = true;
                _server.EnqueueASDU(asdu);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Command processing failed for {Ca}", asdu.Ca);
                return false; // Return false results in a IsNegative message with unknown TypeID
            }
        }

        private void AcknowledgeConfiguredCommands(ASDU asdu)
        {
            for (int i = 0; i < asdu.NumberOfElements; i++)
            {
                GetConfiguration(asdu, i);
            }
            asdu.Cot = CauseOfTransmission.ACTIVATION_CON;
            asdu.IsNegative = false;
            _server.EnqueueASDU(asdu);
        }

        private IecAddress GetConfiguration(ASDU asdu, int i)
        {
            InformationObject ioa = asdu.GetElement(i);
            IecAddress iecAddress = new IecAddress(asdu.Ca, ioa.ObjectAddress);
            if (_configuration.CheckCommandExisting(iecAddress))
                return iecAddress;
            throw new KeyNotFoundException($"Command with CA: {iecAddress.StationaryAddress} and IOA:{iecAddress.ObjectAddress} not found");
        }

        private List<InformationObject> GetGeneratedResponses(ASDU asdu)
        {
            List<InformationObject> responseInformationObjects = new();
            for (int i = 0; i < asdu.NumberOfElements; i++)
            {
                var config = GetConfiguration(asdu, i);
                Iec104CommandDataPointConfig commandConfig = _configuration.GetCommand(config);
                if (commandConfig.SimulatedDataPoint == null)
                    continue;
                InformationObject response = _responseFactory.Update(commandConfig, asdu.GetElement(i));
                responseInformationObjects.Add(response);
            }
            return responseInformationObjects;
        }

        private static bool IsNonCommandType(ASDU asdu)
        {
            return (int)asdu.TypeId < 45 || (int)asdu.TypeId > 107;
        }

        private void SendGeneratedResponses(List<InformationObject> responses, int ca)
        {
            if (responses.Count > 0)
            {
                ASDU newAsduWithResponses = _dispatcher.CreateAsdu(ca, CauseOfTransmission.SPONTANEOUS);
                responses.ForEach(v => newAsduWithResponses.AddInformationObject(v));
                _dispatcher.Send(newAsduWithResponses);
            }
        }
    }
}
