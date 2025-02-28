namespace IEC60870_5_104_simulator.Infrastructure.Exceptions;

public class BadRequestException(String exceptionMessage) : Exception(exceptionMessage)
{
    
}