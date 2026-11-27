using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Infrastructure.Repositories;

public class ProductoRepository(AppDbContext db) : IProductoRepository
{
    public Task<Producto?> ObtenerAsync(Guid productoId, CancellationToken ct)
        => db.Productos.SingleOrDefaultAsync(p => p.ProductoId == productoId, ct);

    public Task<int?> ElegirTipoDocumentoAsync(
        int categoriaProductoId,
        CancellationToken ct)
        => db.ReglasAplicabilidad
            .Where(r => r.TipoEstablecimientoId == null
                        && (r.CategoriaProductoId == null
                            || r.CategoriaProductoId == categoriaProductoId))
            .OrderBy(r => r.ReglaAplicabilidadId)
            .Select(r => (int?)r.TipoDocumentoId)
            .FirstOrDefaultAsync(ct);

    public void Agregar(Producto producto) => db.Productos.Add(producto);
}
