using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions;

public interface ITokenService
{
    // Login: emite un access token y un refresh token nuevo (sin vincular a uno anterior).
    Task<AuthTokens> EmitirAsync(Usuario usuario, CancellationToken ct);

    // Refresh con rotación: emite access token y un refresh token nuevo vinculado a `reemplazaA`.
    Task<AuthTokens> RotarAsync(Usuario usuario, Guid reemplazaA, CancellationToken ct);

    // Hash determinístico del refresh token en claro, para buscarlo por su hash almacenado.
    string HashRefreshToken(string refreshToken);
}
