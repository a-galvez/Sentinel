using Sentinel.Domain.Entities;

namespace Sentinel.Application.Interfaces;

// Solo la operación base. La escritura de tokens
// la agrega el encargado de autenticación.
public interface IRefreshTokenRepository
{
    Task<RefreshToken?> ObtenerPorHashAsync(string hash, CancellationToken ct);
}
