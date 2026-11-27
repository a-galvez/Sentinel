using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Sentinel.API.Middleware;
using Sentinel.Application.Common.Exceptions;

namespace Sentinel.Tests.Middleware;

public class ErrorHandlingMiddlewareTests
{
    [Fact]
    public async Task Validacion_responde_400_con_problem_json_y_errors()
    {
        var ctx = CrearContexto();
        var middleware = Crear(_ => throw new ValidationException(new[]
        {
            new ValidationFailure("Correo", "El correo no es válido."),
            new ValidationFailure("Correo", "El correo es obligatorio.")
        }));

        await middleware.InvokeAsync(ctx);

        Assert.Equal(400, ctx.Response.StatusCode);
        Assert.StartsWith("application/problem+json", ctx.Response.ContentType!);

        var json = await LeerJsonAsync(ctx);
        Assert.Equal(400, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Datos inválidos", json.RootElement.GetProperty("title").GetString());
        Assert.Equal("Uno o más campos no son válidos.", json.RootElement.GetProperty("detail").GetString());

        var errores = json.RootElement.GetProperty("errors").GetProperty("correo");
        Assert.Equal(2, errores.GetArrayLength());
    }

    [Fact]
    public async Task Credenciales_invalidas_responde_401_con_titulo_del_contrato()
    {
        var ctx = CrearContexto();
        var middleware = Crear(_ => throw new UnauthorizedAppException(
            "INVALID_CREDENTIALS", "Correo o contraseña incorrectos"));

        await middleware.InvokeAsync(ctx);

        Assert.Equal(401, ctx.Response.StatusCode);

        var json = await LeerJsonAsync(ctx);
        Assert.Equal(401, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Credenciales incorrectas", json.RootElement.GetProperty("title").GetString());
        Assert.Equal("Correo o contraseña incorrectos", json.RootElement.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task Conflicto_incluye_errors_por_campo()
    {
        var ctx = CrearContexto();
        var middleware = Crear(_ => throw new ConflictException(
            "CORREO_YA_REGISTRADO", "Ya existe una cuenta con ese correo.")
        {
            Errors = new Dictionary<string, string[]>
            {
                ["correo"] = ["El correo ya está registrado."]
            }
        });

        await middleware.InvokeAsync(ctx);

        Assert.Equal(409, ctx.Response.StatusCode);

        var json = await LeerJsonAsync(ctx);
        Assert.Equal(409, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Conflicto", json.RootElement.GetProperty("title").GetString());
        Assert.Equal(
            "El correo ya está registrado.",
            json.RootElement.GetProperty("errors").GetProperty("correo")[0].GetString());
    }

    [Fact]
    public async Task Excepcion_no_controlada_responde_500_generico()
    {
        var ctx = CrearContexto();
        var middleware = Crear(_ => throw new InvalidOperationException("boom"));

        await middleware.InvokeAsync(ctx);

        Assert.Equal(500, ctx.Response.StatusCode);

        var json = await LeerJsonAsync(ctx);
        Assert.Equal(500, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Error interno", json.RootElement.GetProperty("title").GetString());
        Assert.Equal("Ocurrió un error inesperado.", json.RootElement.GetProperty("detail").GetString());
        Assert.False(
            json.RootElement.TryGetProperty("type", out var type)
            && type.ValueKind == JsonValueKind.Null);
    }

    private static DefaultHttpContext CrearContexto()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }

    private static ErrorHandlingMiddleware Crear(RequestDelegate next)
        => new(next, NullLogger<ErrorHandlingMiddleware>.Instance);

    private static async Task<JsonDocument> LeerJsonAsync(HttpContext ctx)
    {
        ctx.Response.Body.Position = 0;
        return await JsonDocument.ParseAsync(ctx.Response.Body);
    }
}
