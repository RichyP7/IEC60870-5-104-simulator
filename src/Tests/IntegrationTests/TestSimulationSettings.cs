using System.Net.Http.Json;
using FluentAssertions;
using IEC60870_5_104_simulator.Service;
using IntegrationTests.TestPreparation;
using Xunit;

namespace IntegrationTests;

public class TestSimulationSettings(CustomWebApplicationFactory<Program> factory) : BaseWebApplication
{

    [Fact]
    // Starts Simulator and Client in parallel and waits for periodic measurement
    public async Task Test_Simulate_Scaled_Measured_Value()
    {
        var simulationEngineEndpoint = "/api/SimulationEngineState";
        var client = factory.CreateClient();
	    
        var response = await client.PostAsync($"{simulationEngineEndpoint}?command=stop", null);
        response.EnsureSuccessStatusCode();
        
        //=> Status should be stopped
        var currentStateResponse = await client.GetAsync($"{simulationEngineEndpoint}");
        var responseString = await currentStateResponse.Content.ReadAsStringAsync();
        Enum.Parse<SimulationEngine.SimulationState>(responseString.Trim('"')).Should().Be(SimulationEngine.SimulationState.Stopped);

        var responseStart = await client.PostAsync($"{simulationEngineEndpoint}?command=start", null);
        responseStart.EnsureSuccessStatusCode();
        
        currentStateResponse = await client.GetAsync($"{simulationEngineEndpoint}");
        responseString = await currentStateResponse.Content.ReadAsStringAsync();
        Enum.Parse<SimulationEngine.SimulationState>(responseString.Trim('"')).Should().Be(SimulationEngine.SimulationState.Running);
    }

}