namespace IEC60870_5_104_simulator.Infrastructure.ExceptionHandler;

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Title { get; set; }
    public string ExceptionMessage { get; set; }
}