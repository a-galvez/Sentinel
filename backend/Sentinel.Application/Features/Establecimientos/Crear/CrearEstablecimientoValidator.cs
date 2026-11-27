using FluentValidation;

namespace Sentinel.Application.Features.Establecimientos.Crear;

public sealed class CrearEstablecimientoValidator : AbstractValidator<CrearEstablecimientoRequest>
{
    public CrearEstablecimientoValidator()
    {
        RuleFor(r => r.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200);

        RuleFor(r => r.Ubicacion)
            .NotEmpty().WithMessage("La ubicación es obligatoria.")
            .MaximumLength(300);

        RuleFor(r => r.TipoEstablecimientoId)
            .GreaterThan(0).WithMessage("El tipo de establecimiento es obligatorio.");
    }
}