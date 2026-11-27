using FluentValidation;

namespace Sentinel.Application.Features.Establecimientos.Listar;

public sealed class ListarEstablecimientosValidator : AbstractValidator<ListarEstablecimientosRequest>
{
    public ListarEstablecimientosValidator()
    {
        RuleFor(r => r.Pagina)
            .GreaterThanOrEqualTo(1).WithMessage("La página debe ser mayor o igual a 1.");

        RuleFor(r => r.TamanoPagina)
            .InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100.");
    }
}
