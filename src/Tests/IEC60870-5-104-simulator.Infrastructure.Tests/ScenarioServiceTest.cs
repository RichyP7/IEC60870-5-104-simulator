using IEC60870_5_104_simulator.Domain;
using IEC60870_5_104_simulator.Domain.Interfaces;
using IEC60870_5_104_simulator.Domain.Service;
using IEC60870_5_104_simulator.Domain.ValueTypes;
using Microsoft.Extensions.Logging;
using Moq;

namespace IEC60870_5_104_simulator.Infrastructure.Tests;

public class ScenarioServiceTest
{
    private readonly Mock<IIecValueRepository> _repoMock = new();
    private readonly Mock<IIec104Service> _iecMock = new();
    private readonly Mock<IScenarioEventPublisher> _publisherMock = new();
    private readonly Mock<ILogger<ScenarioService>> _loggerMock = new();

    private ScenarioService CreateService(IReadOnlyList<ScenarioDefinition> definitions)
        => new(definitions, _repoMock.Object, _iecMock.Object, _publisherMock.Object, _loggerMock.Object);

    private static Iec104DataPoint MakeDoublePoint(int ca, int oa)
        => new(new IecAddress(ca, oa), Iec104DataTypes.M_DP_NA_1)
        {
            Value = new IecDoublePointValueObject(IecDoublePointValue.ON)
        };

    // -----------------------------------------------------------------------
    // Scenario not found
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TriggerAsync_UnknownScenario_ReturnsFalse()
    {
        var svc = CreateService(Array.Empty<ScenarioDefinition>());
        bool result = await svc.TriggerAsync("does-not-exist");
        Assert.False(result);
    }

    // -----------------------------------------------------------------------
    // Happy path: steps applied in order, recovery restores state
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TriggerAsync_ValidScenario_AppliesStepsAndRecovery()
    {
        var address = new IecAddress(1, 101);
        _repoMock.Setup(r => r.GetDataPointValue(address)).Returns(MakeDoublePoint(1, 101));
        _iecMock.Setup(s => s.Simulate(It.IsAny<Iec104DataPoint>())).Returns(Task.CompletedTask);
        _publisherMock.Setup(p => p.PublishScenarioUpdate(It.IsAny<ScenarioState>())).Returns(Task.CompletedTask);

        var definition = new ScenarioDefinition(
            Name: "test-scenario",
            RecoveryMs: 0,
            Steps: new[]
            {
                new ScenarioStep(0, 1, 101, "OFF", Freeze: true, "Trip breaker"),
            }.AsReadOnly(),
            RecoverySteps: new[]
            {
                new ScenarioStep(0, 1, 101, "ON", Freeze: false, "Restore breaker"),
            }.AsReadOnly());

        var svc = CreateService(new[] { definition });
        bool triggered = await svc.TriggerAsync("test-scenario");
        Assert.True(triggered);

        // Give the background task time to complete
        await Task.Delay(200);

        // Freeze was called during fault step, unfreeze during recovery
        _repoMock.Verify(r => r.FreezeDataPoint(address), Times.Once);
        _repoMock.Verify(r => r.UnfreezeDataPoint(address), Times.Once);

        // SetObjectValue called twice: fault + recovery
        _repoMock.Verify(r => r.SetObjectValue(address, It.IsAny<IecValueObject>()), Times.Exactly(2));

        // ScenarioUpdate published: Running (start) + Running (after step) + Completed
        _publisherMock.Verify(p => p.PublishScenarioUpdate(It.IsAny<ScenarioState>()), Times.AtLeast(2));
    }

    // -----------------------------------------------------------------------
    // Concurrency guard: second trigger while running is rejected
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TriggerAsync_AlreadyRunning_ReturnsFalse()
    {
        var address = new IecAddress(1, 101);
        _repoMock.Setup(r => r.GetDataPointValue(address)).Returns(MakeDoublePoint(1, 101));
        _iecMock.Setup(s => s.Simulate(It.IsAny<Iec104DataPoint>())).Returns(Task.CompletedTask);
        _publisherMock.Setup(p => p.PublishScenarioUpdate(It.IsAny<ScenarioState>())).Returns(Task.CompletedTask);

        var definition = new ScenarioDefinition(
            Name: "long-scenario",
            RecoveryMs: 500,
            Steps: new[]
            {
                new ScenarioStep(300, 1, 101, "OFF", true, "Slow step"),
            }.AsReadOnly(),
            RecoverySteps: Array.Empty<ScenarioStep>().AsReadOnly());

        var svc = CreateService(new[] { definition });

        // First trigger starts the scenario
        bool first = await svc.TriggerAsync("long-scenario");
        Assert.True(first);

        // Second trigger while it is running should be rejected
        bool second = await svc.TriggerAsync("long-scenario");
        Assert.False(second);

        // Wait for cleanup
        await Task.Delay(1200);
    }

    // -----------------------------------------------------------------------
    // State transitions
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetStates_InitialState_IsIdle()
    {
        var definition = new ScenarioDefinition("idle-test", 0,
            Array.Empty<ScenarioStep>().AsReadOnly(),
            Array.Empty<ScenarioStep>().AsReadOnly());

        var svc = CreateService(new[] { definition });
        var state = svc.GetStates().Single();
        Assert.Equal("idle-test", state.Name);
        Assert.Equal(ScenarioStatus.Idle, state.Status);
    }

    [Fact]
    public async Task TriggerAsync_AfterCompletion_StateIsCompleted()
    {
        _publisherMock.Setup(p => p.PublishScenarioUpdate(It.IsAny<ScenarioState>())).Returns(Task.CompletedTask);

        var definition = new ScenarioDefinition("quick", 0,
            Array.Empty<ScenarioStep>().AsReadOnly(),
            Array.Empty<ScenarioStep>().AsReadOnly());

        var svc = CreateService(new[] { definition });
        await svc.TriggerAsync("quick");
        await Task.Delay(300);

        var state = svc.GetStates().Single();
        Assert.Equal(ScenarioStatus.Completed, state.Status);
    }
}
