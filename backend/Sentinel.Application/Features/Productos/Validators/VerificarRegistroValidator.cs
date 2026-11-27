using FluentValidation;
using Sentinel.Application.Features.Productos.DTOs;

namespace Sentinel.Application.Features.Productos.Validators;

public class VerificarRegistroValidator : AbstractValidator<VerificarRegistroRequest>
{
    public VerificarRegistroValidator()
    {
        RuleFor(r => r.TieneDocumento)
            .NotNull().WithMessage("Debe indicar si tiene el documento.");

        When(r => r.TieneDocumento == true, () =>
        {
            RuleFor(r => r.Numero)
                .NotEmpty().WithMessage("El número de registro es obligatorio.")
                .MaximumLength(100).WithMessage("El número no puede superar 100 caracteres.");

            RuleFor(r => r.FechaEmision)
                .NotNull().WithMessage("La fecha de emisión es obligatoria.");
        });

        When(r => r.TieneDocumento == false, () =>
        {
            RuleFor(r => r.Numero)
                .Empty().WithMessage("No debe enviar número si no tiene el documento.");

            RuleFor(r => r.FechaEmision)
                .Null().WithMessage("No debe enviar fecha si no tiene el documento.");
        });
    }
}
