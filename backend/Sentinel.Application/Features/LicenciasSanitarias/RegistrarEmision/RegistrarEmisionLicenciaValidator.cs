using FluentValidation;

namespace Sentinel.Application.Features.LicenciasSanitarias.RegistrarEmision;

public sealed class RegistrarEmisionLicenciaValidator : AbstractValidator<RegistrarEmisionLicenciaRequest>
{
    private const string MensajeJuntos = "Número y fecha de emisión deben enviarse juntos.";

    public RegistrarEmisionLicenciaValidator()
    {
        // Solo uno de los dos: el error sale junto a `numero`, como en el contrato.
        RuleFor(r => r.Numero)
            .Must(_ => false).WithMessage(MensajeJuntos)
            .When(r => string.IsNullOrWhiteSpace(r.Numero) != (r.FechaEmision is null));

        // Ninguno de los dos.
        RuleFor(r => r.Numero)
            .NotEmpty().WithMessage("El número de licencia es obligatorio.")
            .When(r => string.IsNullOrWhiteSpace(r.Numero) && r.FechaEmision is null);

        RuleFor(r => r.FechaEmision)
            .NotNull().WithMessage("La fecha de emisión es obligatoria.")
            .When(r => string.IsNullOrWhiteSpace(r.Numero) && r.FechaEmision is null);

        RuleFor(r => r.Numero)
            .MaximumLength(100).WithMessage("El número no puede superar 100 caracteres.");
    }
}
