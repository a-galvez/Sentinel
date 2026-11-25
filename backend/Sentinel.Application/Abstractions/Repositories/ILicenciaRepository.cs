
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Abstractions.Repositories;

public interface ILicenciaRepository
{
    Task<LicenciaSanitaria?> ObtenerAsync(Guid licenciaId, CancellationToken ct);

    Task<int?> TipoEstablecimientoDeLicenciaAsync(Guid licenciaId, CancellationToken ct);

    Task<List<int>> TiposVigentesEnEmpresaAsync(Guid empresaId, IReadOnlyList<int> tiposDocumento, CancellationToken ct);

    Task<IEnumerable<LicenciaSanitaria>> GetByEstablecimientosAsync(IReadOnlyList<Guid> establecimientoIds, CancellationToken ct);
    void Agregar(LicenciaSanitaria licencia);

    //null si el establecimiento no tiene licencia o fue dada de baja
    Task<LicenciaSanitaria?> ObtenerPorEstablecimientoAsync(Guid establecimientoId, CancellationToken ct);
    //con tracking, para modificarla. null si no tiene licencia o ya fue dada de baja.
    Task<LicenciaSanitaria?> ObtenerPorEstablecimientoParaModificarAsync(Guid establecimientoId, CancellationToken ct);
    //filtrado por empresa y sin lo dado de baja
    //solo licencias con documento asignado y cuyo establecimiento sigue activo
    Task<int> CountAsync(EstadoVencimiento? estado, DateOnly hoy, CancellationToken ct);
    Task<IEnumerable<LicenciaSanitaria>> GetPagedAsync(EstadoVencimiento? estado, DateOnly hoy, int pagina, int tamanoPagina, CancellationToken ct);
}
