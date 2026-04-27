using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.Service;
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
        private readonly lib60870.CS104.Server _server;
        private readonly IIecValueRepository _repository;
        private readonly IASDUDispatcher _dispatcher;
        private readonly IIec104CommandHandler _commandHandler;
        private readonly ICyclicSimulationService _cyclicSimulation;
        private readonly ILogger<Iec104Service> _logger;

        private bool _connected = false;
        private int _activeClientCount = 0;

        public Iec104Service(
            lib60870.CS104.Server server,
            IIecValueRepository repository,
            IASDUDispatcher dispatcher,
            IIec104CommandHandler commandHandler,
            ICyclicSimulationService cyclicSimulation,
            ILogger<Iec104Service> logger)
        {
            _server = server;
            _repository = repository;
            _dispatcher = dispatcher;
            _commandHandler = commandHandler;
            _cyclicSimulation = cyclicSimulation;
            _logger = logger;
        }

        public Task Start()
        {
            SendInitialize();
            _server.SetASDUHandler(_commandHandler.HandleAsdu, null);
            _server.SetConnectionEventHandler(OnConnectionEvent, null);
            _server.SetInterrogationHandler(OnInterrogation, null);
            _server.SetConnectionRequestHandler(OnConnectionRequest, null);
            _server.Start();
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            _server.Stop();
            return Task.CompletedTask;
        }

        public Task Simulate(Iec104DataPoint dataPoint)
        {
            return _commandHandler.Simulate(dataPoint);
        }

        public Task SimulateCyclic(IEnumerable<Iec104DataPoint> datapoints, int cycleTimeMs)
        {
            _cyclicSimulation.SimulateCyclicValues(datapoints, cycleTimeMs);
            return Task.CompletedTask;
        }

        public bool ConnectionEstablished() => _connected;

        public int GetActiveClientCount() => _activeClientCount;

        private void SendInitialize()
        {
            ASDU newAsdu = new ASDU(_server.GetApplicationLayerParameters(), CauseOfTransmission.INITIALIZED, false, false, 0, 1, false);
            newAsdu.AddInformationObject(new EndOfInitialization(0));
            _server.EnqueueASDU(newAsdu);
        }

        private bool OnConnectionRequest(object parameter, IPAddress ipAddress)
        {
            _logger.LogInformation("Request event {ipaddress}", ipAddress.ToString());
            return true;
        }

        private bool OnInterrogation(object parameter, IMasterConnection connection, ASDU asdu, byte qoi)
        {
            _logger.LogInformation("Interrogation event (QOI={QOI})", qoi);

            var respondPoints = _repository.GetAllDataPoints()
                .Where(dp => dp.Mode == SimulationMode.Static || dp.Mode == SimulationMode.CounterOnDemand)
                .ToList();

            if (respondPoints.Count > 0)
            {
                var asdus = _dispatcher.BuildAsdus(respondPoints, CauseOfTransmission.INTERROGATED_BY_STATION);
                _dispatcher.Send(asdus);
            }

            asdu.Cot = CauseOfTransmission.ACTIVATION_TERMINATION;
            connection.SendASDU(asdu);
            return true;
        }

        private void OnConnectionEvent(object parameter, ClientConnection connection, ClientConnectionEvent eventType)
        {
            _logger.LogInformation("connection event ({type}): {adress}", eventType.ToString(), connection.RemoteEndpoint.Address.ToString());
            if (eventType == ClientConnectionEvent.OPENED)
            {
                _connected = true;
                Interlocked.Increment(ref _activeClientCount);
            }
            else if (eventType == ClientConnectionEvent.CLOSED)
            {
                int newCount = Interlocked.Decrement(ref _activeClientCount);
                _connected = newCount > 0;
            }
        }
    }
}