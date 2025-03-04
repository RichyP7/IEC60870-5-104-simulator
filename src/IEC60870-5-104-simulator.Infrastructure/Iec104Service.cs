using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.Service;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using lib60870.CS101;
using lib60870.CS104;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("IEC60870-5-104-simulator.Infrastructure.Tests")]
namespace IEC60870_5_104_simulator.Infrastructure
{
    public class Iec104Service : IIec104Service
    {
        private lib60870.CS104.Server server;
        private readonly ICommandResponseFactory responseFactory;
        private readonly ILogger<Iec104Service> logger;
        private IIec104ConfigurationService configuration;
        private readonly IIecValueRepository repository;
        private bool _connected = false;
        private bool _started = false;


        public IInformationObjectFactory factory { get; }

        public Iec104Service(lib60870.CS104.Server server, IInformationObjectFactory factory, ICommandResponseFactory responseFactory, ILogger<Iec104Service> logger,
            IIec104ConfigurationService configuration, IIecValueRepository repository)
        {
            this.server = server;
            this.factory = factory;
            this.responseFactory = responseFactory;
            this.logger = logger;
            this.configuration = configuration;
            this.repository = repository;
        }

        public async Task Start()
        {
            SendInitialize();
            server.SetASDUHandler(AsduSendMirrorAcknowledgements, null);
            server.SetConnectionEventHandler(handler, null);
            server.SetInterrogationHandler(handlerinterrogation, null);
            server.SetConnectionRequestHandler(requesthandler, null);
            this.server.Start();
            this._started = true;
        }

        private bool requesthandler(object parameter, IPAddress ipAddress)
        {
            logger.LogInformation("Request event");
            return true;
        }

        private bool handlerinterrogation(object parameter, IMasterConnection connection, ASDU asdu, byte qoi)
        {
            logger.LogInformation("Interrogation event");
            return true;
        }

        private void SendInitialize()
        {
            ASDU newAsdu = new ASDU(server.GetApplicationLayerParameters(), CauseOfTransmission.INITIALIZED, false, false, 0, 1, false);
            EndOfInitialization eoi = new EndOfInitialization(0);
            newAsdu.AddInformationObject(eoi);
            server.EnqueueASDU(newAsdu);
        }

        private void handler(object parameter, ClientConnection connection, ClientConnectionEvent eventType)
        {
            logger.LogInformation("connection event ({type}): {adress}", eventType.ToString(), connection.RemoteEndpoint.Address.ToString());
            if (eventType == ClientConnectionEvent.OPENED)
            {
                _connected = true;
            }
            else if (eventType == ClientConnectionEvent.ACTIVE)
            {
                if (false)//_iecOptions.InterrogationOnInitailize)
                {
                    //SendInterrogationCommand(connection);
                }
            }
            else if (eventType == ClientConnectionEvent.CLOSED)
            {
                _connected = false;
            }

        }

        public Task Stop()
        {
            this.server.Stop();
            return Task.CompletedTask;
        }

        public Task Simulate(IEnumerable<Iec104DataPoint> datapoints)
        {
            if (this._connected)
            {
                SimulateValues(datapoints);
            }
            return Task.CompletedTask;
        }

        internal void SimulateValues(IEnumerable<Iec104DataPoint> datapoints)
        {
            var cyclicDataPoints = GetCyclicDataPoints(datapoints);
            IEnumerable<KeyValuePair<Iec104DataTypes, ASDU>> asdus = CreateDistinctAsdus(cyclicDataPoints);
            foreach (Iec104DataPoint dataPoint in cyclicDataPoints)
            {
                var ioa = factory.GetInformationObject(dataPoint);
                var myASDU = asdus.First(v => v.Key.Equals(dataPoint.Iec104DataType) && v.Value.Ca.Equals(dataPoint.Address.StationaryAddress));
                myASDU.Value.AddInformationObject(ioa);
            }
            Send(asdus.Select(v => v.Value));
            
            // Simulate Static Values
            var staticDataPoints = GetCyclicStaticDataPoints(datapoints);
            IEnumerable<KeyValuePair<Iec104DataTypes, ASDU>> staticAsdus = CreateDistinctAsdus(staticDataPoints);
            foreach (Iec104DataPoint dataPoint in staticDataPoints)
            {
                var ioa = factory.GetInformationObjectWithStaticValue(dataPoint);
                var myASDU = staticAsdus.First(v => v.Key.Equals(dataPoint.Iec104DataType) && v.Value.Ca.Equals(dataPoint.Address.StationaryAddress));
                myASDU.Value.AddInformationObject(ioa);
            }
            Send(staticAsdus.Select(v => v.Value));
        }

