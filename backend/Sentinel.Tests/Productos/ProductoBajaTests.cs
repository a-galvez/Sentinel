using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Features.Productos.DTOs;
using Sentinel.Domain.Enums;

namespace Sentinel.Tests.Productos;

public class ProductoBajaTests : ProductosTestBase
{
    [Fact]
    public async Task DarDeBaja_devuelve_204_y_marca_el_producto()
    {
        await using var db = await CrearBaseAsync();

        var producto = await SembrarProductoAsync(
            db,
            "Paracetamol",
            EstadoVencimiento.Vigente,
            numero: "RS-001",
            fechaEmision: Hoy,
            fechaVencimiento: Hoy.AddMonths(60));

        var controller = CrearProductosController(db);

        var result = await controller.DarDeBaja(producto.ProductoId, default);

        Assert.IsType<NoContentResult>(result);

        var guardado = await db.Productos
            .IgnoreQueryFilters()
            .SingleAsync(p => p.ProductoId == producto.ProductoId);

        Assert.True(guardado.DadoDeBaja);
    }

    [Fact]
    public async Task DarDeBaja_desaparece_del_listado()
    {
        await using var db = await CrearBaseAsync();

        var producto = await SembrarProductoAsync(
            db,
            "Paracetamol",
            EstadoVencimiento.Vigente,
            numero: "RS-001",
            fechaEmision: Hoy,
            fechaVencimiento: Hoy.AddMonths(60));

        var controller = CrearProductosController(db);

        await controller.DarDeBaja(producto.ProductoId, default);

        var result = await controller.Listar(new ProductoListadoQuery(), default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var pagina = Assert.IsType<ProductoPaginaDto>(ok.Value);

        Assert.Equal(0, pagina.Total);
        Assert.Empty(pagina.Items);
    }

    [Fact]
    public async Task DarDeBaja_ya_no_se_puede_obtener()
    {
        await using var db = await CrearBaseAsync();
        var producto = await SembrarProductoAsync(db, "Paracetamol", EstadoVencimiento.Vigente);

        var controller = CrearProductosController(db);

        await controller.DarDeBaja(producto.ProductoId, default);

        var result = await controller.Obtener(producto.ProductoId, default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task DarDeBaja_producto_inexistente_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearProductosController(db);

        var result = await controller.DarDeBaja(Guid.NewGuid(), default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task DarDeBaja_producto_de_otra_empresa_devuelve_404()
    {
        await using var db = await CrearBaseAsync();

        var producto = await SembrarProductoAsync(
            db,
            "Ajeno",
            EstadoVencimiento.Vigente,
            numero: "RS-999",
            fechaEmision: Hoy,
            fechaVencimiento: Hoy.AddMonths(60),
            empresaId: Guid.NewGuid());

        var controller = CrearProductosController(db);

        var result = await controller.DarDeBaja(producto.ProductoId, default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task DarDeBaja_dos_veces_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var producto = await SembrarProductoAsync(db, "Paracetamol", EstadoVencimiento.Vigente);

        var controller = CrearProductosController(db);

        await controller.DarDeBaja(producto.ProductoId, default);

        var result = await controller.DarDeBaja(producto.ProductoId, default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }
}
