
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Infrastructure.Repositories;

public sealed class CatalogoRepository(AppDbContext db) : ICatalogoRepository
{
    // ---------- Lecturas ----------

    public Task<List<CategoriaProducto>> CategoriasProductoAsync(CancellationToken ct)
        => db.CategoriasProducto.OrderBy(x => x.CategoriaProductoId).ToListAsync(ct);

    public Task<List<TipoEstablecimiento>> TiposEstablecimientoAsync(CancellationToken ct)
        => db.TiposEstablecimiento.OrderBy(x => x.TipoEstablecimientoId).ToListAsync(ct);

    public Task<List<TipoTramite>> TiposTramiteAsync(CancellationToken ct)
        => db.TiposTramite.OrderBy(x => x.TipoTramiteId).ToListAsync(ct);

    public Task<List<AmbitoDocumento>> AmbitosDocumentoAsync(CancellationToken ct)
        => db.AmbitosDocumento.OrderBy(x => x.AmbitoDocumentoId).ToListAsync(ct);

    public Task<List<TipoRequisito>> TiposRequisitoAsync(CancellationToken ct)
        => db.TiposRequisito.OrderBy(x => x.TipoRequisitoId).ToListAsync(ct);

    // ---------- Existencias ----------

    public Task<bool> ExisteCategoriaProductoAsync(int id, CancellationToken ct)
        => db.CategoriasProducto.AnyAsync(x => x.CategoriaProductoId == id, ct);

    public Task<bool> ExisteTipoEstablecimientoAsync(int id, CancellationToken ct)
        => db.TiposEstablecimiento.AnyAsync(x => x.TipoEstablecimientoId == id, ct);

    public Task<bool> ExisteTipoTramiteAsync(int id, CancellationToken ct)
        => db.TiposTramite.AnyAsync(x => x.TipoTramiteId == id, ct);

    public Task<bool> ExisteAmbitoDocumentoAsync(int id, CancellationToken ct)
        => db.AmbitosDocumento.AnyAsync(x => x.AmbitoDocumentoId == id, ct);

    public Task<bool> ExisteTipoRequisitoAsync(int id, CancellationToken ct)
        => db.TiposRequisito.AnyAsync(x => x.TipoRequisitoId == id, ct);

    // ---------- Escrituras ----------
    // No llaman SaveChanges: dejan la entidad en el ChangeTracker. El seeder hace un solo
    // SaveChangesAsync al final (vía IUnitOfWork) para que todo sea transaccional.

    public void AgregarCategoriaProducto(CategoriaProducto entidad) => db.CategoriasProducto.Add(entidad);
    public void AgregarTipoEstablecimiento(TipoEstablecimiento entidad) => db.TiposEstablecimiento.Add(entidad);
    public void AgregarTipoTramite(TipoTramite entidad) => db.TiposTramite.Add(entidad);
    public void AgregarAmbitoDocumento(AmbitoDocumento entidad) => db.AmbitosDocumento.Add(entidad);
    public void AgregarTipoRequisito(TipoRequisito entidad) => db.TiposRequisito.Add(entidad);
}
