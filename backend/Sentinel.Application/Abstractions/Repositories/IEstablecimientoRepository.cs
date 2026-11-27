using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions.Repositories;

public interface IEstablecimientoRepository
{
    //filtrado por empresa y sin lo dado de baja
    Task<int> CountAsync(CancellationToken ct);
    Task<IEnumerable<Establecimiento>> GetPagedAsync(int pagina, int tamanoPagina, CancellationToken ct);
    void Agregar(Establecimiento establecimiento);

    //null si no existe, está dado de baja o es de otra empresa
    Task<Establecimiento?> ObtenerAsync(Guid establecimientoId, CancellationToken ct);
    //con tracking, para modificarlo. null si no existe, está dado de baja o es de otra empresa.
    Task<Establecimiento?> ObtenerParaModificarAsync(Guid establecimientoId, CancellationToken ct);
    Task<IEnumerable<Establecimiento>> GetByIdsAsync(IReadOnlyList<Guid> establecimientoIds, CancellationToken ct);
}
