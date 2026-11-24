using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions;

namespace Sentinel.Application.Features.Requisitos;

// Validación del cuerpo del PATCH. La validación fuerte del responsable (existe y está
// activo en la empresa) se hace en el handler, que ya tiene la fila y el tenant.
public sealed class RequisitoUpdateValidator : AbstractValidator<RequisitoUpdateRequest>
{
    public RequisitoUpdateValidator()
    {
        RuleFor(r => r.Estado)
            .IsInEnum()
            .When(r => r.Estado.HasValue);
    }
}
