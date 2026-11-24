
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Infrastructure.Repositories;

public sealed class RefreshTokenRepository(AppDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> ObtenerPorHashAsync(string hash, CancellationToken ct)
        => db.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == hash, ct);

    public void Agregar(RefreshToken token)
        => db.RefreshTokens.Add(token);
}
