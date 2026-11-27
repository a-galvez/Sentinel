using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.LicenciasSanitarias;
using Sentinel.Application.Features.LicenciasSanitarias.RegistrarEmision;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Tests.Licencias;

public class RegistrarEmisionLicenciaTests : LicenciasTestBase
{
    private const int TipoDocumentoEstablecimiento =
        CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento;

    private async Task<LicenciaSanitariaDto> EmitirAsync(
        AppDbContext db, Guid establecimientoId, RegistrarEmisionLicenciaRequest request)
    {
        var controller = RegistrarEmisionLicenciaController(db);
        var action = await controller.RegistrarEmision(establecimientoId, request, default);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        return Assert.IsType<LicenciaSanitariaDto>(ok.Value);
    }

    [Theory]
    [InlineData(0, EstadoVencimiento.Vigente)]
    [InlineData(-11, EstadoVencimiento.PorVencer)]
    [InlineData(-24, EstadoVencimiento.Vencido)]
    public async Task Emision_actualiza_el_estado_guardado(
        int mesesAtras, EstadoVencimiento esperado)
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        var licencia = await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento);

        await EmitirAsync(
            db,
            establecimiento.EstablecimientoId,
            new RegistrarEmisionLicenciaRequest
            {
                Numero = "LS-001",
                FechaEmision = Hoy.AddMonths(mesesAtras)
            });

        var guardada = await db.Licencias
            .SingleAsync(l => l.LicenciaSanitariaId == licencia.LicenciaSanitariaId);

        Assert.Equal(esperado, guardada.Estado);
    }

    [Fact]
    public async Task Emision_actualiza_numero_fechas_y_estado()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        var licencia = await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento);

        var dto = await EmitirAsync(
            db,
            establecimiento.EstablecimientoId,
            new RegistrarEmisionLicenciaRequest { Numero = "  LS-001  ", FechaEmision = Hoy });

        Assert.Equal("LS-001", dto.Numero);
        Assert.Equal(Hoy, dto.FechaEmision);
        Assert.Equal(Hoy.AddMonths(12), dto.FechaVencimiento);
        Assert.Equal("Vigente", dto.Estado);
        Assert.False(dto.RequiereVerificacion);
        Assert.False(dto.TieneChecklist);

        var guardada = await db.Licencias
            .SingleAsync(l => l.LicenciaSanitariaId == licencia.LicenciaSanitariaId);
        Assert.Equal("LS-001", guardada.NumeroLicencia);
        Assert.Equal(EstadoVencimiento.Vigente, guardada.Estado);
        Assert.Equal(0, await db.ChecklistRequisitos.CountAsync());
    }

    [Fact]
    public async Task Emision_desde_en_tramite_conserva_el_checklist()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        var licencia = await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento,
            estado: EstadoVencimiento.EnTramite);
        await SembrarChecklistAsync(db, licencia.LicenciaSanitariaId);

        var antes = await db.ChecklistRequisitos
            .CountAsync(c => c.LicenciaSanitariaId == licencia.LicenciaSanitariaId);

        var dto = await EmitirAsync(
            db,
            establecimiento.EstablecimientoId,
            new RegistrarEmisionLicenciaRequest { Numero = "LS-002", FechaEmision = Hoy });

        Assert.True(dto.TieneChecklist);
        Assert.Equal(antes, await db.ChecklistRequisitos
            .CountAsync(c => c.LicenciaSanitariaId == licencia.LicenciaSanitariaId));

        var guardada = await db.Licencias
            .SingleAsync(l => l.LicenciaSanitariaId == licencia.LicenciaSanitariaId);
        Assert.Equal(EstadoVencimiento.Vigente, guardada.Estado);
    }

    [Fact]
    public async Task Emision_corrige_datos_ya_emitidos()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento,
            fechaEmision: Hoy.AddYears(-1), fechaVencimiento: Hoy.AddDays(10),
            estado: EstadoVencimiento.PorVencer);

        var dto = await EmitirAsync(
            db,
            establecimiento.EstablecimientoId,
            new RegistrarEmisionLicenciaRequest { Numero = " LS-CORREGIDA ", FechaEmision = Hoy });

        Assert.Equal("LS-CORREGIDA", dto.Numero);
        Assert.Equal(Hoy, dto.FechaEmision);
        Assert.Equal(Hoy.AddMonths(12), dto.FechaVencimiento);
        Assert.Equal("Vigente", dto.Estado);
    }

    [Fact]
    public async Task Emision_404_cuando_no_hay_establecimiento_o_licencia()
    {
        await using var db = await CrearBaseAsync();
        var request = new RegistrarEmisionLicenciaRequest { Numero = "LS-1", FechaEmision = Hoy };

        await Assert.ThrowsAsync<NotFoundException>(
            () => EmitirAsync(db, Guid.NewGuid(), request));

        var ajena = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, empresaId: Guid.NewGuid());
        await Assert.ThrowsAsync<NotFoundException>(
            () => EmitirAsync(db, ajena.EstablecimientoId, request));

        var baja = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, dadoDeBaja: true);
        await Assert.ThrowsAsync<NotFoundException>(
            () => EmitirAsync(db, baja.EstablecimientoId, request));

        var botica = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Botica);
        await Assert.ThrowsAsync<NotFoundException>(
            () => EmitirAsync(db, botica.EstablecimientoId, request));

        var sinTipo = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        var licenciaSinTipo = await SembrarLicenciaAsync(
            db, sinTipo.EstablecimientoId, TipoDocumentoEstablecimiento);
        licenciaSinTipo.TipoDocumentoId = null;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<NotFoundException>(
            () => EmitirAsync(db, sinTipo.EstablecimientoId, request));

        var conLicenciaBaja = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, conLicenciaBaja.EstablecimientoId, TipoDocumentoEstablecimiento, dadoDeBaja: true);
        await Assert.ThrowsAsync<NotFoundException>(
            () => EmitirAsync(db, conLicenciaBaja.EstablecimientoId, request));
    }

    [Fact]
    public async Task Emision_cuerpo_invalido_lanza_validacion()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento);

        var ex = await Assert.ThrowsAsync<ValidationException>(() => EmitirAsync(
            db,
            establecimiento.EstablecimientoId,
            new RegistrarEmisionLicenciaRequest { Numero = "LS-1" }));

        Assert.Contains(ex.Errors, e => e.PropertyName == "Numero");
    }

    [Fact]
    public async Task Validator_rechaza_combinaciones_invalidas()
    {
        var validator = new RegistrarEmisionLicenciaValidator();

        Assert.False((await validator.ValidateAsync(new RegistrarEmisionLicenciaRequest())).IsValid);
        Assert.False((await validator.ValidateAsync(
            new RegistrarEmisionLicenciaRequest { Numero = "LS-1" })).IsValid);
        Assert.False((await validator.ValidateAsync(
            new RegistrarEmisionLicenciaRequest { FechaEmision = Hoy })).IsValid);
        Assert.False((await validator.ValidateAsync(
            new RegistrarEmisionLicenciaRequest { Numero = "   ", FechaEmision = Hoy })).IsValid);
        Assert.False((await validator.ValidateAsync(new RegistrarEmisionLicenciaRequest
        {
            Numero = new string('x', 101),
            FechaEmision = Hoy
        })).IsValid);

        Assert.True((await validator.ValidateAsync(new RegistrarEmisionLicenciaRequest
        {
            Numero = "LS-1",
            FechaEmision = Hoy
        })).IsValid);
    }
}
