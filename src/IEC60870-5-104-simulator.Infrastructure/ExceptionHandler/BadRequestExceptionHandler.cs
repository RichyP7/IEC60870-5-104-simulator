using IEC60870_5_104_simulator.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IEC60870_5_104_simulator.Infrastructure.ExceptionHandler;

public class BadRequestExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is BadRequestException badRequestException)
        {
            var response = new ErrorResponse()
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ExceptionMessage = badRequestException.Message,
                Title = "Bad Request",
            };
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            
            return true;
        }

        return false;
    }
}