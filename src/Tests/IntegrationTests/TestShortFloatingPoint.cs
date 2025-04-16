using FluentAssertions;
using IEC60870_5_104_simulator.API.Mapping;
using IEC60870_5_104_simulator.Domain;
using IntegrationTests.TestPreparation;
using lib60870.CS101;
using lib60870.CS104;
using System.Net.Http.Json;
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
    public async Task Test_Simulate_Static_Floating_Point_Value()
    {
        var client = _factory.CreateClient();

        var toCreate = new Iec104DataPointDto
        {
            Id = "TestShortDoublePoint",
            Mode = SimulationModeDto.None,
            ObjectAddress = oa,
            StationaryAddress = sa,
            Iec104DataType = Iec104DataTypes.M_ME_NC_1,
            Value = new IecValueDto
            {
                FloatValue =new FloatValueDto()
                {
                    Value = 5
                }
            }
        };
        var valueResponse = await client.PostAsJsonAsync($"/api/DataPointValues/{sa}/{oa}", toCreate);
        valueResponse.EnsureSuccessStatusCode();

        await Task.Delay(500);


        Connection con = new Connection(HostName);
        con.DebugOutput = true;

        var tcs = new TaskCompletionSource();
        con.SetASDUReceivedHandler((parameter, asdu) =>
        {
            try
            {
                asdu.TypeId.Should().Be(TypeID.M_ME_NC_1);
                for (int i = 0; i < asdu.NumberOfElements; i++)
                {
                    var val = (MeasuredValueShort)asdu.GetElement(i);
                    val.ObjectAddress.Should().Be(29); // IOA from test config.json
                    val.Value.Should().Be(5);
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