
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Infrastructure.Repositories;

public sealed class ChecklistRepository(AppDbContext db) : IChecklistRepository
{
    public Task<ChecklistRequisito?> ObtenerPorIdAsync(Guid checklistRequisitoId, CancellationToken ct)
        => db.ChecklistRequisitos
            .SingleOrDefaultAsync(c => c.ChecklistRequisitoId == checklistRequisitoId, ct);

    // Proyección fila + requisito en una sola consulta (nada de N+1).
    // El orden final por `Orden` del catálogo lo aplica el llamador con OrdenesDeDocumentoAsync.
    public async Task<List<FilaConRequisito>> FilasDeProductoAsync(Guid productoId, CancellationToken ct)
        => await db.ChecklistRequisitos
            .Where(c => c.ProductoId == productoId)
            .Join(db.Requisitos, c => c.RequisitoId, r => r.RequisitoId, (c, r) => new FilaConRequisito(c, r))
            .OrderBy(x => x.Fila.TipoTramiteId)
            .ThenBy(x => x.Fila.RequisitoId)
            .ToListAsync(ct);

    public async Task<List<FilaConRequisito>> FilasDeLicenciaAsync(Guid licenciaId, CancellationToken ct)
        => await db.ChecklistRequisitos
            .Where(c => c.LicenciaSanitariaId == licenciaId)
            .Join(db.Requisitos, c => c.RequisitoId, r => r.RequisitoId, (c, r) => new FilaConRequisito(c, r))
            .OrderBy(x => x.Fila.TipoTramiteId)
            .ThenBy(x => x.Fila.RequisitoId)
            .ToListAsync(ct);

    public Task<bool> ExisteParaProductoAsync(Guid productoId, CancellationToken ct)
        => db.ChecklistRequisitos.AnyAsync(c => c.ProductoId == productoId, ct);

    public Task<bool> ExisteParaLicenciaAsync(Guid licenciaId, CancellationToken ct)
        => db.ChecklistRequisitos.AnyAsync(c => c.LicenciaSanitariaId == licenciaId, ct);

    public void Agregar(ChecklistRequisito fila) => db.ChecklistRequisitos.Add(fila);

    public async Task<HashSet<Guid>> LicenciaIdsConChecklistAsync(IReadOnlyList<Guid> licenciaIds, CancellationToken ct)
    {
        if (licenciaIds.Count == 0) return [];

        var ids = licenciaIds.ToList();
        var conChecklist = await db.ChecklistRequisitos
            .Where(c => c.LicenciaSanitariaId != null && ids.Contains(c.LicenciaSanitariaId.Value))
            .Select(c => c.LicenciaSanitariaId!.Value)
            .Distinct()
            .ToListAsync(ct);

        return [.. conChecklist];
    }
}
