
using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> ObtenerPorHashAsync(string hash, CancellationToken ct);
    void Agregar(RefreshToken token);
}
