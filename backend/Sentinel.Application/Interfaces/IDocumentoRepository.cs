using Sentinel.Domain.Entities;

namespace Sentinel.Application.Interfaces;

// Solo la operación base. El resto de métodos (Requisito, DocumentoRequisito,
// DocumentoDependencia y ReglaAplicabilidad) los agrega el encargado de documentos.
public interface IDocumentoRepository
{
    Task<TipoDocumento?> ObtenerTipoDocumentoAsync(int id, CancellationToken ct);
}
