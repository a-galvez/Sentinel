using FluentValidation;

namespace Sentinel.Application.Features.Auth.Login;

public sealed class LoginValidator : AbstractValidator<LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(r => r.Correo).NotEmpty().EmailAddress();
        RuleFor(r => r.Password).NotEmpty();
    }
}
