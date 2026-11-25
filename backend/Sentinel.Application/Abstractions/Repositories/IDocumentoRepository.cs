
using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions.Repositories;

public interface IDocumentoRepository
{
    // --- TipoDocumento ---
    Task<TipoDocumento?> ObtenerTipoDocumentoAsync(int id, CancellationToken ct);
    Task<string?> NombreTipoDocumentoAsync(int id, CancellationToken ct);
    Task<bool> ExisteTipoDocumentoAsync(int id, CancellationToken ct);
    void AgregarTipoDocumento(TipoDocumento entidad);

    Task<List<TipoDocumento>> ObtenerTiposDocumentoAsync(IReadOnlyList<int> ids, CancellationToken ct);

    // --- Requisito ---
    Task<Requisito?> ObtenerRequisitoAsync(int id, CancellationToken ct);
    Task<bool> ExisteRequisitoAsync(int id, CancellationToken ct);
    void AgregarRequisito(Requisito entidad);

    // --- DocumentoRequisito (plantilla por documento + trámite) ---
    Task<bool> ExisteDocumentoRequisitoAsync(int tipoDocumentoId, int tipoTramiteId, int requisitoId, CancellationToken ct);
    Task<List<int>> RequisitosDeDocumentoAsync(int tipoDocumentoId, int tipoTramiteId, CancellationToken ct);
    Task<Dictionary<int, int>> OrdenesDeDocumentoAsync(int tipoDocumentoId, int tipoTramiteId, CancellationToken ct);
    void AgregarDocumentoRequisito(DocumentoRequisito entidad);

    // --- DocumentoDependencia ---
    Task<bool> ExisteDependenciaAsync(int tipoDocumentoId, int requiereTipoDocumentoId, CancellationToken ct);
    Task<List<int>> DependeDeAsync(int tipoDocumentoId, CancellationToken ct);
    void AgregarDependencia(DocumentoDependencia entidad);

    // --- ReglaAplicabilidad ---
    Task<bool> ExisteReglaAsync(int reglaId, CancellationToken ct);
    Task<int?> ElegirTipoDocumentoParaProductoAsync(int categoriaProductoId, CancellationToken ct);
    Task<int?> ElegirTipoDocumentoParaEstablecimientoAsync(int tipoEstablecimientoId, CancellationToken ct);
    void AgregarRegla(ReglaAplicabilidad entidad);
}
