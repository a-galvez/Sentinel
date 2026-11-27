using FluentValidation;
using Sentinel.Application.Features.Productos.DTOs;

namespace Sentinel.Application.Features.Productos.Validators;

public class CrearProductoValidator : AbstractValidator<CrearProductoRequest>
{
    public CrearProductoValidator()
    {
        RuleFor(r => r.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.");

        RuleFor(r => r.CategoriaProductoId)
            .GreaterThan(0).WithMessage("La categoría es obligatoria.");
    }
}
