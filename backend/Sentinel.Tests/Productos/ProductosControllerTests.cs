using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Features.Productos.DTOs;
using Sentinel.Domain.Constantes;

namespace Sentinel.Tests.Productos;

public class ProductosControllerTests : ProductosTestBase
{
    [Fact]
    public async Task Crear_devuelve_201_y_registro_no_verificado()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearProductosController(db);

        var request = new CrearProductoRequest
        {
            Nombre = "Paracetamol 500 mg",
            CategoriaProductoId = CatalogoIds.CategoriasProducto.Medicamento
        };

        var result = await controller.Crear(request, default);

        var created = Assert.IsType<CreatedResult>(result);
        var response = Assert.IsType<CrearProductoResponse>(created.Value);

        Assert.NotNull(response.Producto.Registro);
        Assert.Equal("NoVerificado", response.Producto.Registro!.Estado);
        Assert.Null(response.Producto.Registro.Numero);
        Assert.Null(response.Mensaje);

        var producto = await db.Productos.SingleAsync();
        Assert.Equal(
            CatalogoIds.TiposDocumento.RegistroSanitarioProducto,
            producto.TipoDocumentoId);
        Assert.Equal("Farmacia Demo", producto.Fabricante);
        Assert.Equal(1, await db.RegistrosSanitarios.CountAsync());
        Assert.Equal(0, await db.ChecklistRequisitos.CountAsync());
    }

    [Fact]
    public async Task Crear_sin_regla_devuelve_201_con_mensaje_y_sin_registro()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearProductosController(db);

        var request = new CrearProductoRequest
        {
            Nombre = "Crema hidratante",
            CategoriaProductoId = CatalogoIds.CategoriasProducto.Cosmetico
        };

        var result = await controller.Crear(request, default);

        var created = Assert.IsType<CreatedResult>(result);
        var response = Assert.IsType<CrearProductoResponse>(created.Value);

        Assert.Null(response.Producto.Registro);
        Assert.NotNull(response.Mensaje);
        Assert.Equal(0, await db.RegistrosSanitarios.CountAsync());
        Assert.Equal(0, await db.ChecklistRequisitos.CountAsync());
    }

    [Fact]
    public async Task Crear_con_categoria_inexistente_devuelve_400()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearProductosController(db);

        var request = new CrearProductoRequest
        {
            Nombre = "Producto raro",
            CategoriaProductoId = 999
        };

        var result = await controller.Crear(request, default);

        var bad = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(bad.Value);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task Crear_con_nombre_vacio_devuelve_400()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearProductosController(db);

        var request = new CrearProductoRequest
        {
            Nombre = string.Empty,
            CategoriaProductoId = CatalogoIds.CategoriasProducto.Medicamento
        };

        var result = await controller.Crear(request, default);

        var bad = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(bad.Value);
        Assert.Equal(400, problem.Status);
    }
}
