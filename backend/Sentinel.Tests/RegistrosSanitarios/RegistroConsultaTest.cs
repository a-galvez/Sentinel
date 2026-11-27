using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Features.Productos.DTOs;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;
using Sentinel.Tests.Productos;

namespace Sentinel.Tests.RegistrosSanitarios;

public class RegistroConsultaTests : ProductosTestBase
{
    [Fact]
    public async Task Obtener_registro_devuelve_200()
    {
        await using var db = await CrearBaseAsync();
        var (producto, registro) = await SembrarProductoConRegistroAsync(db);

        var controller = CrearRegistrosController(db);

        var result = await controller.Obtener(producto.ProductoId, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<RegistroSanitarioDto>(ok.Value);

        Assert.Equal(registro.RegistroSanitarioId, dto.RegistroId);
        Assert.Equal("NoVerificado", dto.Estado);
        Assert.True(dto.RequiereVerificacion);
        Assert.Null(dto.Numero);
        Assert.False(dto.TieneChecklist);
        Assert.NotNull(dto.TipoDocumento);
    }

    [Fact]
    public async Task Obtener_incluye_el_checklist()
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

        var result = await controller.Obtener(producto.ProductoId, default);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<RegistroSanitarioDto>(ok.Value);

        Assert.True(dto.TieneChecklist);
        Assert.Equal("EnTramite", dto.Estado);
    }

    [Fact]
    public async Task Obtener_sin_registro_devuelve_404()
    {
        await using var db = await CrearBaseAsync();

        var producto = ProductoMedicamento();
        db.Productos.Add(producto);
        await db.SaveChangesAsync(default);

        var controller = CrearRegistrosController(db);

        var result = await controller.Obtener(producto.ProductoId, default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task Obtener_producto_inexistente_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearRegistrosController(db);

        var result = await controller.Obtener(Guid.NewGuid(), default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task Obtener_producto_dado_de_baja_devuelve_404()
    {
        await using var db = await CrearBaseAsync();

        var producto = ProductoMedicamento();
        producto.DadoDeBaja = true;

        db.Productos.Add(producto);
        db.RegistrosSanitarios.Add(RegistroNoVerificado(producto.ProductoId));
        await db.SaveChangesAsync(default);

        var controller = CrearRegistrosController(db);

        var result = await controller.Obtener(producto.ProductoId, default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }

    [Fact]
    public async Task Obtener_registro_dado_de_baja_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var (producto, registro) = await SembrarProductoConRegistroAsync(db);

        registro.DadoDeBaja = true;
        await db.SaveChangesAsync(default);

        var controller = CrearRegistrosController(db);

        var result = await controller.Obtener(producto.ProductoId, default);

        var notFound = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, notFound.StatusCode);
    }
}
