using lib60870;
using lib60870.CS101;
using lib60870.CS104;

namespace IEC60870_5_104_simulator.TestClient;

/// <summary>
/// Minimal IEC 60870-5-104 test client for evaluating the simulator configuration.
/// Connects to the simulator, sends a general interrogation and prints every message
/// Usage: dotnet run [host] [port]   (defaults: 127.0.0.1 2404)
/// Press Q or Ctrl+C to quit.
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        string host = args.Length > 0 ? args[0] : "127.0.0.1";
        int port = args.Length > 1 && int.TryParse(args[1], out int p) ? p : 2404;

        Console.WriteLine($"IEC 60870-5-104 test client — connecting to {host}:{port} (press Q to quit)");

        using var cancel = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancel.Cancel(); };

        using var connected = new ManualResetEventSlim(false);

        var connection = new Connection(host, port);
        connection.SetConnectionHandler((p, evt) =>
        {
            if (evt == ConnectionEvent.STARTDT_CON_RECEIVED) connected.Set();
            OnConnectionEvent(p, evt);
        }, null);
        connection.SetASDUReceivedHandler(OnAsduReceived, null);

        try
        {
            connection.Connect(); // may throw (e.g. host unreachable); STARTDT handshake awaited below
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Could not connect to {host}:{port}: {ex.GetBaseException().Message}");
            Console.Error.WriteLine("Is the simulator running? Start it with: dotnet run --project src/IEC60870-5-104-simulator.API");
            return 1;
        }

        bool isConnected;
        try
        {
            isConnected = connected.Wait(TimeSpan.FromSeconds(10), cancel.Token);
        }
        catch (OperationCanceledException)
        {
            connection.Close();
            return 0;
        }

        if (!isConnected)
        {
            Console.Error.WriteLine($"Could not connect to {host}:{port} within 10 seconds.");
            Console.Error.WriteLine("Is the simulator running? Start it with: dotnet run --project src/IEC60870-5-104-simulator.API");
            connection.Close();
            return 1;
        }

        connection.SendInterrogationCommand(CauseOfTransmission.ACTIVATION, 0xFFFF, QualifierOfInterrogation.STATION);

        bool interactive = !Console.IsInputRedirected;
        while (!cancel.IsCancellationRequested)
        {
            if (interactive && Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Q)
                break;
            Thread.Sleep(100);
        }

        connection.Close();
        Console.WriteLine("Disconnected.");
        return 0;
    }

    private static void OnConnectionEvent(object parameter, ConnectionEvent connectionEvent)
    {
        string message = connectionEvent switch
        {
            ConnectionEvent.OPENED => "Connection opened",
            ConnectionEvent.CLOSED => "Connection closed",
            ConnectionEvent.STARTDT_CON_RECEIVED => "Data transfer started (STARTDT confirmed)",
            ConnectionEvent.STOPDT_CON_RECEIVED => "Data transfer stopped (STOPDT confirmed)",
            ConnectionEvent.CONNECT_FAILED => "Connection attempt failed",
            _ => connectionEvent.ToString(),
        };
        Console.WriteLine($"{Timestamp()} | --- {message} ---");
    }

    private static bool OnAsduReceived(object parameter, ASDU asdu)
    {
        if (asdu.TypeId == TypeID.C_IC_NA_1)
        {
            string text = asdu.Cot switch
            {
                CauseOfTransmission.ACTIVATION_CON =>
                    (asdu.IsNegative ? "Interrogation rejected" : "Interrogation confirmed"),
                CauseOfTransmission.ACTIVATION_TERMINATION => "Interrogation finished",
                _ => $"Interrogation ({asdu.Cot})",
            };
            Console.WriteLine($"{Timestamp()} | --- {text} ---");
            return true;
        }

        if (asdu.TypeId == TypeID.M_EI_NA_1)
        {
            Console.WriteLine($"{Timestamp()} | --- Station initialized (end of initialization) ---");
            return true;
        }

        for (int i = 0; i < asdu.NumberOfElements; i++)
        {
            InformationObject io = asdu.GetElement(i);
            (string value, string quality, string time) = Describe(io);

            Console.WriteLine(
                $"{Timestamp()} | {asdu.Cot,-15} | {asdu.TypeId,-12} | CA {asdu.Ca,4} | IOA {io.ObjectAddress,5} | {value,10}{quality}{time}");
        }

        return true;
    }

    /// <summary>Extracts value, quality and timestamp for all datapoint types the simulator sends.</summary>
    private static (string Value, string Quality, string Time) Describe(InformationObject io) => io switch
    {
        // Single point (bool)
        SinglePointWithCP56Time2a v => (OnOff(v.Value), Q(v.Quality), T(v.Timestamp)),
        SinglePointInformation v => (OnOff(v.Value), Q(v.Quality), ""),

        // Double point (INTERMEDIATE / OFF / ON / INDETERMINATE)
        DoublePointWithCP56Time2a v => (v.Value.ToString(), Q(v.Quality), T(v.Timestamp)),
        DoublePointInformation v => (v.Value.ToString(), Q(v.Quality), ""),

        // Step position (-64..63)
        StepPositionWithCP56Time2a v => (v.Value.ToString(), Q(v.Quality), T(v.Timestamp)),
        StepPositionInformation v => (v.Value.ToString(), Q(v.Quality), ""),

        // Normalized (-1..1) — order matters: MeasuredValueNormalized derives from ...WithoutQuality
        MeasuredValueNormalizedWithCP56Time2a v => (v.NormalizedValue.ToString("0.###"), Q(v.Quality), T(v.Timestamp)),
        MeasuredValueNormalized v => (v.NormalizedValue.ToString("0.###"), Q(v.Quality), ""),
        MeasuredValueNormalizedWithoutQuality v => (v.NormalizedValue.ToString("0.###"), "", ""),

        // Scaled (Int16)
        MeasuredValueScaledWithCP56Time2a v => (v.ScaledValue.ToString(), Q(v.Quality), T(v.Timestamp)),
        MeasuredValueScaled v => (v.ScaledValue.ToString(), Q(v.Quality), ""),

        // Short float
        MeasuredValueShortWithCP56Time2a v => (v.Value.ToString("0.###"), Q(v.Quality), T(v.Timestamp)),
        MeasuredValueShort v => (v.Value.ToString("0.###"), Q(v.Quality), ""),

        _ => (io.ToString() ?? "?", "", ""),
    };

    private static string Timestamp() => DateTime.Now.ToString("HH:mm:ss.fff");

    private static string OnOff(bool value) => value ? "ON (true)" : "OFF (false)";

    private static string Q(QualityDescriptor quality) =>
        quality.Invalid || quality.NonTopical || quality.Substituted || quality.Blocked || quality.Overflow
            ? $"  [{quality}]"
            : "  [GOOD]";

    private static string T(CP56Time2a timestamp) => $"  @ {timestamp.GetDateTime():HH:mm:ss.fff}";
}
