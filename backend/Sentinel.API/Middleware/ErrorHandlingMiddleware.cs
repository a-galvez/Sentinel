using FluentValidation;
using Sentinel.Application.Common.Exceptions;

namespace Sentinel.API.Middleware;

public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> log)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try { await next(ctx); }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => new
            {
                field = ToCamelCase(e.PropertyName),
                message = e.ErrorMessage
            });
            await Responder(ctx, 400, new { error = "VALIDATION_ERROR", message = "Hay campos inválidos.", errors });
        }
        catch (AppException ex)
        {
            var status = ex switch
            {
                NotFoundException => 404,
                ConflictException => 409,
                UnauthorizedAppException => 401,
                _ => 400
            };
            await Responder(ctx, status, new { error = ex.Code, message = ex.Message });
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error no controlado");
            await Responder(ctx, 500, new { error = "INTERNAL_ERROR", message = "Ocurrió un error inesperado." });
        }
    }

    private static string ToCamelCase(string? nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return string.Empty;
        return char.ToLowerInvariant(nombre[0]) + nombre[1..];
    }

    private static Task Responder(HttpContext ctx, int status, object body)
    {
        if (ctx.Response.HasStarted) return Task.CompletedTask;
        ctx.Response.StatusCode = status;
        return ctx.Response.WriteAsJsonAsync(body);
    }
}
