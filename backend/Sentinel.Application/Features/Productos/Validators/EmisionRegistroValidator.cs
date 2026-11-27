using FluentValidation;
using Sentinel.Application.Features.Productos.DTOs;

namespace Sentinel.Application.Features.Productos.Validators;

public class EmisionRegistroValidator : AbstractValidator<EmisionRequest>
{
    public EmisionRegistroValidator()
    {
        RuleFor(r => r.Numero)
            .NotEmpty().WithMessage("El número de registro es obligatorio.")
            .MaximumLength(100).WithMessage("El número no puede superar 100 caracteres.");

        RuleFor(r => r.FechaEmision)
            .NotNull().WithMessage("La fecha de emisión es obligatoria.");
    }
}
