using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.LicenciasSanitarias;
using Sentinel.Application.Features.LicenciasSanitarias.Verificar;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Tests.Licencias;

public class VerificarLicenciaTests : LicenciasTestBase
{
    private const int TipoDocumentoEstablecimiento =
        CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento;

    private async Task<LicenciaSanitariaDto> VerificarAsync(
        AppDbContext db, Guid establecimientoId, VerificarLicenciaRequest request)
    {
        var controller = VerificarLicenciaController(db);
        var action = await controller.Verificar(establecimientoId, request, default);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        return Assert.IsType<LicenciaSanitariaDto>(ok.Value);
    }

    private async Task<(Establecimiento Establecimiento, LicenciaSanitaria Licencia)>
        SembrarFarmaciaConLicenciaAsync(
            AppDbContext db, EstadoVencimiento estado = EstadoVencimiento.NoVerificado)
    {
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        var licencia = await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento, estado: estado);
        return (establecimiento, licencia);
    }

    [Fact]
    public async Task Verificar_tiene_documento_actualiza_la_licencia_y_no_crea_checklist()
    {
        await using var db = await CrearBaseAsync();
        var (establecimiento, licencia) = await SembrarFarmaciaConLicenciaAsync(db);

        var dto = await VerificarAsync(
            db,
            establecimiento.EstablecimientoId,
            new VerificarLicenciaRequest
            {
                TieneDocumento = true,
                Numero = "  LS-001  ",
                FechaEmision = Hoy
            });

        Assert.Equal("Vigente", dto.Estado);
        Assert.Equal("LS-001", dto.Numero);
        Assert.Equal(Hoy, dto.FechaEmision);
        Assert.Equal(Hoy.AddMonths(12), dto.FechaVencimiento);
        Assert.Equal(Hoy.AddMonths(12).DayNumber - Hoy.DayNumber, dto.DiasParaVencer);
        Assert.False(dto.RequiereVerificacion);
        Assert.False(dto.TieneChecklist);

        var guardada = await db.Licencias
            .SingleAsync(l => l.LicenciaSanitariaId == licencia.LicenciaSanitariaId);
        Assert.Equal(EstadoVencimiento.Vigente, guardada.Estado);
        Assert.Equal(0, await db.ChecklistRequisitos.CountAsync());
    }

    [Fact]
    public async Task Verificar_sin_documento_pasa_a_en_tramite_y_crea_checklist()
    {
        await using var db = await CrearBaseAsync();
        var (establecimiento, licencia) = await SembrarFarmaciaConLicenciaAsync(db);

        var dto = await VerificarAsync(
            db,
            establecimiento.EstablecimientoId,
            new VerificarLicenciaRequest { TieneDocumento = false });

        Assert.Equal("EnTramite", dto.Estado);
        Assert.Null(dto.Numero);
        Assert.Null(dto.FechaEmision);
        Assert.Null(dto.FechaVencimiento);
        Assert.Null(dto.DiasParaVencer);
        Assert.False(dto.RequiereVerificacion);
        Assert.True(dto.TieneChecklist);

        var filas = await db.ChecklistRequisitos
            .Where(c => c.LicenciaSanitariaId == licencia.LicenciaSanitariaId)
            .ToListAsync();

        Assert.Equal(5, filas.Count);
        Assert.All(filas, fila =>
        {
            Assert.Equal(EmpresaId, fila.EmpresaId);
            Assert.Equal(EstadoRequisito.Pendiente, fila.Estado);
            Assert.Equal(CatalogoIds.TiposTramite.AltaEstablecimiento, fila.TipoTramiteId);
            Assert.True(fila.Orden > 0);
        });
    }

    [Fact]
    public async Task Verificar_sin_documento_no_duplica_un_checklist_existente()
    {
        await using var db = await CrearBaseAsync();
        var (establecimiento, licencia) = await SembrarFarmaciaConLicenciaAsync(db);
        await SembrarChecklistAsync(db, licencia.LicenciaSanitariaId);

        await VerificarAsync(
            db,
            establecimiento.EstablecimientoId,
            new VerificarLicenciaRequest { TieneDocumento = false });

        Assert.Equal(1, await db.ChecklistRequisitos
            .CountAsync(c => c.LicenciaSanitariaId == licencia.LicenciaSanitariaId));
    }

    [Theory]
    [InlineData(0, "Vigente")]
    [InlineData(-11, "PorVencer")]
    [InlineData(-24, "Vencido")]
    public async Task Verificar_calcula_el_estado_segun_la_fecha_de_emision(
        int mesesAtras, string esperado)
    {
        await using var db = await CrearBaseAsync();
        var (establecimiento, _) = await SembrarFarmaciaConLicenciaAsync(db);

        var dto = await VerificarAsync(
            db,
            establecimiento.EstablecimientoId,
            new VerificarLicenciaRequest
            {
                TieneDocumento = true,
                Numero = "LS-001",
                FechaEmision = Hoy.AddMonths(mesesAtras)
            });

        Assert.Equal(esperado, dto.Estado);
    }

    [Fact]
    public async Task Verificar_licencia_ya_emitida_devuelve_409()
    {
        await using var db = await CrearBaseAsync();
        var (establecimiento, _) = await SembrarFarmaciaConLicenciaAsync(
            db, EstadoVencimiento.Vigente);

        await Assert.ThrowsAsync<ConflictException>(() => VerificarAsync(
            db,
            establecimiento.EstablecimientoId,
            new VerificarLicenciaRequest { TieneDocumento = false }));
    }

    [Fact]
    public async Task Verificar_licencia_con_fechas_y_estado_obsoleto_devuelve_409()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db,
            establecimiento.EstablecimientoId,
            TipoDocumentoEstablecimiento,
            fechaEmision: Hoy,
            fechaVencimiento: Hoy.AddMonths(12),
            estado: EstadoVencimiento.NoVerificado);

        await Assert.ThrowsAsync<ConflictException>(() => VerificarAsync(
            db,
            establecimiento.EstablecimientoId,
            new VerificarLicenciaRequest { TieneDocumento = false }));
    }

    [Fact]
    public async Task Verificar_404_cuando_no_hay_establecimiento_o_licencia()
    {
        await using var db = await CrearBaseAsync();
        var request = new VerificarLicenciaRequest { TieneDocumento = false };

        await Assert.ThrowsAsync<NotFoundException>(
            () => VerificarAsync(db, Guid.NewGuid(), request));

        var ajena = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, empresaId: Guid.NewGuid());
        await Assert.ThrowsAsync<NotFoundException>(
            () => VerificarAsync(db, ajena.EstablecimientoId, request));

        var baja = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, dadoDeBaja: true);
        await Assert.ThrowsAsync<NotFoundException>(
            () => VerificarAsync(db, baja.EstablecimientoId, request));

        var botica = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Botica);
        await Assert.ThrowsAsync<NotFoundException>(
            () => VerificarAsync(db, botica.EstablecimientoId, request));

        var sinTipo = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        var licenciaSinTipo = await SembrarLicenciaAsync(
            db, sinTipo.EstablecimientoId, TipoDocumentoEstablecimiento);
        licenciaSinTipo.TipoDocumentoId = null;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<NotFoundException>(
            () => VerificarAsync(db, sinTipo.EstablecimientoId, request));

        var conLicenciaBaja = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, conLicenciaBaja.EstablecimientoId, TipoDocumentoEstablecimiento, dadoDeBaja: true);
        await Assert.ThrowsAsync<NotFoundException>(
            () => VerificarAsync(db, conLicenciaBaja.EstablecimientoId, request));
    }

    [Fact]
    public async Task Verificar_valida_el_orden_404_409_400()
    {
        await using var db = await CrearBaseAsync();
        var requestVacio = new VerificarLicenciaRequest();

        // 404 gana a cuerpo inválido.
        await Assert.ThrowsAsync<NotFoundException>(
            () => VerificarAsync(db, Guid.NewGuid(), requestVacio));

        // 409 gana a cuerpo inválido.
        var (verificada, _) = await SembrarFarmaciaConLicenciaAsync(
            db, EstadoVencimiento.EnTramite);
        await Assert.ThrowsAsync<ConflictException>(
            () => VerificarAsync(db, verificada.EstablecimientoId, requestVacio));

        // Existe y NoVerificado → 400 por el cuerpo.
        var (pendiente, _) = await SembrarFarmaciaConLicenciaAsync(db);
        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => VerificarAsync(db, pendiente.EstablecimientoId, requestVacio));
        Assert.Contains(ex.Errors, e => e.PropertyName == "TieneDocumento");
    }

    [Fact]
    public async Task Validator_rechaza_combinaciones_invalidas()
    {
        var validator = new VerificarLicenciaValidator();

        Assert.False((await validator.ValidateAsync(new VerificarLicenciaRequest())).IsValid);
        Assert.False((await validator.ValidateAsync(
            new VerificarLicenciaRequest { TieneDocumento = true })).IsValid);
        Assert.False((await validator.ValidateAsync(
            new VerificarLicenciaRequest { TieneDocumento = true, Numero = "LS-1" })).IsValid);
        Assert.False((await validator.ValidateAsync(
            new VerificarLicenciaRequest { TieneDocumento = true, FechaEmision = Hoy })).IsValid);
        Assert.False((await validator.ValidateAsync(
            new VerificarLicenciaRequest { TieneDocumento = false, Numero = "LS-1" })).IsValid);
        Assert.False((await validator.ValidateAsync(
            new VerificarLicenciaRequest { TieneDocumento = false, FechaEmision = Hoy })).IsValid);
        Assert.False((await validator.ValidateAsync(new VerificarLicenciaRequest
        {
            TieneDocumento = true,
            Numero = new string('x', 101),
            FechaEmision = Hoy
        })).IsValid);

        Assert.True((await validator.ValidateAsync(new VerificarLicenciaRequest
        {
            TieneDocumento = true,
            Numero = "LS-1",
            FechaEmision = Hoy
        })).IsValid);
        Assert.True((await validator.ValidateAsync(
            new VerificarLicenciaRequest { TieneDocumento = false })).IsValid);
    }

    [Fact]
    public void Request_deserializa_tieneDocumento()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var request = JsonSerializer.Deserialize<VerificarLicenciaRequest>(
            "{\"tieneDocumento\":true}", options);

        Assert.NotNull(request);
        Assert.True(request!.TieneDocumento);
    }
}
