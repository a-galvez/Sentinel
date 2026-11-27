using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions.Repositories;

public interface IProductoRepository
{
    Task<Producto?> ObtenerAsync(Guid productoId, CancellationToken ct);
    Task<int?> ElegirTipoDocumentoAsync(int categoriaProductoId, CancellationToken ct);
    void Agregar(Producto producto);
}
