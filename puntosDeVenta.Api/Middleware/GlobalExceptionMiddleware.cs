using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace puntosDeVenta.Api.Middleware;

/// <summary>
/// Middleware global para manejo de excepciones
/// Proporciona respuestas consistentes y logging detallado
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception - TraceId: {TraceId}", context.TraceIdentifier);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        // 1. Determinamos el Status Code primero
        var statusCode = exception switch
        {
            ValidationException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            ArgumentException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError
        };

        context.Response.StatusCode = (int)statusCode;

        // 2. Extraemos los detalles adicionales (opcional)
        object? extraDetails = exception switch
        {
            ValidationException ex => ex.InnerException,
            ArgumentException ex => ex.Message,
            _ => null
        };

        // 3. Enviamos una respuesta consistente
        return context.Response.WriteAsJsonAsync(new
        {
            success = false,
            statusCode = (int)statusCode,
            message = GetErrorMessage(exception),
            details = extraDetails, // Aqu� unificamos la informaci�n extra
            traceId = context.TraceIdentifier,
            timestamp = DateTime.UtcNow
        });
    }

    private static string GetErrorMessage(Exception exception) => exception switch
    {
        UnauthorizedAccessException => "No autorizado",
        ValidationException => "Validaci�n fallida",
        ArgumentException => "Argumento inv�lido",
        _ => "Ha ocurrido un error inesperado"
    };
}