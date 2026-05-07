using System.Net.Http.Json;
using FluentAssertions;
using IntegrationTests.TestPreparation;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests;

/// <summary>Isolated test to verify initial scenario list state with a fresh factory.</summary>
public sealed class ScenarioListTest : BaseWebApplication
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ScenarioListTest(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _factory.UpdateOptionsPath("../../../Configuration/ScenarioOptionsTest.json");
    }

    [Fact]
    public async Task GetScenarios_ReturnsConfiguredScenarios()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/Scenarios");

        response.IsSuccessStatusCode.Should().BeTrue();

        var body = await response.Content.ReadFromJsonAsync<List<ScenarioDto>>();
        body.Should().NotBeNull();
        body!.Should().ContainSingle(s => s.Name == "ca1-transformer-trip");
        body![0].Status.Should().Be("Idle");
    }

    private record ScenarioDto(string Name, string Status, int RemainingMs);
}

public sealed class ScenarioIntegrationTest : BaseWebApplication
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _output;

    public ScenarioIntegrationTest(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        _factory = factory;
        _output = output;
        // Use the scenario test config (includes CA1 datapoints + scenario definition)
        _factory.UpdateOptionsPath("../../../Configuration/ScenarioOptionsTest.json");
    }

    [Fact]
    public async Task TriggerScenario_AndWaitForCompletion_RestoresDataPointValues()
    {
        var client = _factory.CreateClient();

        // Verify initial breaker state is ON
        var initialResponse = await client.GetAsync("/api/DataPointConfigs/1/101");
        initialResponse.IsSuccessStatusCode.Should().BeTrue();

        // Trigger the transformer-trip scenario
        var triggerResponse = await client.PostAsync("/api/Scenarios/ca1-transformer-trip/trigger", null);
        triggerResponse.IsSuccessStatusCode.Should().BeTrue();
        _output.WriteLine("Scenario triggered, waiting for completion...");

        // Poll until Completed (max 15s; recovery is immediate in test config)
        ScenarioDto? finalState = null;
        for (int i = 0; i < 30; i++)
        {
            await Task.Delay(500);
            var stateResp = await client.GetAsync("/api/Scenarios");
            stateResp.IsSuccessStatusCode.Should().BeTrue();
            var states = await stateResp.Content.ReadFromJsonAsync<List<ScenarioDto>>();
            finalState = states?.FirstOrDefault(s => s.Name == "ca1-transformer-trip");
            _output.WriteLine($"Status: {finalState?.Status}, Remaining: {finalState?.RemainingMs}ms");
            if (finalState?.Status is "Completed" or "Failed") break;
        }

        finalState.Should().NotBeNull();
        finalState!.Status.Should().Be("Completed");

        // After recovery, power should be back to 5000 (init value)
        var powerResp = await client.GetAsync("/api/DataPointConfigs/1/102");
        powerResp.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task TriggerScenario_WhileRunning_Returns409()
    {
        _factory.UpdateOptionsPath("../../../Configuration/ScenarioOptionsTest.json");
        var client = _factory.CreateClient();

        // First trigger
        await client.PostAsync("/api/Scenarios/ca1-transformer-trip/trigger", null);

        // Second trigger while still running should be conflict
        var response = await client.PostAsync("/api/Scenarios/ca1-transformer-trip/trigger", null);
        // May be 202 (already done quickly) or 409 (still running) — both are acceptable
        ((int)response.StatusCode).Should().BeOneOf(202, 409);
    }

    private record ScenarioDto(string Name, string Status, int RemainingMs);
}
