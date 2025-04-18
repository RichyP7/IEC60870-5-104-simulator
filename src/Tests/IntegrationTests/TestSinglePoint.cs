﻿using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using IEC60870_5_104_simulator.API.Mapping;
using IEC60870_5_104_simulator.Domain;
using IntegrationTests.TestPreparation;
using lib60870.CS101;
using lib60870.CS104;
using Xunit;

namespace IntegrationTests;

public sealed class TestSinglePoint: BaseWebApplication
{
    private readonly CustomWebApplicationFactory<Program> _factory;

    private const int Sa = 20;
    private const int Oa = 57;

    public TestSinglePoint(CustomWebApplicationFactory<Program> factory)
    {
	    _factory = factory;
	    
	    _factory.UpdateOptionsPath("../../../Configuration/OptionsSinglePoint.json");
    }

    [Fact]
    // Starts Simulator and Client in parallel and waits for periodic measurement
    public async Task Test_Simulate_Scaled_Measured_Value()
    {
	    var client = _factory.CreateClient();
	    
	    var response = await client.PutAsJsonAsync($"/api/DataPointConfigs/{Sa}/{Oa}/simulation-mode", SimulationMode.Cyclic.ToString());
	    response.EnsureSuccessStatusCode();

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
	    
	    await tcs.Task.WaitAsync(TimeSpan.FromSeconds(35));
	    
	    await Task.Run(() => con.Close());
    }
    
    [Fact]
    public async Task Test_Simulate_Static_Single_Point_Value()
    {
	    var client = _factory.CreateClient();

        Iec104DataPointDto dto = new()
		{
			Id = "test",
			StationaryAddress = Sa,
			ObjectAddress = Oa,
			Iec104DataType = Iec104DataTypes.M_SP_NA_1,
			Value = new IecValueDto()
			{
				SinglePointValue = new SinglePointValueDto() { Value = true}
			},
			Mode = SimulationModeDto.None
		};
        var valueResponse = await client.PostAsJsonAsync($"/api/DataPointValues/{Sa}/{Oa}", dto);
	    valueResponse.EnsureSuccessStatusCode();
        

	    Connection con = new Connection(HostName);
	    con.DebugOutput = true;

	    var tcs = new TaskCompletionSource();
	    con.SetASDUReceivedHandler((parameter, asdu) =>
	    {
		    try
		    {
			    asdu.TypeId.Should().Be(TypeID.M_SP_NA_1);
			    for (int i = 0; i < asdu.NumberOfElements; i++) {

				    var val = (SinglePointInformation) asdu.GetElement (i);

				    val.ObjectAddress.Should().Be(Oa);
				    val.Value.Should().Be(true);
			    }
			    tcs.SetResult();
		    }
		    catch (Exception ex)
		    {
			    tcs.SetException(ex);
		    }
		    return true;
	    }, null);

	    con.SetConnectionHandler(ConnectionHandler, null);
	    con.Connect();
        
	    await tcs.Task.WaitAsync(TimeSpan.FromSeconds(35));
        
	    await Task.Run(() => con.Close());
    }
    
    [Fact]
    public async Task Test_Receive_Command()
    {
	    var client = _factory.CreateClient();
        

	    Connection con = new Connection(HostName);
	    con.DebugOutput = true;

	    var tcs = new TaskCompletionSource();
	    con.SetASDUReceivedHandler((parameter, asdu) =>
	    {
		    try
		    {
			    if (asdu.TypeId != TypeID.C_SC_NA_1) return false;
			    asdu.TypeId.Should().Be(TypeID.C_SC_NA_1);
			    for (int i = 0; i < asdu.NumberOfElements; i++) {

				    var val = (SingleCommand) asdu.GetElement (i);

				    val.ObjectAddress.Should().Be(28);
			    }

			    tcs.SetResult();
		    }
		    catch (Exception ex)
		    {
			    tcs.SetException(ex);
		    }
		    return true;
	    }, null);

	    con.SetConnectionHandler(ConnectionHandler, null);
	    con.Connect();
	    con.SendControlCommand(CauseOfTransmission.ACTIVATION, 100, new SingleCommand(28, true, false, 0));
	    
	    await tcs.Task.WaitAsync(TimeSpan.FromSeconds(35));
        
	    await Task.Run(() => con.Close());
    }
}