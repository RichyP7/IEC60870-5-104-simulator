using lib60870.CS104;
using Xunit;

namespace IntegrationTests.TestPreparation;

public class BaseWebApplication : IClassFixture<CustomWebApplicationFactory<Program>>
{
    protected const String HostName = "127.0.0.1";
    protected static void ConnectionHandler (object parameter, ConnectionEvent connectionEvent)
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