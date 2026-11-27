using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Common;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.Establecimientos;
using Sentinel.Application.Features.Establecimientos.Listar;
using Sentinel.Application.Features.LicenciasSanitarias;
using Sentinel.Application.Features.LicenciasSanitarias.Listar;
using Sentinel.Domain.Constantes;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Tests.Licencias;

public class DarDeBajaLicenciaTests : LicenciasTestBase
{
    private const int TipoDocumentoEstablecimiento =
        CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento;

    private async Task<IActionResult> DarDeBajaAsync(AppDbContext db, Guid establecimientoId)
    {
        var controller = DarDeBajaLicenciaController(db);
        return await controller.DarDeBaja(establecimientoId, default);
    }

    [Fact]
    public async Task DarDeBaja_marca_solo_la_licencia()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        var licencia = await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento);
        await SembrarChecklistAsync(db, licencia.LicenciaSanitariaId);

        var result = await DarDeBajaAsync(db, establecimiento.EstablecimientoId);

        Assert.IsType<NoContentResult>(result);

        var licenciaGuardada = await db.Licencias.IgnoreQueryFilters()
            .SingleAsync(l => l.LicenciaSanitariaId == licencia.LicenciaSanitariaId);
        var establecimientoGuardado = await db.Establecimientos.IgnoreQueryFilters()
            .SingleAsync(e => e.EstablecimientoId == establecimiento.EstablecimientoId);

        Assert.True(licenciaGuardada.DadoDeBaja);
        Assert.False(establecimientoGuardado.DadoDeBaja);
        Assert.Equal(1, await db.ChecklistRequisitos
            .CountAsync(c => c.LicenciaSanitariaId == licencia.LicenciaSanitariaId));
    }

    [Fact]
    public async Task DarDeBaja_la_licencia_desaparece_de_los_endpoints()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento,
            fechaEmision: Hoy, fechaVencimiento: Hoy.AddYears(1));

        await DarDeBajaAsync(db, establecimiento.EstablecimientoId);

        var listarEstablecimientos = ListarController(db);
        var accionEstablecimientos = await listarEstablecimientos.Listar(
            new ListarEstablecimientosRequest(), default);
        var okEstablecimientos = Assert.IsType<OkObjectResult>(accionEstablecimientos.Result);
        var paginaEstablecimientos =
            Assert.IsType<PaginaDto<EstablecimientoDto>>(okEstablecimientos.Value);
        Assert.Equal(1, paginaEstablecimientos.Total);
        Assert.Null(Assert.Single(paginaEstablecimientos.Items).Licencia);

        var obtenerEstablecimiento = ObtenerController(db);
        var accionObtener = await obtenerEstablecimiento.Obtener(
            establecimiento.EstablecimientoId, default);
        var okObtener = Assert.IsType<OkObjectResult>(accionObtener.Result);
        Assert.Null(Assert.IsType<EstablecimientoDto>(okObtener.Value).Licencia);

        var listarLicencias = ListarLicenciasController(db);
        var accionLicencias = await listarLicencias.Listar(new ListarLicenciasRequest(), default);
        var okLicencias = Assert.IsType<OkObjectResult>(accionLicencias.Result);
        Assert.Equal(0, Assert.IsType<PaginaDto<LicenciaListadoDto>>(okLicencias.Value).Total);

        var obtenerLicencia = ObtenerLicenciaController(db);
        await Assert.ThrowsAsync<NotFoundException>(
            () => obtenerLicencia.Obtener(establecimiento.EstablecimientoId, default));
    }

    [Fact]
    public async Task DarDeBaja_404_cuando_no_hay_establecimiento_o_licencia()
    {
        await using var db = await CrearBaseAsync();

        await Assert.ThrowsAsync<NotFoundException>(
            () => DarDeBajaAsync(db, Guid.NewGuid()));

        var ajena = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, empresaId: Guid.NewGuid());
        await Assert.ThrowsAsync<NotFoundException>(
            () => DarDeBajaAsync(db, ajena.EstablecimientoId));

        var baja = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, dadoDeBaja: true);
        await Assert.ThrowsAsync<NotFoundException>(
            () => DarDeBajaAsync(db, baja.EstablecimientoId));

        var botica = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Botica);
        await Assert.ThrowsAsync<NotFoundException>(
            () => DarDeBajaAsync(db, botica.EstablecimientoId));

        var sinTipo = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        var licenciaSinTipo = await SembrarLicenciaAsync(
            db, sinTipo.EstablecimientoId, TipoDocumentoEstablecimiento);
        licenciaSinTipo.TipoDocumentoId = null;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<NotFoundException>(
            () => DarDeBajaAsync(db, sinTipo.EstablecimientoId));

        var conLicenciaBaja = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, conLicenciaBaja.EstablecimientoId, TipoDocumentoEstablecimiento, dadoDeBaja: true);
        await Assert.ThrowsAsync<NotFoundException>(
            () => DarDeBajaAsync(db, conLicenciaBaja.EstablecimientoId));
    }

    [Fact]
    public async Task DarDeBaja_dos_veces_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento);

        Assert.IsType<NoContentResult>(
            await DarDeBajaAsync(db, establecimiento.EstablecimientoId));

        await Assert.ThrowsAsync<NotFoundException>(
            () => DarDeBajaAsync(db, establecimiento.EstablecimientoId));
    }
}
