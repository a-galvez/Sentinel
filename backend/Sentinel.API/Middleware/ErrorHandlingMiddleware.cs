using FluentValidation;
using Sentinel.API.Common;
using Sentinel.Application.Common.Exceptions;

namespace Sentinel.API.Middleware;

// Traduce excepciones de Application a respuestas HTTP en formato RFC 7807.
public sealed class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> log)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors
                .GroupBy(e => ToCamelCase(e.PropertyName))
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            await ProblemDetailsWriter.WriteAsync(
                ctx, 400, "Datos inválidos", "Uno o más campos no son válidos.", errors);
        }
        catch (AppException ex)
        {
            var (status, title) = Clasificar(ex);
            await ProblemDetailsWriter.WriteAsync(ctx, status, title, ex.Message, ex.Errors);
        }
        catch (Exception ex)
        {
            log.LogError(ex, "Error no controlado");
            await ProblemDetailsWriter.WriteAsync(ctx, 500, "Error interno", "Ocurrió un error inesperado.");
        }
    }

    private static (int Status, string Title) Clasificar(AppException ex) => ex switch
    {
        NotFoundException => (404, "No encontrado"),
        ConflictException => (409, "Conflicto"),
        UnauthorizedAppException u => (401, u.Code switch
        {
            "INVALID_CREDENTIALS" => "Credenciales incorrectas",
            "INVALID_REFRESH_TOKEN" => "Sesión vencida",
            _ => "No autenticado"
        }),
        BadRequestException => (400, "Solicitud incorrecta"),
        _ => (400, "Solicitud incorrecta")
    };

    private static string ToCamelCase(string? nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return string.Empty;
        return char.ToLowerInvariant(nombre[0]) + nombre[1..];
    }
}
