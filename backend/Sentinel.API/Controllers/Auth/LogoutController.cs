using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Features.Auth.Refresh;

namespace Sentinel.API.Controllers.Auth;

// POST /auth/logout (H2).
// Sin [Authorize]: se puede cerrar sesión aunque el access token ya haya vencido.
// Idempotente: si el token no existe o ya estaba revocado, igual responde 204.
[ApiController]
[Route("api/v1/auth")]
public sealed class LogoutController(
    IRefreshTokenRepository refreshTokens,
    ITokenService tokenService,
    IClock clock,
    IUnitOfWork uow,
    IValidator<RefreshRequest> validator) : ControllerBase
{
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] RefreshRequest request,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var hash = tokenService.HashRefreshToken(request.RefreshToken);
        var token = await refreshTokens.ObtenerPorHashAsync(hash, ct);

        if (token is { RevocadoEn: null })
        {
            token.RevocadoEn = clock.UtcAhora;
            await uow.SaveChangesAsync(ct);
        }

        return NoContent();
    }
}
