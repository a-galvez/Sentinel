using Sentinel.Domain.Entities;

namespace Sentinel.Tests.Builders;

public static class ProductoBuilder
{
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(DateTime.UtcNow);

    public static Producto Crear(
        Guid empresaId,
        Guid? productoId = null,
        int categoriaProductoId = 1,
        string nombre = "Producto Demo",
        string fabricante = "Fabricante Demo",
        string? numeroRegistro = null,
        DateOnly? fechaEmision = null,
        DateOnly? fechaVencimiento = null)
        => new()
        {
            ProductoId = productoId ?? Guid.NewGuid(),
            EmpresaId = empresaId,
            CategoriaProductoId = categoriaProductoId,
            Nombre = nombre,
            Fabricante = fabricante,
            NumeroRegistro = numeroRegistro ?? $"REG-{Guid.NewGuid():N}",
            FechaEmision = fechaEmision ?? Hoy,
            FechaVencimiento = fechaVencimiento ?? Hoy.AddYears(1),
            DadoDeBaja = false
        };
}
