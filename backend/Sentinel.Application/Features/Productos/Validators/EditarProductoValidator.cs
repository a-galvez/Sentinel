using FluentValidation;
using Sentinel.Application.Features.Productos.DTOs;

namespace Sentinel.Application.Features.Productos.Validators;

public class EditarProductoValidator : AbstractValidator<EditarProductoRequest>
{
    public EditarProductoValidator()
    {
        RuleFor(r => r.Nombre)
            .NotEmpty().WithMessage("El nombre no puede estar vacío.")
            .MaximumLength(200).WithMessage("El nombre no puede superar 200 caracteres.")
            .When(r => r.Nombre is not null);

        RuleFor(r => r.CategoriaProductoId)
            .GreaterThan(0).WithMessage("La categoría no es válida.")
            .When(r => r.CategoriaProductoId is not null);
    }
}
