using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.Auth;
using Sentinel.Application.Features.Auth.Refresh;

namespace Sentinel.API.Controllers.Auth;

[ApiController]
[Route("api/v1/auth")]
public sealed class RefreshController(
    IUsuarioRepository usuarios,
    IRefreshTokenRepository refreshTokens,
    IClock clock,
    ITokenService tokenService,
    IUnitOfWork uow,
    IValidator<RefreshRequest> validator) : ControllerBase
{
    [HttpPost("refresh")]
    public async Task<ActionResult<TokensResponse>> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken ct)
    {
        await validator.ValidateAndThrowAsync(request, ct);

        var hash = tokenService.HashRefreshToken(request.RefreshToken);

        // RefreshToken no tiene filtro global por empresa; se busca directo por su hash.
        var token = await refreshTokens.ObtenerPorHashAsync(hash, ct);

        // Token inexistente, revocado (reutilización) o expirado → mismo 401.
        if (token is null || token.RevocadoEn is not null || token.Expira <= clock.UtcAhora)
        {
            throw new UnauthorizedAppException("INVALID_REFRESH_TOKEN", "La sesión venció. Inicia sesión de nuevo.");
        }

        // El endpoint es público (sin sesión): se ignora el filtro global por empresa.
        var usuario = await usuarios.ObtenerPorIdIgnorandoFiltrosAsync(token.UsuarioId, ct);

        if (usuario is null || !usuario.Activo)
        {
            throw new UnauthorizedAppException("INVALID_REFRESH_TOKEN", "La sesión venció. Inicia sesión de nuevo.");
        }

        // Rotación: el token anterior queda revocado y se emite uno nuevo vinculado a él.
        token.RevocadoEn = clock.UtcAhora;

        var tokens = await tokenService.RotarAsync(usuario, token.RefreshTokenId, ct);
        await uow.SaveChangesAsync(ct);

        return Ok(new TokensResponse(tokens.AccessToken, tokens.RefreshToken, tokens.ExpiraEn));
    }
}
