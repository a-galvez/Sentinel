
using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions.Repositories;

public interface IProductoRepository
{
    Task<Producto?> ObtenerAsync(Guid productoId, CancellationToken ct);
}
