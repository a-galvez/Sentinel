
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Infrastructure.Repositories;

public sealed class ProductoRepository(AppDbContext db) : IProductoRepository
{
    public Task<Producto?> ObtenerAsync(Guid productoId, CancellationToken ct)
        => db.Productos.SingleOrDefaultAsync(p => p.ProductoId == productoId, ct);
}
