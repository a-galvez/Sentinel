using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Abstractions.Repositories;

public interface IProductoRepository
{
    Task<Producto?> ObtenerAsync(Guid productoId, CancellationToken ct);

    Task<int?> ElegirTipoDocumentoAsync(
        int categoriaProductoId,
        CancellationToken ct);

    Task<(List<Producto> Items, int Total)> ListarAsync(
        string? q,
        EstadoVencimiento? estado,
        DateOnly hoy,
        int skip,
        int take,
        CancellationToken ct);

    Task<HashSet<Guid>> ProductosConChecklistAsync(
        IReadOnlyCollection<Guid> productoIds,
        CancellationToken ct);

    void Agregar(Producto producto);
}
