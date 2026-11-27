using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Features.Productos.DTOs;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Enums;

namespace Sentinel.Tests.Productos;

public class ProductoEdicionTests : ProductosTestBase
{
    [Fact]
    public async Task Editar_cambia_el_nombre_y_devuelve_200()
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

        var result = await controller.Editar(
            producto.ProductoId,
            new EditarProductoRequest { Nombre = "  Paracetamol 500 mg  " },
            default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<ProductoDto>(ok.Value);

        Assert.Equal("Paracetamol 500 mg", dto.Nombre);
        Assert.NotNull(dto.Registro);
        Assert.Equal("RS-001", dto.Registro!.Numero);
    }

    [Fact]
    public async Task Editar_con_la_misma_categoria_devuelve_200()
    {
        await using var db = await CrearBaseAsync();
        var producto = await SembrarProductoAsync(db, "Paracetamol", EstadoVencimiento.Vigente);

        var controller = CrearProductosController(db);

        var result = await controller.Editar(
            producto.ProductoId,
            new EditarProductoRequest
            {
                CategoriaProductoId = CatalogoIds.CategoriasProducto.Medicamento
            },
            default);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Editar_categoria_distinta_devuelve_400()
    {
        await using var db = await CrearBaseAsync();
        var producto = await SembrarProductoAsync(db, "Paracetamol", EstadoVencimiento.Vigente);

        var controller = CrearProductosController(db);

        var result = await controller.Editar(
            producto.ProductoId,
            new EditarProductoRequest
            {
                CategoriaProductoId = CatalogoIds.CategoriasProducto.Cosmetico
            },
            default);

        var bad = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(bad.Value);
        var errors = Assert.IsType<Dictionary<string, string[]>>(
            problem.Extensions["errors"]);
        Assert.Contains("categoriaProductoId", errors.Keys);
    }

    [Fact]
    public async Task Editar_sin_campos_devuelve_400()
    {
        await using var db = await CrearBaseAsync();
        var producto = await SembrarProductoAsync(db, "Paracetamol", EstadoVencimiento.Vigente);

        var controller = CrearProductosController(db);

        var result = await controller.Editar(
            producto.ProductoId,
            new EditarProductoRequest(),
            default);

        var bad = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);
    }

    [Fact]
    public async Task Editar_producto_inexistente_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearProductosController(db);

        var result = await controller.Editar(
            Guid.NewGuid(),
            new EditarProductoRequest { Nombre = "Nuevo" },
            default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task Editar_producto_dado_de_baja_devuelve_404()
    {
        await using var db = await CrearBaseAsync();

        var producto = await SembrarProductoAsync(
            db, "Baja", EstadoVencimiento.Vigente, dadoDeBaja: true);

        var controller = CrearProductosController(db);

        var result = await controller.Editar(
            producto.ProductoId,
            new EditarProductoRequest { Nombre = "Nuevo" },
            default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }
}
