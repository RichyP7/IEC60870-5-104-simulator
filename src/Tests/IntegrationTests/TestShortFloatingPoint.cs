using System.Net.Http.Json;
using FluentAssertions;
using IEC60870_5_104_simulator.Domain;
using IntegrationTests.TestPreparation;
using lib60870.CS101;
using lib60870.CS104;
using Xunit;

namespace IntegrationTests;

public class TestShortFloatingPoint : BaseWebApplication
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    
    int oa = 29;
    int sa = 20;

    public TestShortFloatingPoint(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
	    
        _factory.UpdateOptionsPath("../../../Configuration/OptionsMeasuredValueFP.json");
    }
    
    [Fact]
    public async Task Test_Simulate_Floating_Point_Value()
    {
        var client = _factory.CreateClient();
        

        Connection con = new Connection(HostName);
        con.DebugOutput = true;

        var tcs = new TaskCompletionSource();
        con.SetASDUReceivedHandler((parameter, asdu) =>
        {
    	    try
    	    {
    		    asdu.TypeId.Should().Be(TypeID.M_ME_NC_1);
    		    for (int i = 0; i < asdu.NumberOfElements; i++) {

    			    var val = asdu.GetElement (i);

    			    val.ObjectAddress.Should().Be(29);
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
        var response = await client.PutAsJsonAsync($"/api/DataPointConfigs/{sa}/{oa}/simulation-mode", SimulationMode.None.ToString());
        response.EnsureSuccessStatusCode();
        
        await Task.Run(() => con.Close());
    }
    
    [Fact]
    public async Task Test_Simulate_Static_Floating_Point_Value()
    {
	    var client = _factory.CreateClient();
	    var response = await client.PutAsJsonAsync($"/api/DataPointConfigs/{sa}/{oa}/simulation-mode", SimulationMode.CyclicStatic.ToString());
	    response.EnsureSuccessStatusCode();
	    
	    var valueResponse = await client.PutAsJsonAsync($"/api/DataPointValue/{sa}/{oa}", "4.3");
	    valueResponse.EnsureSuccessStatusCode();

	    await Task.Delay(500);
        

	    Connection con = new Connection(HostName);
	    con.DebugOutput = true;

	    var tcs = new TaskCompletionSource();
	    bool firstTry = true;
	    con.SetASDUReceivedHandler((parameter, asdu) =>
	    {
		    try
		    {
			    // Clear old sent values
			    if (firstTry)
			    {
				    firstTry = false;
				    return false;
			    }
			    asdu.TypeId.Should().Be(TypeID.M_ME_NC_1);
			    for (int i = 0; i < asdu.NumberOfElements; i++) {

				    var val = (MeasuredValueShort) asdu.GetElement (i);

				    val.ObjectAddress.Should().Be(29); // IOA from test config.json
				    val.Value.Should().Be(4.3f);
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
        
	    await Task.Run(() => con.Close());
    }
}