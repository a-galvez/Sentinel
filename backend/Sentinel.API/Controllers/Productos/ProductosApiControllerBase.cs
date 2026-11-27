using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Sentinel.API.Controllers.Productos;

public abstract class ProductosApiControllerBase : ControllerBase
{
    protected ObjectResult ValidacionInvalida(ValidationResult validacion)
    {
        var errors = validacion.Errors
            .GroupBy(e => ToCamelCase(e.PropertyName))
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return Problema(
            400,
            "Datos inválidos",
            "Uno o más campos no son válidos.",
            errors);
    }

    protected ObjectResult Problema(
        int status,
        string title,
        string detail,
        IDictionary<string, string[]>? errors = null)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };

        if (errors is not null)
            problem.Extensions["errors"] = errors;

        return StatusCode(status, problem);
    }

    private static string ToCamelCase(string? nombre)
        => string.IsNullOrEmpty(nombre)
            ? string.Empty
            : char.ToLowerInvariant(nombre[0]) + nombre[1..];
}
