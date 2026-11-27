using FluentValidation;

namespace Sentinel.Application.Features.Auth.Registro;

public sealed class RegistroValidator : AbstractValidator<RegistroRequest>
{
    public RegistroValidator()
    {
        RuleFor(r => r.NombreEmpresa)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(r => r.Nombre)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(r => r.Correo)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(254);

        RuleFor(r => r.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128);
    }
}
