using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Common;
using Sentinel.Application.Features.Establecimientos;
using Sentinel.Application.Features.Establecimientos.Listar;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Tests.Establecimientos;

public class ListarEstablecimientosTests : EstablecimientosTestBase
{
    private const int TipoDocumentoEstablecimiento =
        CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento;

    private async Task<PaginaDto<EstablecimientoDto>> ListarAsync(
        AppDbContext db, int pagina = 1, int tamano = 20)
    {
        var controller = ListarController(db);
        var action = await controller.Listar(
            new ListarEstablecimientosRequest { Pagina = pagina, TamanoPagina = tamano }, default);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        return Assert.IsType<PaginaDto<EstablecimientoDto>>(ok.Value);
    }

    [Fact]
    public async Task Listar_sin_parametros_usa_pagina_1_y_tamano_20()
    {
        await using var db = await CrearBaseAsync();
        await SembrarEstablecimientoAsync(db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarEstablecimientoAsync(db, CatalogoIds.TiposEstablecimiento.Botica);

        var pagina = await ListarAsync(db);

        Assert.Equal(1, pagina.Pagina);
        Assert.Equal(20, pagina.TamanoPagina);
        Assert.Equal(2, pagina.Total);
        Assert.Equal(2, pagina.Items.Count);
    }

    [Fact]
    public async Task Listar_respeta_pagina_y_tamano()
    {
        await using var db = await CrearBaseAsync();
        for (var i = 1; i <= 5; i++)
            await SembrarEstablecimientoAsync(
                db, CatalogoIds.TiposEstablecimiento.Farmacia, nombre: $"Est {i:00}");

        var pagina = await ListarAsync(db, pagina: 2, tamano: 2);

        Assert.Equal(2, pagina.Pagina);
        Assert.Equal(2, pagina.TamanoPagina);
        Assert.Equal(5, pagina.Total);
        Assert.Equal(new[] { "Est 03", "Est 04" }, pagina.Items.Select(i => i.Nombre));
    }

    [Fact]
    public async Task Listar_pagina_fuera_de_rango_devuelve_items_vacios_y_total_real()
    {
        await using var db = await CrearBaseAsync();
        for (var i = 1; i <= 3; i++)
            await SembrarEstablecimientoAsync(db, CatalogoIds.TiposEstablecimiento.Farmacia);

        var pagina = await ListarAsync(db, pagina: 99);

        Assert.Equal(3, pagina.Total);
        Assert.Empty(pagina.Items);
    }

    [Fact]
    public async Task Listar_ordena_por_nombre()
    {
        await using var db = await CrearBaseAsync();
        await SembrarEstablecimientoAsync(db, CatalogoIds.TiposEstablecimiento.Farmacia, nombre: "Zeta");
        await SembrarEstablecimientoAsync(db, CatalogoIds.TiposEstablecimiento.Farmacia, nombre: "Alfa");
        await SembrarEstablecimientoAsync(db, CatalogoIds.TiposEstablecimiento.Farmacia, nombre: "Mango");

        var pagina = await ListarAsync(db);

        Assert.Equal(new[] { "Alfa", "Mango", "Zeta" }, pagina.Items.Select(i => i.Nombre));
    }

    [Fact]
    public async Task Listar_calcula_el_estado_de_la_licencia_por_fecha()
    {
        await using var db = await CrearBaseAsync();

        var vencido = await SembrarEstablecimientoAsync(db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, vencido.EstablecimientoId, TipoDocumentoEstablecimiento,
            fechaEmision: Hoy.AddYears(-1), fechaVencimiento: Hoy.AddDays(-1));

        var porVencer = await SembrarEstablecimientoAsync(db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, porVencer.EstablecimientoId, TipoDocumentoEstablecimiento,
            fechaEmision: Hoy, fechaVencimiento: Hoy.AddDays(30));

        var vigente = await SembrarEstablecimientoAsync(db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, vigente.EstablecimientoId, TipoDocumentoEstablecimiento,
            fechaEmision: Hoy, fechaVencimiento: Hoy.AddDays(91));

        var noVerificado = await SembrarEstablecimientoAsync(db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, noVerificado.EstablecimientoId, TipoDocumentoEstablecimiento,
            estado: EstadoVencimiento.NoVerificado);

        var enTramite = await SembrarEstablecimientoAsync(db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, enTramite.EstablecimientoId, TipoDocumentoEstablecimiento,
            estado: EstadoVencimiento.EnTramite);

        var items = (await ListarAsync(db)).Items.ToDictionary(i => i.EstablecimientoId);

        var vencidoDto = items[vencido.EstablecimientoId].Licencia!;
        Assert.Equal("Vencido", vencidoDto.Estado);
        Assert.Equal(-1, vencidoDto.DiasParaVencer);
        Assert.False(vencidoDto.RequiereVerificacion);

        var porVencerDto = items[porVencer.EstablecimientoId].Licencia!;
        Assert.Equal("PorVencer", porVencerDto.Estado);
        Assert.Equal(30, porVencerDto.DiasParaVencer);

        var vigenteDto = items[vigente.EstablecimientoId].Licencia!;
        Assert.Equal("Vigente", vigenteDto.Estado);
        Assert.Equal(91, vigenteDto.DiasParaVencer);

        var noVerificadoDto = items[noVerificado.EstablecimientoId].Licencia!;
        Assert.Equal("NoVerificado", noVerificadoDto.Estado);
        Assert.Null(noVerificadoDto.DiasParaVencer);
        Assert.True(noVerificadoDto.RequiereVerificacion);

        var enTramiteDto = items[enTramite.EstablecimientoId].Licencia!;
        Assert.Equal("EnTramite", enTramiteDto.Estado);
        Assert.Null(enTramiteDto.DiasParaVencer);
        Assert.False(enTramiteDto.RequiereVerificacion);
    }

    [Fact]
    public async Task Listar_licencia_sin_tipo_documento_se_muestra_sin_licencia()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        var licencia = await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento);

