using Sentinel.Domain.Entities;


namespace Sentinel.Application.Abstractions.Repositories;


public interface ICatalogoRepository
{
    // --- Lecturas ---
    Task<List<CategoriaProducto>> CategoriasProductoAsync(CancellationToken ct);
    Task<List<TipoEstablecimiento>> TiposEstablecimientoAsync(CancellationToken ct);
    Task<List<TipoTramite>> TiposTramiteAsync(CancellationToken ct);
    Task<List<AmbitoDocumento>> AmbitosDocumentoAsync(CancellationToken ct);
    Task<List<TipoRequisito>> TiposRequisitoAsync(CancellationToken ct);

    Task<bool> ExisteCategoriaProductoAsync(int id, CancellationToken ct);
    Task<bool> ExisteTipoEstablecimientoAsync(int id, CancellationToken ct);
    Task<bool> ExisteTipoTramiteAsync(int id, CancellationToken ct);
    Task<bool> ExisteAmbitoDocumentoAsync(int id, CancellationToken ct);
    Task<bool> ExisteTipoRequisitoAsync(int id, CancellationToken ct);

    // --- Escrituras (sólo para el seeder) ---
    void AgregarCategoriaProducto(CategoriaProducto entidad);
    void AgregarTipoEstablecimiento(TipoEstablecimiento entidad);
    void AgregarTipoTramite(TipoTramite entidad);
    void AgregarAmbitoDocumento(AmbitoDocumento entidad);
    void AgregarTipoRequisito(TipoRequisito entidad);
}
