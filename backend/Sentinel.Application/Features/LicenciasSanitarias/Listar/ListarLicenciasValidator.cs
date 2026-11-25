using FluentValidation;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Features.LicenciasSanitarias.Listar;

public sealed class ListarLicenciasValidator : AbstractValidator<ListarLicenciasRequest>
{
    public ListarLicenciasValidator()
    {
        RuleFor(r => r.Estado)
            .Must(estado => Enum.GetNames<EstadoVencimiento>().Contains(estado))
            .When(r => r.Estado is not null)
            .WithMessage("El estado no es válido. Valores permitidos: NoVerificado, EnTramite, Vigente, PorVencer, Vencido.");

        RuleFor(r => r.Pagina)
            .GreaterThanOrEqualTo(1).WithMessage("La página debe ser mayor o igual a 1.");

        RuleFor(r => r.TamanoPagina)
            .InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100.");
    }
}