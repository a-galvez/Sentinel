using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Domain.Entities;

namespace Sentinel.Tests.Catalogos;

internal sealed class FakeCatalogoRepository : ICatalogoRepository
{
    public List<CategoriaProducto> Categorias { get; } = [];
    public List<TipoEstablecimiento> TiposEstablecimiento { get; } = [];
    public List<TipoTramite> TiposTramite { get; } = [];
    public List<AmbitoDocumento> AmbitosDocumento { get; } = [];
    public List<TipoRequisito> TiposRequisito { get; } = [];

    public Task<List<CategoriaProducto>> CategoriasProductoAsync(CancellationToken ct)
        => Task.FromResult(Categorias);

    public Task<List<TipoEstablecimiento>> TiposEstablecimientoAsync(CancellationToken ct)
        => Task.FromResult(TiposEstablecimiento);

    public Task<List<TipoTramite>> TiposTramiteAsync(CancellationToken ct)
        => Task.FromResult(TiposTramite);

    public Task<List<AmbitoDocumento>> AmbitosDocumentoAsync(CancellationToken ct)
        => Task.FromResult(AmbitosDocumento);

    public Task<List<TipoRequisito>> TiposRequisitoAsync(CancellationToken ct)
        => Task.FromResult(TiposRequisito);

    public Task<bool> ExisteCategoriaProductoAsync(int id, CancellationToken ct)
        => Task.FromResult(Categorias.Any(c => c.CategoriaProductoId == id));

    public Task<bool> ExisteTipoEstablecimientoAsync(int id, CancellationToken ct)
        => Task.FromResult(TiposEstablecimiento.Any(t => t.TipoEstablecimientoId == id));

    public Task<bool> ExisteTipoTramiteAsync(int id, CancellationToken ct)
        => Task.FromResult(TiposTramite.Any(t => t.TipoTramiteId == id));

    public Task<bool> ExisteAmbitoDocumentoAsync(int id, CancellationToken ct)
        => Task.FromResult(AmbitosDocumento.Any(a => a.AmbitoDocumentoId == id));

    public Task<bool> ExisteTipoRequisitoAsync(int id, CancellationToken ct)
        => Task.FromResult(TiposRequisito.Any(t => t.TipoRequisitoId == id));

    public void AgregarCategoriaProducto(CategoriaProducto entidad) => Categorias.Add(entidad);

    public void AgregarTipoEstablecimiento(TipoEstablecimiento entidad) => TiposEstablecimiento.Add(entidad);

    public void AgregarTipoTramite(TipoTramite entidad) => TiposTramite.Add(entidad);

    public void AgregarAmbitoDocumento(AmbitoDocumento entidad) => AmbitosDocumento.Add(entidad);

    public void AgregarTipoRequisito(TipoRequisito entidad) => TiposRequisito.Add(entidad);
}
