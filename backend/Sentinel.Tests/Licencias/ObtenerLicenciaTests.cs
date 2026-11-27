using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.LicenciasSanitarias;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Tests.Licencias;

public class ObtenerLicenciaTests : LicenciasTestBase
{
    private const int TipoDocumentoEstablecimiento =
        CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento;

    private async Task<LicenciaSanitariaDto> ObtenerAsync(
        AppDbContext db, Guid establecimientoId)
    {
        var controller = ObtenerLicenciaController(db);
        var action = await controller.Obtener(establecimientoId, default);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        return Assert.IsType<LicenciaSanitariaDto>(ok.Value);
    }

    [Fact]
    public async Task Obtener_devuelve_la_licencia()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, nombre: "Farmacia A");
        var licencia = await SembrarLicenciaAsync(
            db,
            establecimiento.EstablecimientoId,
            TipoDocumentoEstablecimiento,
            fechaEmision: Hoy.AddDays(-10),
            fechaVencimiento: Hoy.AddDays(100));

        var dto = await ObtenerAsync(db, establecimiento.EstablecimientoId);

        Assert.Equal(licencia.LicenciaSanitariaId, dto.LicenciaId);
        Assert.Equal(establecimiento.EstablecimientoId, dto.EstablecimientoId);
        Assert.Equal("LIC-FARM", dto.TipoDocumento.Codigo);
        Assert.Equal("Licencia sanitaria de farmacia", dto.TipoDocumento.Nombre);
        Assert.Equal("ARSA", dto.TipoDocumento.AutoridadEmisora);
        Assert.Equal(12, dto.TipoDocumento.VigenciaMeses);
        Assert.Equal("Vigente", dto.Estado);
        Assert.Equal("LIC-DEMO", dto.Numero);
        Assert.Equal(Hoy.AddDays(-10), dto.FechaEmision);
        Assert.Equal(Hoy.AddDays(100), dto.FechaVencimiento);
        Assert.Equal(100, dto.DiasParaVencer);
        Assert.False(dto.RequiereVerificacion);
        Assert.False(dto.TieneChecklist);
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

        Assert.Equal(esperado, dto.Estado);
        Assert.Equal(diasVencimiento, dto.DiasParaVencer);
        Assert.Equal(requiereVerificacion, dto.RequiereVerificacion);
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

        Assert.True((await ObtenerAsync(db, conChecklist.EstablecimientoId)).TieneChecklist);
        Assert.False((await ObtenerAsync(db, sinChecklist.EstablecimientoId)).TieneChecklist);
    }

    [Fact]
    public async Task Obtener_establecimiento_inexistente_404()
    {
        await using var db = await CrearBaseAsync();
        var controller = ObtenerLicenciaController(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => controller.Obtener(Guid.NewGuid(), default));
    }

    [Fact]
    public async Task Obtener_establecimiento_de_otra_empresa_404()
    {
        await using var db = await CrearBaseAsync();
        var ajena = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, empresaId: Guid.NewGuid());
        var controller = ObtenerLicenciaController(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => controller.Obtener(ajena.EstablecimientoId, default));
    }

    [Fact]
    public async Task Obtener_establecimiento_dado_de_baja_404()
    {
        await using var db = await CrearBaseAsync();
        var baja = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, dadoDeBaja: true);
        var controller = ObtenerLicenciaController(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => controller.Obtener(baja.EstablecimientoId, default));
    }

    [Fact]
    public async Task Obtener_sin_licencia_404()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Botica);
        var controller = ObtenerLicenciaController(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => controller.Obtener(establecimiento.EstablecimientoId, default));
    }

    [Fact]
    public async Task Obtener_licencia_sin_tipo_documento_404()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        var licencia = await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento);

        licencia.TipoDocumentoId = null;
        await db.SaveChangesAsync();

        var controller = ObtenerLicenciaController(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => controller.Obtener(establecimiento.EstablecimientoId, default));
    }

    [Fact]
    public async Task Obtener_licencia_dada_de_baja_404()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento, dadoDeBaja: true);
        var controller = ObtenerLicenciaController(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => controller.Obtener(establecimiento.EstablecimientoId, default));
    }

    [Fact]
    public async Task Obtener_tipo_documento_inexistente_lanza_invalid_operation()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, tipoDocumentoId: 999);
        var controller = ObtenerLicenciaController(db);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => controller.Obtener(establecimiento.EstablecimientoId, default));
    }
}
