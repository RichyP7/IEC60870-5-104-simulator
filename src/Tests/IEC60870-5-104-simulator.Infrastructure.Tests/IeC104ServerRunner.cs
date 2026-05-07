using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IEC60870_5_104_simulator.Infrastructure.Tests
{
    public class IeC104ServerRunnerTest
    {
        private Mock<IASDUDispatcher> mockDispatcher;
        private Mock<IIec104CommandHandler> mockCommandHandler;
        private Mock<ICyclicSimulationService> mockCyclicSimulation;
        private Mock<IIecValueRepository> storageMock;
        lib60870.CS104.Server testServer;
        private Iec104Service service;

        public IeC104ServerRunnerTest()
        {
            mockDispatcher = new();
            mockCommandHandler = new();
            mockCyclicSimulation = new();
            storageMock = new();
            testServer = new lib60870.CS104.Server();
            service = new Iec104Service(
                testServer,
                storageMock.Object,
                mockDispatcher.Object,
                mockCommandHandler.Object,
                mockCyclicSimulation.Object,
                NullLogger<Iec104Service>.Instance);
        }
    }
}