using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Infrastructure.Repositories;

public sealed class RegistroSanitarioRepository(AppDbContext db) : IRegistroSanitarioRepository
{
    public Task<RegistroSanitario?> ObtenerAsync(Guid registroSanitarioId, CancellationToken ct)
        => db.RegistrosSanitarios
            .SingleOrDefaultAsync(r => r.RegistroSanitarioId == registroSanitarioId, ct);

    public Task<RegistroSanitario?> ObtenerPorProductoAsync(Guid productoId, CancellationToken ct)
        => (from r in db.RegistrosSanitarios
            join p in db.Productos on r.ProductoId equals p.ProductoId
            where r.ProductoId == productoId
            select r)
           .SingleOrDefaultAsync(ct);

    public Task<bool> ExisteNumeroAsync(
        Guid empresaId,
        string numero,
        Guid? excluirRegistroSanitarioId,
        CancellationToken ct)
        => db.RegistrosSanitarios
            .IgnoreQueryFilters()
            .AnyAsync(r => r.EmpresaId == empresaId
                           && r.NumeroRegistro == numero
                           && (excluirRegistroSanitarioId == null
                               || r.RegistroSanitarioId != excluirRegistroSanitarioId), ct);

    public void Agregar(RegistroSanitario registro)
        => db.RegistrosSanitarios.Add(registro);

    public async Task<List<RegistroSanitario>> ObtenerPorProductosAsync(
        IReadOnlyCollection<Guid> productoIds,
        CancellationToken ct)
    {
        if (productoIds.Count == 0)
            return new List<RegistroSanitario>();

        var ids = productoIds.ToList();

        return await db.RegistrosSanitarios
            .Where(r => ids.Contains(r.ProductoId))
            .ToListAsync(ct);
    }

}
