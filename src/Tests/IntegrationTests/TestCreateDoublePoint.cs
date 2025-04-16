using System.Net.Http.Json;
using FluentAssertions;
using IEC60870_5_104_simulator.API.Mapping;
using IEC60870_5_104_simulator.Domain;
using IntegrationTests.TestPreparation;
using lib60870.CS101;
using lib60870.CS104;
using Xunit;

namespace IntegrationTests;

public class TestCreateDoublePoint : BaseWebApplication
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    

    public TestCreateDoublePoint(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
	    
        _factory.UpdateOptionsPath("../../../Configuration/EmptyOptions.json");
    }

    [Fact]
    public async Task Test_Create_And_Simulate_Double_Point()
    {
        var client = _factory.CreateClient();
        var toCreate = new Iec104DataPointDto
        {
            Id = "TestCreateDoublePoint",
            Mode = SimulationModeDto.CyclicStatic,
            ObjectAddress = 20,
            StationaryAddress = 21,
            Iec104DataType = Iec104DataTypes.M_DP_NA_1,
            Value = new IecValueDto
            {
                DoublePointValue =
                    new DoublePointValueDto()
                    {
                        Value = IecDoublePointValueEnumDto.INTERMEDIATE
                    }
            }
        };
        
        var response = await client.PostAsJsonAsync($"/api/DataPointConfigs",
            toCreate);
        response.EnsureSuccessStatusCode();
        var result = response.Content.ReadAsStringAsync();
        
        Connection con = new Connection(HostName);
        con.DebugOutput = true;

        var tcs = new TaskCompletionSource();
        con.SetASDUReceivedHandler((parameter, asdu) =>
        {
            try
            {
                asdu.TypeId.Should().Be(TypeID.M_DP_NA_1);
                for (int i = 0; i < asdu.NumberOfElements; i++) {

                    var val = (DoublePointInformation)asdu.GetElement (i);

                    val.ObjectAddress.Should().Be(20);
                    val.Value.Should().Be(DoublePointValue.INTERMEDIATE);
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

}