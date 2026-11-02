using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Pedidos.Application;
using Pedidos.Domain;

namespace Pedidos.Api;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var status = exception switch { BusinessException => 400, NotFoundException => 404, ConflictException => 409, _ => 500 };
        if (status == 500) logger.LogError(exception, "Error no controlado. TraceId: {TraceId}", context.TraceIdentifier);
        else logger.LogWarning("Peticion rechazada: {Status} {Message}", status, exception.Message);
        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status, Title = status == 500 ? "Ocurrio un error inesperado." : exception.Message,
            Extensions = { ["traceId"] = context.TraceIdentifier }
        }, cancellationToken);
        return true;
    }
}