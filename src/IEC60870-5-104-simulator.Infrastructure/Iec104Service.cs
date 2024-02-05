﻿using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Service;
using lib60870.CS101;
using lib60870.CS104;
using Microsoft.Extensions.Logging;
using System.Net;
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
        private bool _connected = false;

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

        public async Task Start()
        {
            SendInitialize();
            SetupIecDataPointList();
            server.SetASDUHandler(AsduSendMirrorAcknowledgements, null);
            server.SetConnectionEventHandler(handler, null);
            server.SetInterrogationHandler(handlerinterrogation, null);
            server.SetConnectionRequestHandler(requesthandler, null);


            this.server.Start();
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
            int FIXEDca = 1;
            if (this._connected)
            {
                valueFactory.SimulateValues(this.objectsToSimulate);
                ASDU newAsdu = CreateAsdu(FIXEDca);
                foreach (InformationObject typeddataPoints in objectsToSimulate)
                {
                    newAsdu.AddInformationObject(typeddataPoints);
                }
                if (newAsdu.NumberOfElements > 1)
                {
                    server.EnqueueASDU(newAsdu);
                    logger.LogDebug("Enqeued {asdu} items", newAsdu.NumberOfElements);
                }

            }
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

        private ASDU CreateAsdu(int ca)
        {
            return new ASDU(server.GetApplicationLayerParameters(), CauseOfTransmission.PERIODIC, false, false, 1, ca, false);
        }
        private void SendGeneratedResponses(List<InformationObject> responses)
        {
            int ca = 1;
            if (responses.Count > 0)
            {
                ASDU newAsduWithResponses = CreateAsdu(ca);
                responses.ForEach(v => newAsduWithResponses.AddInformationObject(v));
                server.EnqueueASDU(newAsduWithResponses);
            }
        }
    }
}