using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Features.Productos.DTOs;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;
using Sentinel.Tests.Productos;

namespace Sentinel.Tests.RegistrosSanitarios;

public class RegistroEmisionTests : ProductosTestBase
{
    [Fact]
    public async Task Emision_registra_numero_y_calcula_vencimiento()
    {
        await using var db = await CrearBaseAsync();
        var (producto, _) = await SembrarProductoConRegistroAsync(db);
        var controller = CrearRegistrosController(db);

        var result = await controller.RegistrarEmision(
            producto.ProductoId,
            new EmisionRequest { Numero = "RS-0001-EJ", FechaEmision = Hoy },
            default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<RegistroSanitarioDto>(ok.Value);

        Assert.Equal("RS-0001-EJ", dto.Numero);
        Assert.Equal(Hoy, dto.FechaEmision);
        Assert.Equal(Hoy.AddMonths(60), dto.FechaVencimiento);
        Assert.Equal("Vigente", dto.Estado);
        Assert.False(dto.RequiereVerificacion);
    }

    [Fact]
    public async Task Emision_trim_del_numero()
    {
        await using var db = await CrearBaseAsync();
        var (producto, _) = await SembrarProductoConRegistroAsync(db);
        var controller = CrearRegistrosController(db);

        var result = await controller.RegistrarEmision(
            producto.ProductoId,
            new EmisionRequest { Numero = "  RS-0002  ", FechaEmision = Hoy },
            default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<RegistroSanitarioDto>(ok.Value);

        Assert.Equal("RS-0002", dto.Numero);
    }

    [Fact]
    public async Task Emision_corrige_datos_existentes()
    {
        await using var db = await CrearBaseAsync();
        var (producto, _) = await SembrarProductoConRegistroAsync(db);
        var controller = CrearRegistrosController(db);

        await controller.RegistrarEmision(
            producto.ProductoId,
            new EmisionRequest { Numero = "RS-001", FechaEmision = Hoy },
            default);

        var result = await controller.RegistrarEmision(
            producto.ProductoId,
            new EmisionRequest
            {
                Numero = "RS-002",
                FechaEmision = Hoy.AddMonths(1)
            },
            default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<RegistroSanitarioDto>(ok.Value);

        Assert.Equal("RS-002", dto.Numero);
        Assert.Equal(Hoy.AddMonths(1), dto.FechaEmision);
        Assert.Equal(Hoy.AddMonths(61), dto.FechaVencimiento);
    }

    [Fact]
    public async Task Emision_numero_duplicado_devuelve_409()
    {
        await using var db = await CrearBaseAsync();

        var (producto1, _) = await SembrarProductoConRegistroAsync(db);
        var (_, registro2) = await SembrarProductoConRegistroAsync(db);

        registro2.NumeroRegistro = "RS-001";
        registro2.FechaEmision = Hoy;
        registro2.FechaVencimiento = Hoy.AddMonths(60);
        registro2.Estado = EstadoVencimiento.Vigente;
        await db.SaveChangesAsync(default);

        var controller = CrearRegistrosController(db);

        var result = await controller.RegistrarEmision(
            producto1.ProductoId,
            new EmisionRequest { Numero = "RS-001", FechaEmision = Hoy },
            default);

        var conflict = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, conflict.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(conflict.Value);
        var errors = Assert.IsType<Dictionary<string, string[]>>(
            problem.Extensions["errors"]);
        Assert.Contains("numero", errors.Keys);
    }

    [Fact]
    public async Task Emision_numero_de_registro_dado_de_baja_tambien_duplica()
    {
        await using var db = await CrearBaseAsync();

        var (producto1, _) = await SembrarProductoConRegistroAsync(db);
        var (_, registro2) = await SembrarProductoConRegistroAsync(db);

        registro2.NumeroRegistro = "RS-001";
        registro2.FechaEmision = Hoy;
        registro2.FechaVencimiento = Hoy.AddMonths(60);
        registro2.DadoDeBaja = true;
        await db.SaveChangesAsync(default);

        var controller = CrearRegistrosController(db);

        var result = await controller.RegistrarEmision(
            producto1.ProductoId,
            new EmisionRequest { Numero = "RS-001", FechaEmision = Hoy },
            default);

        var conflict = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, conflict.StatusCode);
    }

    [Fact]
    public async Task Emision_conserva_el_checklist_existente()
    {
        await using var db = await CrearBaseAsync();
        var (producto, registro) = await SembrarProductoConRegistroAsync(db);

        registro.Estado = EstadoVencimiento.EnTramite;

        db.ChecklistRequisitos.Add(new ChecklistRequisito
        {
            ChecklistRequisitoId = Guid.NewGuid(),
            EmpresaId = EmpresaId,
            RequisitoId = CatalogoIds.Requisitos.SolicitudArsa,
            TipoTramiteId = CatalogoIds.TiposTramite.AltaProducto,
            ProductoId = producto.ProductoId,
            Estado = EstadoRequisito.Pendiente
        });

        await db.SaveChangesAsync(default);

        var controller = CrearRegistrosController(db);

        var result = await controller.RegistrarEmision(
            producto.ProductoId,
            new EmisionRequest { Numero = "RS-001", FechaEmision = Hoy },
            default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<RegistroSanitarioDto>(ok.Value);

        Assert.True(dto.TieneChecklist);
        Assert.Equal(
            1,
            await db.ChecklistRequisitos.CountAsync(
                c => c.ProductoId == producto.ProductoId));
    }

    [Fact]
    public async Task Emision_producto_inexistente_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearRegistrosController(db);

        var result = await controller.RegistrarEmision(
            Guid.NewGuid(),
            new EmisionRequest { Numero = "RS-001", FechaEmision = Hoy },
            default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task Emision_producto_sin_registro_devuelve_404()
    {
        await using var db = await CrearBaseAsync();

        var producto = ProductoMedicamento();
        db.Productos.Add(producto);
        await db.SaveChangesAsync(default);

        var controller = CrearRegistrosController(db);

        var result = await controller.RegistrarEmision(
            producto.ProductoId,
            new EmisionRequest { Numero = "RS-001", FechaEmision = Hoy },
            default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task Emision_sin_numero_devuelve_400()
    {
        await using var db = await CrearBaseAsync();
        var (producto, _) = await SembrarProductoConRegistroAsync(db);
        var controller = CrearRegistrosController(db);

        var result = await controller.RegistrarEmision(
            producto.ProductoId,
            new EmisionRequest { FechaEmision = Hoy },
            default);

        var bad = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);
    }

    [Fact]
    public async Task Emision_sin_fecha_devuelve_400()
    {
        await using var db = await CrearBaseAsync();
        var (producto, _) = await SembrarProductoConRegistroAsync(db);
        var controller = CrearRegistrosController(db);

        var result = await controller.RegistrarEmision(
            producto.ProductoId,
            new EmisionRequest { Numero = "RS-001" },
            default);

        var bad = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);
    }
}
