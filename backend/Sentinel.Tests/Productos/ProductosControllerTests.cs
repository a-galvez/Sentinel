using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Features.Productos.DTOs;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Enums;

namespace Sentinel.Tests.Productos;

public class ProductosControllerTests : ProductosTestBase
{
    [Fact]
    public async Task Crear_devuelve_201_y_registro_no_verificado()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearProductosController(db);

        var result = await controller.Crear(
            new CrearProductoRequest
            {
                Nombre = "Paracetamol 500 mg",
                CategoriaProductoId = CatalogoIds.CategoriasProducto.Medicamento
            },
            default);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal(201, created.StatusCode);

        var response = Assert.IsType<CrearProductoResponse>(created.Value);
        Assert.Null(response.Mensaje);
        Assert.Equal(
            $"/api/v1/productos/{response.Producto.ProductoId}",
            created.Location);

        var registro = response.Producto.Registro;
        Assert.NotNull(registro);
        Assert.Equal("NoVerificado", registro!.Estado);
        Assert.True(registro.RequiereVerificacion);
        Assert.False(registro.TieneChecklist);

        Assert.Equal(1, await db.RegistrosSanitarios.CountAsync());
        Assert.Equal(0, await db.ChecklistRequisitos.CountAsync());
    }

    [Fact]
    public async Task Crear_recorta_el_nombre()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearProductosController(db);

        var result = await controller.Crear(
            new CrearProductoRequest
            {
                Nombre = "  Ibuprofeno 400 mg  ",
                CategoriaProductoId = CatalogoIds.CategoriasProducto.Medicamento
            },
            default);

        var created = Assert.IsType<CreatedResult>(result);
        var response = Assert.IsType<CrearProductoResponse>(created.Value);

        Assert.Equal("Ibuprofeno 400 mg", response.Producto.Nombre);

        var guardado = await db.Productos.SingleAsync();
        Assert.Equal("Ibuprofeno 400 mg", guardado.Nombre);
    }

    [Fact]
    public async Task Crear_sin_regla_devuelve_201_con_mensaje_y_sin_registro()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearProductosController(db);

        var result = await controller.Crear(
            new CrearProductoRequest
            {
                Nombre = "Crema hidratante",
                CategoriaProductoId = CatalogoIds.CategoriasProducto.Cosmetico
            },
            default);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Equal(201, created.StatusCode);

        var response = Assert.IsType<CrearProductoResponse>(created.Value);
        Assert.NotNull(response.Mensaje);
        Assert.Null(response.Producto.Registro);

        Assert.Equal(0, await db.RegistrosSanitarios.CountAsync());
        Assert.Equal(0, await db.ChecklistRequisitos.CountAsync());
    }

    [Fact]
    public async Task Crear_categoria_inexistente_devuelve_400()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearProductosController(db);

        var result = await controller.Crear(
            new CrearProductoRequest
            {
                Nombre = "Producto raro",
                CategoriaProductoId = 99
            },
            default);

        var bad = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(bad.Value);
        var errors = Assert.IsType<Dictionary<string, string[]>>(
            problem.Extensions["errors"]);
        Assert.Contains("categoriaProductoId", errors.Keys);

        Assert.Equal(0, await db.Productos.CountAsync());
    }

    [Fact]
    public async Task Crear_sin_nombre_devuelve_400()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearProductosController(db);

        var result = await controller.Crear(
            new CrearProductoRequest
            {
                Nombre = "   ",
                CategoriaProductoId = CatalogoIds.CategoriasProducto.Medicamento
            },
            default);

        var bad = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);

        Assert.Equal(0, await db.Productos.CountAsync());
    }

    [Fact]
    public async Task Listar_devuelve_pagina_con_total()
    {
        await using var db = await CrearBaseAsync();

        await SembrarProductoAsync(db, "Paracetamol", EstadoVencimiento.Vigente,
            numero: "RS-001", fechaEmision: Hoy, fechaVencimiento: Hoy.AddMonths(60));
        await SembrarProductoAsync(db, "Ibuprofeno", EstadoVencimiento.PorVencer,
            numero: "RS-002", fechaEmision: Hoy, fechaVencimiento: Hoy.AddDays(30));
        await SembrarProductoAsync(db, "Loratadina", EstadoVencimiento.Vencido,
            numero: "RS-003", fechaEmision: Hoy.AddYears(-1), fechaVencimiento: Hoy.AddDays(-1));

        var controller = CrearProductosController(db);

        var result = await controller.Listar(
            new ProductoListadoQuery { Pagina = 1, TamanoPagina = 2 }, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var pagina = Assert.IsType<ProductoPaginaDto>(ok.Value);

        Assert.Equal(1, pagina.Pagina);
        Assert.Equal(2, pagina.TamanoPagina);
        Assert.Equal(3, pagina.Total);
        Assert.Equal(2, pagina.Items.Count);
    }

    [Fact]
    public async Task Listar_busca_por_nombre()
    {
        await using var db = await CrearBaseAsync();

        await SembrarProductoAsync(db, "Paracetamol", EstadoVencimiento.Vigente,
            numero: "RS-001", fechaEmision: Hoy, fechaVencimiento: Hoy.AddMonths(60));
        await SembrarProductoAsync(db, "Ibuprofeno", EstadoVencimiento.Vigente,
            numero: "RS-002", fechaEmision: Hoy, fechaVencimiento: Hoy.AddMonths(60));

        var controller = CrearProductosController(db);

        var result = await controller.Listar(
            new ProductoListadoQuery { Q = "para" }, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var pagina = Assert.IsType<ProductoPaginaDto>(ok.Value);

        Assert.Single(pagina.Items);
        Assert.Equal("Paracetamol", pagina.Items[0].Nombre);
    }

    [Fact]
    public async Task Listar_busca_por_numero_de_registro()
    {
        await using var db = await CrearBaseAsync();

        await SembrarProductoAsync(db, "Paracetamol", EstadoVencimiento.Vigente,
            numero: "RS-001", fechaEmision: Hoy, fechaVencimiento: Hoy.AddMonths(60));
        await SembrarProductoAsync(db, "Ibuprofeno", EstadoVencimiento.Vigente,
            numero: "RS-002", fechaEmision: Hoy, fechaVencimiento: Hoy.AddMonths(60));

        var controller = CrearProductosController(db);

        var result = await controller.Listar(
            new ProductoListadoQuery { Q = "RS-002" }, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var pagina = Assert.IsType<ProductoPaginaDto>(ok.Value);

        Assert.Single(pagina.Items);
        Assert.Equal("Ibuprofeno", pagina.Items[0].Nombre);
    }

    [Fact]
    public async Task Listar_filtra_por_estado()
    {
        await using var db = await CrearBaseAsync();

        await SembrarProductoAsync(db, "Vigente", EstadoVencimiento.Vigente,
            numero: "RS-001", fechaEmision: Hoy, fechaVencimiento: Hoy.AddMonths(60));
        await SembrarProductoAsync(db, "Vencido", EstadoVencimiento.Vencido,
            numero: "RS-002", fechaEmision: Hoy.AddYears(-1), fechaVencimiento: Hoy.AddDays(-1));
        await SembrarProductoAsync(db, "NoVerificado", EstadoVencimiento.NoVerificado);

        var controller = CrearProductosController(db);

        var result = await controller.Listar(
            new ProductoListadoQuery { Estado = "Vencido" }, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var pagina = Assert.IsType<ProductoPaginaDto>(ok.Value);

        Assert.Single(pagina.Items);
        Assert.Equal("Vencido", pagina.Items[0].Nombre);
    }

    [Fact]
    public async Task Listar_excluye_productos_dados_de_baja()
    {
        await using var db = await CrearBaseAsync();

        await SembrarProductoAsync(db, "Activo", EstadoVencimiento.Vigente,
            numero: "RS-001", fechaEmision: Hoy, fechaVencimiento: Hoy.AddMonths(60));
        await SembrarProductoAsync(db, "Baja", EstadoVencimiento.Vigente,
            numero: "RS-002", fechaEmision: Hoy, fechaVencimiento: Hoy.AddMonths(60),
            dadoDeBaja: true);

        var controller = CrearProductosController(db);

        var result = await controller.Listar(new ProductoListadoQuery(), default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var pagina = Assert.IsType<ProductoPaginaDto>(ok.Value);

        Assert.Equal(1, pagina.Total);
        Assert.Equal("Activo", pagina.Items[0].Nombre);
    }

    [Fact]
    public async Task Listar_respeta_la_empresa_de_la_sesion()
    {
        await using var db = await CrearBaseAsync();

        await SembrarProductoAsync(db, "Ajeno", EstadoVencimiento.Vigente,
            numero: "RS-999", fechaEmision: Hoy, fechaVencimiento: Hoy.AddMonths(60),
            empresaId: Guid.NewGuid());

        var controller = CrearProductosController(db);

        var result = await controller.Listar(new ProductoListadoQuery(), default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var pagina = Assert.IsType<ProductoPaginaDto>(ok.Value);

        Assert.Equal(0, pagina.Total);
    }

    [Fact]
    public async Task Obtener_devuelve_el_detalle_del_producto()
    {
        await using var db = await CrearBaseAsync();

        var producto = await SembrarProductoAsync(db, "Paracetamol",
            EstadoVencimiento.Vigente, numero: "RS-001",
            fechaEmision: Hoy, fechaVencimiento: Hoy.AddMonths(60));

        var controller = CrearProductosController(db);

        var result = await controller.Obtener(producto.ProductoId, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<ProductoDto>(ok.Value);

        Assert.Equal(producto.ProductoId, dto.ProductoId);
        Assert.NotNull(dto.Registro);
        Assert.Equal("Vigente", dto.Registro!.Estado);
    }

    [Fact]
    public async Task Obtener_producto_inexistente_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearProductosController(db);

        var result = await controller.Obtener(Guid.NewGuid(), default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task Obtener_producto_de_otra_empresa_devuelve_404()
    {
        await using var db = await CrearBaseAsync();

        var producto = await SembrarProductoAsync(db, "Ajeno",
            EstadoVencimiento.Vigente, numero: "RS-999",
            fechaEmision: Hoy, fechaVencimiento: Hoy.AddMonths(60),
            empresaId: Guid.NewGuid());

        var controller = CrearProductosController(db);

        var result = await controller.Obtener(producto.ProductoId, default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task Obtener_producto_dado_de_baja_devuelve_404()
    {
        await using var db = await CrearBaseAsync();

        var producto = await SembrarProductoAsync(db, "Baja",
            EstadoVencimiento.Vigente, numero: "RS-001",
            fechaEmision: Hoy, fechaVencimiento: Hoy.AddMonths(60),
            dadoDeBaja: true);

        var controller = CrearProductosController(db);

        var result = await controller.Obtener(producto.ProductoId, default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task Listar_con_estado_invalido_devuelve_400()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearProductosController(db);

        var result = await controller.Listar(
            new ProductoListadoQuery { Estado = "Foo" }, default);

        var bad = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);
    }

    [Fact]
    public async Task Listar_con_pagina_invalida_devuelve_400()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearProductosController(db);

        var result = await controller.Listar(
            new ProductoListadoQuery { Pagina = 0 }, default);

        var bad = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);
    }
}
