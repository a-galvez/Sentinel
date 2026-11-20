using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Common.Exceptions;

namespace Sentinel.Application.Features.Auth.Refresh;

public sealed class RefreshHandler(
    IAppDbContext db,
    IClock clock,
    ITokenService tokenService)
{
    public async Task<AuthResponse> ManejarAsync(RefreshRequest request, CancellationToken ct)
    {
        var hash = tokenService.HashRefreshToken(request.RefreshToken);

        // RefreshToken no tiene filtro global por empresa; se busca directo por su hash.
        var token = await db.RefreshTokens
            .SingleOrDefaultAsync(t => t.TokenHash == hash, ct);

        // Token inexistente, revocado (reutilización) o expirado → mismo 401.
        if (token is null || token.RevocadoEn is not null || token.Expira <= clock.UtcAhora)
        {
            throw new UnauthorizedAppException("INVALID_REFRESH_TOKEN", "La sesión venció. Inicia sesión de nuevo.");
        }

        // El endpoint es público (sin sesión): se ignora el filtro global por empresa.
        var usuario = await db.Usuarios
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(u => u.UsuarioId == token.UsuarioId, ct);

        if (usuario is null || !usuario.Activo)
        {
            throw new UnauthorizedAppException("INVALID_REFRESH_TOKEN", "La sesión venció. Inicia sesión de nuevo.");
        }

        // Rotación: el token anterior queda revocado y se emite uno nuevo vinculado a él.
        token.RevocadoEn = clock.UtcAhora;

        var tokens = await tokenService.RotarAsync(usuario, token.RefreshTokenId, ct);

        return new AuthResponse(
            tokens.AccessToken,
            tokens.RefreshToken,
            "Bearer",
            UsuarioDto.Desde(usuario));
    }
}
