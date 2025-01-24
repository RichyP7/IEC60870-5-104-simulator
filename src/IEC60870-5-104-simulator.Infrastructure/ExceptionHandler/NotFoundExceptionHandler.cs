using IEC60870_5_104_simulator.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace IEC60870_5_104_simulator.Infrastructure.ExceptionHandler;

public class NotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is KeyNotFoundException notFoundException)
        {
            var response = new ErrorResponse()
            {
                StatusCode = StatusCodes.Status404NotFound,
                ExceptionMessage = notFoundException.Message,
                Title = "Not Found",
            };
            httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            
            return true;
        }

        return false;
    }
}