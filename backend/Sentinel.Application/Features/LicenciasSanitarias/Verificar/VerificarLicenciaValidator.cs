using FluentValidation;

namespace Sentinel.Application.Features.LicenciasSanitarias.Verificar;

public sealed class VerificarLicenciaValidator : AbstractValidator<VerificarLicenciaRequest>
{
    public VerificarLicenciaValidator()
    {
        RuleFor(r => r.TieneDocumento)
            .NotNull().WithMessage("Indique si el establecimiento ya tiene la licencia.");

        // "Sí": número y emisión van juntos.
        When(r => r.TieneDocumento == true, () =>
        {
            RuleFor(r => r.Numero)
                .NotEmpty().WithMessage("El número de licencia es obligatorio.")
                .MaximumLength(100);
            RuleFor(r => r.FechaEmision)
                .NotNull().WithMessage("La fecha de emisión es obligatoria.");
        });

        // "No": no se aceptan datos de la licencia.
        When(r => r.TieneDocumento == false, () =>
        {
            RuleFor(r => r.Numero).Empty().WithMessage("No envíe número si la licencia no existe.");
            RuleFor(r => r.FechaEmision).Null().WithMessage("No envíe fecha si la licencia no existe.");
        });
    }
}