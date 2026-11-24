
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Infrastructure.Repositories;

public sealed class DocumentoRepository(AppDbContext db) : IDocumentoRepository
{
    // ---------- TipoDocumento ----------

    public Task<TipoDocumento?> ObtenerTipoDocumentoAsync(int id, CancellationToken ct)
        => db.TiposDocumento.SingleOrDefaultAsync(t => t.TipoDocumentoId == id, ct);

    public Task<string?> NombreTipoDocumentoAsync(int id, CancellationToken ct)
        => db.TiposDocumento
            .Where(t => t.TipoDocumentoId == id)
            .Select(t => (string?)t.Nombre)
            .SingleOrDefaultAsync(ct);

    public Task<bool> ExisteTipoDocumentoAsync(int id, CancellationToken ct)
        => db.TiposDocumento.AnyAsync(t => t.TipoDocumentoId == id, ct);

    public void AgregarTipoDocumento(TipoDocumento entidad) => db.TiposDocumento.Add(entidad);

    // ---------- Requisito ----------

    public Task<Requisito?> ObtenerRequisitoAsync(int id, CancellationToken ct)
        => db.Requisitos.SingleOrDefaultAsync(r => r.RequisitoId == id, ct);

    public Task<bool> ExisteRequisitoAsync(int id, CancellationToken ct)
        => db.Requisitos.AnyAsync(r => r.RequisitoId == id, ct);

    public void AgregarRequisito(Requisito entidad) => db.Requisitos.Add(entidad);

    // ---------- DocumentoRequisito ----------

    public Task<bool> ExisteDocumentoRequisitoAsync(int tipoDocumentoId, int tipoTramiteId, int requisitoId, CancellationToken ct)
        => db.DocumentosRequisito.AnyAsync(
            x => x.TipoDocumentoId == tipoDocumentoId
                 && x.TipoTramiteId == tipoTramiteId
                 && x.RequisitoId == requisitoId, ct);

    public Task<List<int>> RequisitosDeDocumentoAsync(int tipoDocumentoId, int tipoTramiteId, CancellationToken ct)
        => db.DocumentosRequisito
            .Where(d => d.TipoDocumentoId == tipoDocumentoId && d.TipoTramiteId == tipoTramiteId)
            .OrderBy(d => d.Orden)
            .Select(d => d.RequisitoId)
            .ToListAsync(ct);

    public Task<Dictionary<int, int>> OrdenesDeDocumentoAsync(int tipoDocumentoId, int tipoTramiteId, CancellationToken ct)
        => db.DocumentosRequisito
            .Where(d => d.TipoDocumentoId == tipoDocumentoId && d.TipoTramiteId == tipoTramiteId)
            .ToDictionaryAsync(d => d.RequisitoId, d => d.Orden, ct);

    public void AgregarDocumentoRequisito(DocumentoRequisito entidad) => db.DocumentosRequisito.Add(entidad);

    // ---------- DocumentoDependencia ----------

    public Task<bool> ExisteDependenciaAsync(int tipoDocumentoId, int requiereTipoDocumentoId, CancellationToken ct)
        => db.DocumentosDependencia.AnyAsync(
            x => x.TipoDocumentoId == tipoDocumentoId
                 && x.RequiereTipoDocumentoId == requiereTipoDocumentoId, ct);

    public Task<List<int>> DependeDeAsync(int tipoDocumentoId, CancellationToken ct)
        => db.DocumentosDependencia
            .Where(d => d.TipoDocumentoId == tipoDocumentoId)
            .Select(d => d.RequiereTipoDocumentoId)
            .ToListAsync(ct);

    public void AgregarDependencia(DocumentoDependencia entidad) => db.DocumentosDependencia.Add(entidad);

    // ---------- ReglaAplicabilidad ----------
    //
    // ⚠️ ESTAS DOS CONSULTAS TODAVÍA USAN LA ESTRUCTURA DEL PASO 1
    // (columnas nullable TipoEstablecimientoId/CategoriaProductoId, desempate por id).
    // En el Paso 2 (Prioridad + CondicionJson) se reescriben. Mantengo esta forma para que
    // el proyecto compile y los tests actuales sigan verdes hasta que llegue ese cambio.

    public Task<bool> ExisteReglaAsync(int reglaId, CancellationToken ct)
        => db.ReglasAplicabilidad.AnyAsync(r => r.ReglaAplicabilidadId == reglaId, ct);

    public Task<int?> ElegirTipoDocumentoParaProductoAsync(int categoriaProductoId, CancellationToken ct)
        => db.ReglasAplicabilidad
            .Where(r => r.CategoriaProductoId == null || r.CategoriaProductoId == categoriaProductoId)
            .OrderBy(r => r.ReglaAplicabilidadId)
            .Select(r => (int?)r.TipoDocumentoId)
            .FirstOrDefaultAsync(ct);

    public Task<int?> ElegirTipoDocumentoParaEstablecimientoAsync(int tipoEstablecimientoId, CancellationToken ct)
        => db.ReglasAplicabilidad
            .Where(r => r.TipoEstablecimientoId == null || r.TipoEstablecimientoId == tipoEstablecimientoId)
            .OrderBy(r => r.ReglaAplicabilidadId)
            .Select(r => (int?)r.TipoDocumentoId)
            .FirstOrDefaultAsync(ct);

    public void AgregarRegla(ReglaAplicabilidad entidad) => db.ReglasAplicabilidad.Add(entidad);
}
