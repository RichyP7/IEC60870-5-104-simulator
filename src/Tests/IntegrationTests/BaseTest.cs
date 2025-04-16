using FluentAssertions;
using IntegrationTests.TestPreparation;
using lib60870;
using lib60870.CS101;
using lib60870.CS104;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests;

public sealed class BaseTest: BaseWebApplication
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _testOutputHelper;

    public BaseTest(CustomWebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
	    _factory = factory;
	    _testOutputHelper = testOutputHelper;
    }
	[Fact]
	// Starts Simulator and Client in parallel and waits for periodic measurement
	public async Task GetDatapoints()
	{
        HttpClient client =_factory.CreateClient();
        var response = await client.GetAsync($"/api/DataPointConfigs");

		Assert.True(response.IsSuccessStatusCode);
    }

        [Fact]
    public async Task Test_Simulator_Starts_Correctly()
    {
	    _factory.CreateClient();
    }

    [Fact]
    // Starts Simulator and Client in parallel and waits for periodic measurement
    public async Task Test_Simulate_Scaled_Measured_Value()
    {
	    var client = _factory.CreateClient();

	    _testOutputHelper.WriteLine("Using lib60870.NET version " + LibraryCommon.GetLibraryVersionString());

	    Connection con = new Connection(HostName);
	    con.DebugOutput = true;

	    var tcs = new TaskCompletionSource();
	    con.SetASDUReceivedHandler((parameter, asdu) =>
	    {
		    try
		    {
			    asdu.TypeId.Should().Be(TypeID.M_ME_NB_1);
			    for (int i = 0; i < asdu.NumberOfElements; i++)
			    {
				    var msv = (MeasuredValueScaled)asdu.GetElement(i);
				    
				    msv.ObjectAddress.Should().Be(25); // IOA from test config.json
				    msv.ScaledValue.Value.Should().Be(Int16.MaxValue);
			    }

			    tcs.SetResult(); // end loop
		    }
		    catch (Exception ex)
		    {
			    tcs.SetException(ex); // Send Error to main thread
		    }

		    return true;
	    }, null);

	    con.SetConnectionHandler(ConnectionHandler, null);
	    con.Connect();
	    
	    await tcs.Task.WaitAsync(TimeSpan.FromSeconds(30));
	    con.Close();
    }

}