using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Sentinel.API.Common;

// Único punto que serializa errores HTTP en formato RFC 7807.
// Lo usan el middleware de errores y el challenge de JWT.
public static class ProblemDetailsWriter
{
    private const string ProblemContentType = "application/problem+json";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static Task WriteAsync(
        HttpContext ctx,
        int status,
        string title,
        string? detail = null,
        IReadOnlyDictionary<string, string[]>? errors = null)
    {
        if (ctx.Response.HasStarted) return Task.CompletedTask;

        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = ProblemContentType;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };

        if (errors is { Count: > 0 })
            problem.Extensions["errors"] = errors;

        return JsonSerializer.SerializeAsync(ctx.Response.Body, problem, JsonOptions, ctx.RequestAborted);
    }
}
