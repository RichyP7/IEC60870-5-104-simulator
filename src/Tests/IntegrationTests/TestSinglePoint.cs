using FluentAssertions;
using IntegrationTests.TestPreparation;
using lib60870;
using lib60870.CS101;
using lib60870.CS104;
using Xunit;
using Xunit.Abstractions;

namespace IntegrationTests;

public sealed class TestSinglePoint: IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly ITestOutputHelper _testOutputHelper;

    private const String HostName = "127.0.0.1";

    public TestSinglePoint(CustomWebApplicationFactory<Program> factory, ITestOutputHelper testOutputHelper)
    {
	    _factory = factory;
	    _testOutputHelper = testOutputHelper;
	    
	    _factory.UpdateOptionsPath("../../../Configuration/OptionsSinglePoint.json");
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
			    asdu.TypeId.Should().Be(TypeID.M_SP_NA_1);
			    for (int i = 0; i < asdu.NumberOfElements; i++) {

				    var val = (SinglePointInformation)asdu.GetElement (i);

				    val.ObjectAddress.Should().Be(57); // IOA from test config.json
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
    
		private static void ConnectionHandler (object parameter, ConnectionEvent connectionEvent)
		{
			switch (connectionEvent) {
				case ConnectionEvent.OPENED:
					Console.WriteLine ("Connected");
					break;
				case ConnectionEvent.CLOSED:
					Console.WriteLine ("Connection closed");
					break;
				case ConnectionEvent.STARTDT_CON_RECEIVED:
					Console.WriteLine ("STARTDT CON received");
					break;
				case ConnectionEvent.STOPDT_CON_RECEIVED:
					Console.WriteLine ("STOPDT CON received");
					break;
			}
		}
}