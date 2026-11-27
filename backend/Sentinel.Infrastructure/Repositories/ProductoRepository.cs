using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;
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

    public async Task<(List<Producto> Items, int Total)> ListarAsync(
        string? q,
        EstadoVencimiento? estado,
        DateOnly hoy,
        int skip,
        int take,
        CancellationToken ct)
    {
        IQueryable<Producto> query = db.Productos;

        if (!string.IsNullOrWhiteSpace(q))
        {
            var texto = q.Trim().ToLower();

            query = query.Where(p =>
                p.Nombre.ToLower().Contains(texto)
                || db.RegistrosSanitarios.Any(r =>
                    r.ProductoId == p.ProductoId
                    && r.NumeroRegistro != null
                    && r.NumeroRegistro.ToLower().Contains(texto)));
        }

        if (estado is not null)
        {
            var hoy90 = hoy.AddDays(90);

            query = estado switch
            {
                EstadoVencimiento.NoVerificado => query.Where(p =>
                    db.RegistrosSanitarios.Any(r =>
                        r.ProductoId == p.ProductoId
                        && r.Estado == EstadoVencimiento.NoVerificado)),

                EstadoVencimiento.EnTramite => query.Where(p =>
                    db.RegistrosSanitarios.Any(r =>
                        r.ProductoId == p.ProductoId
                        && r.Estado == EstadoVencimiento.EnTramite)),

                EstadoVencimiento.Vencido => query.Where(p =>
                    db.RegistrosSanitarios.Any(r =>
                        r.ProductoId == p.ProductoId
                        && r.Estado != EstadoVencimiento.NoVerificado
                        && r.Estado != EstadoVencimiento.EnTramite
                        && r.FechaVencimiento.HasValue
                        && r.FechaVencimiento.Value < hoy)),

                EstadoVencimiento.PorVencer => query.Where(p =>
                    db.RegistrosSanitarios.Any(r =>
                        r.ProductoId == p.ProductoId
                        && r.Estado != EstadoVencimiento.NoVerificado
                        && r.Estado != EstadoVencimiento.EnTramite
                        && r.FechaVencimiento.HasValue
                        && r.FechaVencimiento.Value >= hoy
                        && r.FechaVencimiento.Value <= hoy90)),

                _ => query.Where(p =>
                    db.RegistrosSanitarios.Any(r =>
                        r.ProductoId == p.ProductoId
                        && r.Estado != EstadoVencimiento.NoVerificado
                        && r.Estado != EstadoVencimiento.EnTramite
                        && r.FechaVencimiento.HasValue
                        && r.FechaVencimiento.Value > hoy90))
            };
        }

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Nombre)
            .ThenBy(p => p.ProductoId)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<HashSet<Guid>> ProductosConChecklistAsync(
        IReadOnlyCollection<Guid> productoIds,
        CancellationToken ct)
    {
        if (productoIds.Count == 0)
            return new HashSet<Guid>();

        var ids = productoIds.ToList();

        var conChecklist = await db.ChecklistRequisitos
            .Where(c => c.ProductoId != null && ids.Contains(c.ProductoId.Value))
            .Select(c => c.ProductoId!.Value)
            .Distinct()
            .ToListAsync(ct);

        return conChecklist.ToHashSet();
    }

    public void Agregar(Producto producto) => db.Productos.Add(producto);
}
