using Sentinel.Domain.Entities;

namespace Sentinel.Application.Interfaces;

// Solo la operación base. Las demás consultas de producto
// las agrega el encargado de productos.
public interface IProductoRepository
{
    Task<Producto?> ObtenerAsync(Guid productoId, CancellationToken ct);
}