        private static IEnumerable<Iec104DataPoint> GetCyclicDataPoints(IEnumerable<Iec104DataPoint> datapoints)
        {
            return datapoints.Where(v => v.Mode.Equals(SimulationMode.Cyclic));
        }
        
        private static IEnumerable<Iec104DataPoint> GetCyclicStaticDataPoints(IEnumerable<Iec104DataPoint> datapoints)
        {
            return datapoints.Where(v => v.Mode.Equals(SimulationMode.CyclicStatic));
        }

        private IEnumerable<KeyValuePair<Iec104DataTypes, ASDU>> CreateDistinctAsdus(IEnumerable<Iec104DataPoint> datapoints)
        {
            List<KeyValuePair<Iec104DataTypes, ASDU>> asdusPerTypeandCa = new();
            foreach (var groupByStationAndType in
                    datapoints
                    .GroupBy(x => new { x.Address?.StationaryAddress, x.Iec104DataType })
                    .Select(g => new { station = g.First().Address.StationaryAddress, iectype = g.First().Iec104DataType }))
            {
                ASDU newAsdu = CreateAsdu(groupByStationAndType.station, CauseOfTransmission.PERIODIC);
                asdusPerTypeandCa.Add(new KeyValuePair<Iec104DataTypes, ASDU>(groupByStationAndType.iectype, newAsdu));
            }
            return asdusPerTypeandCa;
        }
        private void Send(IEnumerable<ASDU> asdus)
        {
            foreach (var toSend in from ASDU toSend in asdus
                                   where toSend.NumberOfElements > 0
                                   select toSend)
            {
                server.EnqueueASDU(toSend);
                logger.LogInformation("Enqueued {Asdu} items on station {Ca}", toSend.NumberOfElements, toSend.Ca);
            }
        }


        /// <summary>
        /// Send ack and response message from the same stationary address
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="connection"></param>
        /// <param name="asdu"></param>
        /// <returns></returns>
        private bool AsduSendMirrorAcknowledgements(object parameter, IMasterConnection connection, ASDU asdu)
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
                logger.LogWarning(kex, "Command processing failed for {Ca}", asdu.Ca);
                asdu.Cot = CauseOfTransmission.UNKNOWN_INFORMATION_OBJECT_ADDRESS;
                asdu.IsNegative = true;
                server.EnqueueASDU(asdu);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Command processing failed for {Ca}", asdu.Ca);
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
            server.EnqueueASDU(asdu);
        }

        private IecAddress GetConfiguration(ASDU asdu, int i)
        {
            InformationObject ioa = asdu.GetElement(i);
            IecAddress iecAddress = new IecAddress(asdu.Ca, ioa.ObjectAddress);
            if (this.configuration.CheckCommandExisting(iecAddress))
                return iecAddress;
            throw new KeyNotFoundException($"Command with CA: {iecAddress.StationaryAddress} and IOA:{iecAddress.ObjectAddress} not found");
        }

        private List<InformationObject> GetGeneratedResponses(ASDU asdu)
        {
            List<InformationObject> responseInformationObjects = new();
            for (int i = 0; i < asdu.NumberOfElements; i++)
            {
                var config = GetConfiguration(asdu, i);
                Iec104CommandDataPointConfig commandConfig = this.configuration.GetCommand(config);
                if (commandConfig.SimulatedDataPoint == null)
                    continue;
                InformationObject response = responseFactory.Update(commandConfig, asdu.GetElement(i));
                responseInformationObjects.Add(response);
            }
            return responseInformationObjects;
        }

        private bool IsNonCommandType(ASDU asdu)
        {
            return (int)asdu.TypeId < 45 || (int)asdu.TypeId > 107;
        }

        private ASDU CreateAsdu(int ca, CauseOfTransmission cot)
        {
            return new ASDU(server.GetApplicationLayerParameters(), cot, false, false, 1, ca, false);
        }
        private void SendGeneratedResponses(List<InformationObject> responses, int ca)
        {
            if (responses.Count > 0)
            {
                ASDU newAsduWithResponses = CreateAsdu(ca, CauseOfTransmission.SPONTANEOUS);
                responses.ForEach(v => newAsduWithResponses.AddInformationObject(v));
                server.EnqueueASDU(newAsduWithResponses);
            }
        }

        public bool ConnectionEstablished()
        {
            return this._connected;
        }
    }
}