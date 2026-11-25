using FluentValidation;
using Sentinel.Application.Abstractions.Repositories;

namespace Sentinel.Application.Features.Establecimientos.Crear;

public sealed class CrearEstablecimientoValidator : AbstractValidator<CrearEstablecimientoRequest>
{
    public CrearEstablecimientoValidator(ICatalogoRepository catalogos)
    {
        RuleFor(r => r.Nombre)
            .NotEmpty().WithMessage("El nombre del establecimiento es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(r => r.Ubicacion)
            .NotEmpty().WithMessage("La ubicación es obligatoria.")
            .MaximumLength(300).WithMessage("La ubicación no puede superar 300 caracteres.");

        RuleFor(r => r.TipoEstablecimientoId)
            .MustAsync((id, ct) => catalogos.ExisteTipoEstablecimientoAsync(id, ct))
            .WithMessage("El tipo de establecimiento no existe.");
    }
}