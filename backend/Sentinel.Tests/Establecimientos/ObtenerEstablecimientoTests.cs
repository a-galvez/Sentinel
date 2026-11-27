using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.Establecimientos;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Tests.Establecimientos;

public class ObtenerEstablecimientoTests : EstablecimientosTestBase
{
    private const int TipoDocumentoEstablecimiento =
        CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento;

    private async Task<EstablecimientoDto> ObtenerAsync(AppDbContext db, Guid establecimientoId)
    {
        var controller = ObtenerController(db);
        var action = await controller.Obtener(establecimientoId, default);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        return Assert.IsType<EstablecimientoDto>(ok.Value);
    }

    [Fact]
    public async Task Obtener_devuelve_datos_y_licencia()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Drogueria, nombre: "Bodega Principal");

        establecimiento.Ubicacion = "Tegucigalpa, Col. Palmira";
        await db.SaveChangesAsync();

        await SembrarLicenciaAsync(
            db,
            establecimiento.EstablecimientoId,
            CatalogoIds.TiposDocumento.LicenciaSanitariaProducto,
            fechaEmision: Hoy.AddDays(-10),
            fechaVencimiento: Hoy.AddDays(100));

        var dto = await ObtenerAsync(db, establecimiento.EstablecimientoId);

        Assert.Equal(establecimiento.EstablecimientoId, dto.EstablecimientoId);
        Assert.Equal("Bodega Principal", dto.Nombre);
        Assert.Equal("Tegucigalpa, Col. Palmira", dto.Ubicacion);
        Assert.Equal(CatalogoIds.TiposEstablecimiento.Drogueria, dto.Tipo.Id);
        Assert.Equal("Droguería", dto.Tipo.Nombre);

        var licencia = dto.Licencia;
        Assert.NotNull(licencia);
        Assert.Equal(establecimiento.EstablecimientoId, licencia!.EstablecimientoId);
        Assert.Equal("Vigente", licencia.Estado);
        Assert.Equal("LIC-DROG", licencia.TipoDocumento.Codigo);
        Assert.Equal("Licencia sanitaria de droguería", licencia.TipoDocumento.Nombre);
        Assert.Equal("ARSA", licencia.TipoDocumento.AutoridadEmisora);
        Assert.Equal(12, licencia.TipoDocumento.VigenciaMeses);
        Assert.Equal(100, licencia.DiasParaVencer);
        Assert.False(licencia.RequiereVerificacion);
    }

    [Fact]
    public async Task Obtener_sin_licencia_devuelve_licencia_null()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Botica, nombre: "Botica Sin Regla");

        var dto = await ObtenerAsync(db, establecimiento.EstablecimientoId);

        Assert.Equal("Botica", dto.Tipo.Nombre);
        Assert.Null(dto.Licencia);
    }

    [Fact]
    public async Task Obtener_licencia_sin_tipo_documento_devuelve_licencia_null()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        var licencia = await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento);

        licencia.TipoDocumentoId = null;
        await db.SaveChangesAsync();

        var dto = await ObtenerAsync(db, establecimiento.EstablecimientoId);

        Assert.Null(dto.Licencia);
    }

    [Fact]
    public async Task Obtener_licencia_dada_de_baja_devuelve_licencia_null()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento, dadoDeBaja: true);

        var dto = await ObtenerAsync(db, establecimiento.EstablecimientoId);

        Assert.Null(dto.Licencia);
    }

    [Fact]
    public async Task Obtener_marca_tiene_checklist()
    {
        await using var db = await CrearBaseAsync();

        var conChecklist = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        var licenciaCon = await SembrarLicenciaAsync(
            db, conChecklist.EstablecimientoId, TipoDocumentoEstablecimiento);
        await SembrarChecklistAsync(db, licenciaCon.LicenciaSanitariaId);

        var sinChecklist = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, sinChecklist.EstablecimientoId, TipoDocumentoEstablecimiento);

        var conDto = await ObtenerAsync(db, conChecklist.EstablecimientoId);
        var sinDto = await ObtenerAsync(db, sinChecklist.EstablecimientoId);

        Assert.True(conDto.Licencia!.TieneChecklist);
        Assert.False(sinDto.Licencia!.TieneChecklist);
    }

    [Theory]
    [InlineData(null, EstadoVencimiento.NoVerificado, "NoVerificado", true)]
    [InlineData(null, EstadoVencimiento.EnTramite, "EnTramite", false)]
    [InlineData(-1, EstadoVencimiento.NoVerificado, "Vencido", false)]
    [InlineData(30, EstadoVencimiento.NoVerificado, "PorVencer", false)]
    [InlineData(91, EstadoVencimiento.NoVerificado, "Vigente", false)]
    public async Task Obtener_calcula_estado(
        int? diasVencimiento,
        EstadoVencimiento estado,
        string esperado,
        bool requiereVerificacion)
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);

        DateOnly? fechaVencimiento = diasVencimiento is { } dias ? Hoy.AddDays(dias) : null;
        DateOnly? fechaEmision = fechaVencimiento is null ? null : Hoy;

        await SembrarLicenciaAsync(
            db,
            establecimiento.EstablecimientoId,
            TipoDocumentoEstablecimiento,
            fechaEmision: fechaEmision,
            fechaVencimiento: fechaVencimiento,
            estado: estado);

        var dto = await ObtenerAsync(db, establecimiento.EstablecimientoId);
        var licencia = dto.Licencia;

        Assert.NotNull(licencia);
        Assert.Equal(esperado, licencia!.Estado);
        Assert.Equal(diasVencimiento, licencia.DiasParaVencer);
        Assert.Equal(requiereVerificacion, licencia.RequiereVerificacion);
    }

    [Fact]
    public async Task Obtener_tipo_documento_inexistente_devuelve_licencia_null()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, tipoDocumentoId: 999);

        var dto = await ObtenerAsync(db, establecimiento.EstablecimientoId);

        Assert.Null(dto.Licencia);
    }

    [Fact]
    public async Task Obtener_inexistente_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var controller = ObtenerController(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => controller.Obtener(Guid.NewGuid(), default));
    }

    [Fact]
    public async Task Obtener_otra_empresa_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var ajena = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, empresaId: Guid.NewGuid());

        var controller = ObtenerController(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => controller.Obtener(ajena.EstablecimientoId, default));
    }

    [Fact]
    public async Task Obtener_dado_de_baja_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var baja = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, dadoDeBaja: true);

        var controller = ObtenerController(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => controller.Obtener(baja.EstablecimientoId, default));
    }
}
