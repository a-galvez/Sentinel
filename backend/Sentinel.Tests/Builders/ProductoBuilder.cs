using Sentinel.Domain.Entities;

namespace Sentinel.Tests.Builders;

public static class ProductoBuilder
{
    public static Producto Crear(
        Guid empresaId,
        Guid? productoId = null,
        int categoriaProductoId = 1,
        string nombre = "Producto Demo",
        string fabricante = "Fabricante Demo")
        => new()
        {
            ProductoId = productoId ?? Guid.NewGuid(),
            EmpresaId = empresaId,
            CategoriaProductoId = categoriaProductoId,
            Nombre = nombre,
            Fabricante = fabricante,
            DadoDeBaja = false
        };
}