        licencia.TipoDocumentoId = null;
        await db.SaveChangesAsync();

        var dto = (await ListarAsync(db)).Items.Single();
        Assert.Null(dto.Licencia);
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

        var items = (await ListarAsync(db)).Items.ToDictionary(i => i.EstablecimientoId);

        Assert.True(items[conChecklist.EstablecimientoId].Licencia!.TieneChecklist);
        Assert.False(items[sinChecklist.EstablecimientoId].Licencia!.TieneChecklist);
    }

    [Fact]
    public async Task Listar_oculta_la_licencia_dada_de_baja()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        await SembrarLicenciaAsync(
            db, establecimiento.EstablecimientoId, TipoDocumentoEstablecimiento, dadoDeBaja: true);

        var dto = (await ListarAsync(db)).Items.Single();
        Assert.Null(dto.Licencia);
    }

    [Fact]
    public async Task Listar_no_mezcla_establecimientos_de_otra_empresa()
    {
        await using var db = await CrearBaseAsync();
        await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, nombre: "Propia");
        await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, empresaId: Guid.NewGuid(), nombre: "Ajena");

        var pagina = await ListarAsync(db);

        Assert.Equal(1, pagina.Total);
        Assert.Equal("Propia", pagina.Items.Single().Nombre);
    }

    [Fact]
    public async Task Validator_rechaza_pagina_y_tamano_invalidos()
    {
        var validator = new ListarEstablecimientosValidator();

        var paginaCero = await validator.ValidateAsync(
            new ListarEstablecimientosRequest { Pagina = 0 });
        Assert.False(paginaCero.IsValid);
        Assert.Contains(paginaCero.Errors, e => e.PropertyName == "Pagina");

        var tamanoCero = await validator.ValidateAsync(
            new ListarEstablecimientosRequest { TamanoPagina = 0 });
        Assert.False(tamanoCero.IsValid);
        Assert.Contains(tamanoCero.Errors, e => e.PropertyName == "TamanoPagina");

        var tamanoExcesivo = await validator.ValidateAsync(
            new ListarEstablecimientosRequest { TamanoPagina = 101 });
        Assert.False(tamanoExcesivo.IsValid);

        var valido = await validator.ValidateAsync(
            new ListarEstablecimientosRequest { Pagina = 1, TamanoPagina = 100 });
        Assert.True(valido.IsValid);
    }
}
