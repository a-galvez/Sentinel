using FluentValidation;

namespace Sentinel.Application.Features.Auth.Refresh;

public sealed class RefreshValidator : AbstractValidator<RefreshRequest>
{
    public RefreshValidator()
    {
        RuleFor(r => r.RefreshToken).NotEmpty();
    }
}
