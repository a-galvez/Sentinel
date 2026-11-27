using FluentValidation;
using Sentinel.Application.Features.Productos.DTOs;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Features.Productos.Validators;

public class ProductoListadoValidator : AbstractValidator<ProductoListadoQuery>
{
    public ProductoListadoValidator()
    {
        RuleFor(q => q.Pagina)
            .GreaterThanOrEqualTo(1)
            .WithMessage("La página debe ser mayor o igual a 1.");

        RuleFor(q => q.TamanoPagina)
            .InclusiveBetween(1, 100)
            .WithMessage("El tamaño de página debe estar entre 1 y 100.");

        RuleFor(q => q.Q)
            .MaximumLength(200)
            .When(q => !string.IsNullOrWhiteSpace(q.Q));

        RuleFor(q => q.Estado)
            .Must(estado => string.IsNullOrWhiteSpace(estado)
                || Enum.GetNames<EstadoVencimiento>()
                    .Contains(estado, StringComparer.OrdinalIgnoreCase))
            .WithMessage("El estado no es válido.");
    }
}
