using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Common;
using Sentinel.Application.Features.LicenciasSanitarias;
using Sentinel.Application.Features.LicenciasSanitarias.Listar;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Tests.Licencias;

public class ListarLicenciasTests : LicenciasTestBase
{
    private const int TipoDocumentoEstablecimiento =
        CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento;

    private async Task<PaginaDto<LicenciaListadoDto>> ListarAsync(
        AppDbContext db, string? estado = null, int pagina = 1, int tamano = 20)
    {
        var controller = ListarLicenciasController(db);
        var action = await controller.Listar(
            new ListarLicenciasRequest { Estado = estado, Pagina = pagina, TamanoPagina = tamano },
            default);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        return Assert.IsType<PaginaDto<LicenciaListadoDto>>(ok.Value);
    }

    [Fact]
    public async Task Listar_sin_filtro_excluye_lo_que_no_corresponde()
    {
        await using var db = await CrearBaseAsync();

        var propia = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, nombre: "Farmacia A");
        await SembrarLicenciaAsync(
            db, propia.EstablecimientoId, TipoDocumentoEstablecimiento,
            fechaEmision: Hoy.AddDays(-10), fechaVencimiento: Hoy.AddDays(100));

        var sinTipo = await SembrarEstablecimientoAsync(db, CatalogoIds.TiposEstablecimiento.Farmacia);
        var licenciaSinTipo = await SembrarLicenciaAsync(
            db, sinTipo.EstablecimientoId, TipoDocumentoEstablecimiento);
        licenciaSinTipo.TipoDocumentoId = null;
        await db.SaveChangesAsync();

        var conLicenciaBaja = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, conLicenciaBaja.EstablecimientoId, TipoDocumentoEstablecimiento, dadoDeBaja: true);

        var establecimientoBaja = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, dadoDeBaja: true);
        await SembrarLicenciaAsync(
            db, establecimientoBaja.EstablecimientoId, TipoDocumentoEstablecimiento);

        var ajena = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, empresaId: Guid.NewGuid());
        await SembrarLicenciaAsync(
            db, ajena.EstablecimientoId, TipoDocumentoEstablecimiento, empresaId: ajena.EmpresaId);

        var pagina = await ListarAsync(db);

        Assert.Equal(1, pagina.Total);
        var item = Assert.Single(pagina.Items);
        Assert.Equal(propia.EstablecimientoId, item.Establecimiento.EstablecimientoId);
    }

    [Theory]
    [InlineData(null, EstadoVencimiento.NoVerificado, "NoVerificado")]
    [InlineData(null, EstadoVencimiento.EnTramite, "EnTramite")]
    [InlineData(-1, EstadoVencimiento.NoVerificado, "Vencido")]
    [InlineData(30, EstadoVencimiento.NoVerificado, "PorVencer")]
    [InlineData(91, EstadoVencimiento.NoVerificado, "Vigente")]
    public async Task Listar_filtra_por_estado(
        int? diasVencimiento, EstadoVencimiento almacenado, string filtro)
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
            estado: almacenado);

        var pagina = await ListarAsync(db, estado: filtro);

        Assert.Equal(1, pagina.Total);
        Assert.Equal(filtro, Assert.Single(pagina.Items).Estado);
    }

    [Fact]
    public async Task Listar_filtros_sin_fechas_excluyen_licencia_emitida_con_estado_obsoleto()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db,
            establecimiento.EstablecimientoId,
            TipoDocumentoEstablecimiento,
            fechaEmision: Hoy,
            fechaVencimiento: Hoy.AddDays(100),
            estado: EstadoVencimiento.EnTramite);

        Assert.Equal(0, (await ListarAsync(db, estado: "EnTramite")).Total);
        Assert.Equal(0, (await ListarAsync(db, estado: "NoVerificado")).Total);
        Assert.Equal(1, (await ListarAsync(db, estado: "Vigente")).Total);
    }

    [Fact]
    public async Task Listar_respeta_pagina_y_total()
    {
        await using var db = await CrearBaseAsync();
        for (var i = 1; i <= 5; i++)
        {
            var establecimiento = await SembrarEstablecimientoAsync(
                db, CatalogoIds.TiposEstablecimiento.Farmacia);
            await SembrarLicenciaAsync(
                db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento,
                fechaEmision: Hoy.AddDays(-i), fechaVencimiento: Hoy.AddDays(100 + i));
        }

        var pagina = await ListarAsync(db, pagina: 2, tamano: 2);

        Assert.Equal(2, pagina.Pagina);
        Assert.Equal(2, pagina.TamanoPagina);
        Assert.Equal(5, pagina.Total);
        Assert.Equal(2, pagina.Items.Count);
    }

    [Fact]
    public async Task Listar_devuelve_el_dto_completo()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, nombre: "Farmacia A");
        var licencia = await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento,
            fechaEmision: Hoy.AddDays(-10), fechaVencimiento: Hoy.AddDays(100));

        var dto = Assert.Single((await ListarAsync(db)).Items);

        Assert.Equal(licencia.LicenciaSanitariaId, dto.LicenciaId);
        Assert.Equal(establecimiento.EstablecimientoId, dto.Establecimiento.EstablecimientoId);
        Assert.Equal("Farmacia A", dto.Establecimiento.Nombre);
        Assert.Equal("LIC-FARM", dto.TipoDocumento.Codigo);
        Assert.Equal("Vigente", dto.Estado);
        Assert.Equal("LIC-DEMO", dto.Numero);
        Assert.Equal(Hoy.AddDays(-10), dto.FechaEmision);
        Assert.Equal(Hoy.AddDays(100), dto.FechaVencimiento);
        Assert.Equal(100, dto.DiasParaVencer);
        Assert.False(dto.RequiereVerificacion);
        Assert.False(dto.TieneChecklist);
    }

    [Fact]
    public async Task Listar_marca_tiene_checklist()
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

        var items = (await ListarAsync(db)).Items
            .ToDictionary(i => i.Establecimiento.EstablecimientoId);

        Assert.True(items[conChecklist.EstablecimientoId].TieneChecklist);
        Assert.False(items[sinChecklist.EstablecimientoId].TieneChecklist);
    }

    [Fact]
    public async Task Validator_rechaza_estado_y_paginacion_invalidos()
    {
        var validator = new ListarLicenciasValidator();

        Assert.False((await validator.ValidateAsync(
            new ListarLicenciasRequest { Estado = "Invalido" })).IsValid);
        Assert.False((await validator.ValidateAsync(
            new ListarLicenciasRequest { Estado = "vigente" })).IsValid);
        Assert.False((await validator.ValidateAsync(
            new ListarLicenciasRequest { Pagina = 0 })).IsValid);
        Assert.False((await validator.ValidateAsync(
            new ListarLicenciasRequest { TamanoPagina = 0 })).IsValid);
        Assert.False((await validator.ValidateAsync(
            new ListarLicenciasRequest { TamanoPagina = 101 })).IsValid);
        Assert.True((await validator.ValidateAsync(
            new ListarLicenciasRequest { Estado = "Vigente", Pagina = 1, TamanoPagina = 100 })).IsValid);
    }
}
