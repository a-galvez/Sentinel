using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Features.Productos.DTOs;
using Sentinel.Domain.Enums;
using Sentinel.Tests.Productos;

namespace Sentinel.Tests.RegistrosSanitarios;

public class RegistroBajaTests : ProductosTestBase
{
    [Fact]
    public async Task DarDeBaja_registro_devuelve_204_y_marca_el_registro()
    {
        await using var db = await CrearBaseAsync();
        var (producto, registro) = await SembrarProductoConRegistroAsync(db);

        var controller = CrearRegistrosController(db);

        var result = await controller.DarDeBaja(producto.ProductoId, default);

        Assert.IsType<NoContentResult>(result);

        var guardado = await db.RegistrosSanitarios
            .IgnoreQueryFilters()
            .SingleAsync(r => r.RegistroSanitarioId == registro.RegistroSanitarioId);

        Assert.True(guardado.DadoDeBaja);
    }

    [Fact]
    public async Task DarDeBaja_registro_no_da_de_baja_el_producto()
    {
        await using var db = await CrearBaseAsync();
        var (producto, _) = await SembrarProductoConRegistroAsync(db);

        var registros = CrearRegistrosController(db);
        await registros.DarDeBaja(producto.ProductoId, default);

        var productoGuardado = await db.Productos
            .IgnoreQueryFilters()
            .SingleAsync(p => p.ProductoId == producto.ProductoId);

        Assert.False(productoGuardado.DadoDeBaja);

        var productos = CrearProductosController(db);
        var result = await productos.Listar(new ProductoListadoQuery(), default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var pagina = Assert.IsType<ProductoPaginaDto>(ok.Value);

        Assert.Equal(1, pagina.Total);
        Assert.Null(pagina.Items[0].Registro);
    }

    [Fact]
    public async Task DarDeBaja_registro_lo_saca_del_detalle()
    {
        await using var db = await CrearBaseAsync();
        var (producto, _) = await SembrarProductoConRegistroAsync(db);

        var registros = CrearRegistrosController(db);
        await registros.DarDeBaja(producto.ProductoId, default);

        var productos = CrearProductosController(db);
        var result = await productos.Obtener(producto.ProductoId, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<ProductoDto>(ok.Value);

        Assert.Null(dto.Registro);
    }

    [Fact]
    public async Task DarDeBaja_registro_producto_sin_registro_devuelve_404()
    {
        await using var db = await CrearBaseAsync();

        var producto = ProductoMedicamento();
        db.Productos.Add(producto);
        await db.SaveChangesAsync(default);

        var controller = CrearRegistrosController(db);

        var result = await controller.DarDeBaja(producto.ProductoId, default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task DarDeBaja_registro_producto_inexistente_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearRegistrosController(db);

        var result = await controller.DarDeBaja(Guid.NewGuid(), default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task DarDeBaja_registro_dos_veces_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var (producto, _) = await SembrarProductoConRegistroAsync(db);

        var controller = CrearRegistrosController(db);

        await controller.DarDeBaja(producto.ProductoId, default);

        var result = await controller.DarDeBaja(producto.ProductoId, default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }
}
