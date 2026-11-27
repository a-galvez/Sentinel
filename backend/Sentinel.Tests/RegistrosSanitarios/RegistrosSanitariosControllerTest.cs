using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Features.Productos.DTOs;
using Sentinel.Domain.Enums;
using Sentinel.Tests.Productos;

namespace Sentinel.Tests.RegistrosSanitarios;

public class RegistrosSanitariosControllerTests : ProductosTestBase
{
    [Fact]
    public async Task Verificar_si_lo_tiene_devuelve_200_y_vigente()
    {
        await using var db = await CrearBaseAsync();
        var (producto, _) = await SembrarProductoConRegistroAsync(db);
        var controller = CrearRegistrosController(db);

        var request = new VerificarRegistroRequest
        {
            TieneDocumento = true,
            Numero = "RS-001",
            FechaEmision = Hoy
        };

        var result = await controller.Verificar(producto.ProductoId, request, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<RegistroSanitarioDto>(ok.Value);

        Assert.Equal("RS-001", dto.Numero);
        Assert.Equal(Hoy.AddMonths(60), dto.FechaVencimiento);
        Assert.Equal("Vigente", dto.Estado);
        Assert.False(dto.TieneChecklist);
        Assert.Equal(0, await db.ChecklistRequisitos.CountAsync());
    }

    [Fact]
    public async Task Verificar_no_lo_tiene_devuelve_200_y_crea_checklist()
    {
        await using var db = await CrearBaseAsync();
        var (producto, _) = await SembrarProductoConRegistroAsync(db);
        var controller = CrearRegistrosController(db);

        var request = new VerificarRegistroRequest
        {
            TieneDocumento = false
        };

        var result = await controller.Verificar(producto.ProductoId, request, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<RegistroSanitarioDto>(ok.Value);

        Assert.Equal("EnTramite", dto.Estado);
        Assert.True(dto.TieneChecklist);
        Assert.Equal(
            6,
            await db.ChecklistRequisitos.CountAsync(
                c => c.ProductoId == producto.ProductoId));
    }

    [Fact]
    public async Task Verificar_numero_duplicado_devuelve_409()
    {
        await using var db = await CrearBaseAsync();

        var (producto1, _) = await SembrarProductoConRegistroAsync(db);
        var (_, registro2) = await SembrarProductoConRegistroAsync(db);

        registro2.NumeroRegistro = "RS-001";
        registro2.FechaEmision = Hoy;
        registro2.FechaVencimiento = Hoy.AddMonths(60);
        await db.SaveChangesAsync(default);

        var controller = CrearRegistrosController(db);

        var request = new VerificarRegistroRequest
        {
            TieneDocumento = true,
            Numero = "RS-001",
            FechaEmision = Hoy
        };

        var result = await controller.Verificar(producto1.ProductoId, request, default);

        var conflict = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, conflict.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(conflict.Value);
        Assert.Equal(409, problem.Status);
    }

    [Fact]
    public async Task Verificar_ya_verificado_devuelve_409()
    {
        await using var db = await CrearBaseAsync();
        var (producto, registro) = await SembrarProductoConRegistroAsync(db);

        registro.Estado = EstadoVencimiento.EnTramite;
        await db.SaveChangesAsync(default);

        var controller = CrearRegistrosController(db);

        var request = new VerificarRegistroRequest
        {
            TieneDocumento = false
        };

        var result = await controller.Verificar(producto.ProductoId, request, default);

        var conflict = Assert.IsType<ObjectResult>(result);
        Assert.Equal(409, conflict.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(conflict.Value);
        Assert.Equal(409, problem.Status);
    }

    [Fact]
    public async Task Verificar_producto_inexistente_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearRegistrosController(db);

        var request = new VerificarRegistroRequest
        {
            TieneDocumento = false
        };

        var result = await controller.Verificar(Guid.NewGuid(), request, default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(notFound.Value);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task Verificar_producto_sin_registro_devuelve_404()
    {
        await using var db = await CrearBaseAsync();

        var producto = ProductoMedicamento();
        db.Productos.Add(producto);
        await db.SaveChangesAsync(default);

        var controller = CrearRegistrosController(db);

        var request = new VerificarRegistroRequest
        {
            TieneDocumento = false
        };

        var result = await controller.Verificar(producto.ProductoId, request, default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(notFound.Value);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task Verificar_sin_numero_o_fecha_devuelve_400()
    {
        await using var db = await CrearBaseAsync();
        var (producto, _) = await SembrarProductoConRegistroAsync(db);
        var controller = CrearRegistrosController(db);

        var request = new VerificarRegistroRequest
        {
            TieneDocumento = true
        };

        var result = await controller.Verificar(producto.ProductoId, request, default);

        var bad = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, bad.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(bad.Value);
        Assert.Equal(400, problem.Status);
    }
}
