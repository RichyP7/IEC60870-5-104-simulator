using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace IEC60870_5_104_simulator.Infrastructure.ExceptionHandler;

public class InternalErrorExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            var response = new ErrorResponse()
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                ExceptionMessage = exception.Message,
                Title = "Internal Server Error",
            };
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            
            return true;
        }
    }