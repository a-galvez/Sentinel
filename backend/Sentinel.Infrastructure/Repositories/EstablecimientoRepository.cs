using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Infrastructure.Repositories;

public sealed class EstablecimientoRepository(AppDbContext db) : IEstablecimientoRepository
{
    public Task<int> CountAsync(CancellationToken ct)
    => db.Establecimientos.CountAsync(ct);

    public async Task<IEnumerable<Establecimiento>> GetPagedAsync(int pagina, int tamanoPagina, CancellationToken ct)
        => await db.Establecimientos
            .AsNoTracking()
            .OrderBy(e => e.Nombre)
            .ThenBy(e => e.EstablecimientoId)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(ct);

    public void Agregar(Establecimiento establecimiento)
    => db.Establecimientos.Add(establecimiento);

    public Task<Establecimiento?> ObtenerAsync(Guid establecimientoId, CancellationToken ct)
    => db.Establecimientos
        .AsNoTracking()
        .SingleOrDefaultAsync(e => e.EstablecimientoId == establecimientoId, ct);

    public Task<Establecimiento?> ObtenerParaModificarAsync(Guid establecimientoId, CancellationToken ct)
    => db.Establecimientos
        .SingleOrDefaultAsync(e => e.EstablecimientoId == establecimientoId, ct);

    public async Task<IEnumerable<Establecimiento>> GetByIdsAsync(IReadOnlyList<Guid> establecimientoIds, CancellationToken ct)
    {
        if (establecimientoIds.Count == 0) return [];

        var ids = establecimientoIds.ToList();
        return await db.Establecimientos
            .AsNoTracking()
            .Where(e => ids.Contains(e.EstablecimientoId))
            .ToListAsync(ct);
    }
}
