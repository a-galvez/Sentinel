using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Common;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.Establecimientos;
using Sentinel.Application.Features.Establecimientos.Crear;
using Sentinel.Application.Features.Establecimientos.Listar;
using Sentinel.Domain.Constantes;

namespace Sentinel.Tests.Establecimientos;

public class EstablecimientosControllerTests : EstablecimientosTestBase
{
    private static CrearEstablecimientoRequest Request(
        string nombre = "Farmacia Central",
        int tipo = CatalogoIds.TiposEstablecimiento.Farmacia,
        string ubicacion = "Tegucigalpa")
        => new() { Nombre = nombre, TipoEstablecimientoId = tipo, Ubicacion = ubicacion };

    [Fact]
    public async Task Crear_con_regla_devuelve_201_y_licencia_no_verificada()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearController(db);

        var action = await controller.Crear(Request(), default);

        var created = Assert.IsType<CreatedResult>(action.Result);
        var response = Assert.IsType<EstablecimientoCreadoDto>(created.Value);
        Assert.Null(response.Mensaje);
        Assert.Equal(
            $"/api/v1/establecimientos/{response.Establecimiento.EstablecimientoId}",
            created.Location);

        var licencia = response.Establecimiento.Licencia;
        Assert.NotNull(licencia);
        Assert.Equal("NoVerificado", licencia!.Estado);
        Assert.True(licencia.RequiereVerificacion);
        Assert.False(licencia.TieneChecklist);
        Assert.Equal(
            CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento,
            licencia.TipoDocumento.TipoDocumentoId);

        Assert.Equal(1, await db.Establecimientos.CountAsync());
        Assert.Equal(1, await db.Licencias.CountAsync());
        Assert.Equal(0, await db.ChecklistRequisitos.CountAsync());
    }

    [Fact]
    public async Task Crear_sin_regla_devuelve_201_con_mensaje_y_sin_licencia()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearController(db);

        var action = await controller.Crear(
            Request(tipo: CatalogoIds.TiposEstablecimiento.Botica), default);

        var created = Assert.IsType<CreatedResult>(action.Result);
        var response = Assert.IsType<EstablecimientoCreadoDto>(created.Value);
        Assert.NotNull(response.Mensaje);
        Assert.Null(response.Establecimiento.Licencia);

        Assert.Equal(1, await db.Establecimientos.CountAsync());
        Assert.Equal(0, await db.Licencias.CountAsync());
        Assert.Equal(0, await db.ChecklistRequisitos.CountAsync());
    }

    [Fact]
    public async Task Crear_tipo_inexistente_lanza_validacion_en_el_campo()
    {
        await using var db = await CrearBaseAsync();
        var controller = CrearController(db);

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => controller.Crear(Request(tipo: 99), default));

        Assert.Contains(ex.Errors, e => e.PropertyName == "tipoEstablecimientoId");
        Assert.Equal(0, await db.Establecimientos.CountAsync());
    }

    [Fact]
    public async Task Validator_usa_las_longitudes_del_contrato()
    {
        var validator = new CrearEstablecimientoValidator();

        var valido = await validator.ValidateAsync(Request(
            nombre: new string('a', 200),
            ubicacion: new string('b', 300)));
        Assert.True(valido.IsValid);

        var nombreLargo = await validator.ValidateAsync(Request(nombre: new string('a', 201)));
        Assert.False(nombreLargo.IsValid);
        Assert.Contains(nombreLargo.Errors, e => e.PropertyName == "Nombre");

        var ubicacionLarga = await validator.ValidateAsync(Request(ubicacion: new string('b', 301)));
        Assert.False(ubicacionLarga.IsValid);
        Assert.Contains(ubicacionLarga.Errors, e => e.PropertyName == "Ubicacion");
    }

    [Fact]
    public async Task Listar_devuelve_solo_establecimientos_de_la_empresa_y_activos()
    {
        await using var db = await CrearBaseAsync();

        var farmacia = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, nombre: "Farmacia A");
        await SembrarLicenciaAsync(
            db,
            farmacia.EstablecimientoId,
            CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento,
            fechaEmision: Hoy,
            fechaVencimiento: Hoy.AddYears(1));

        await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Botica, nombre: "Botica B");

        await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, empresaId: Guid.NewGuid(), nombre: "Otra empresa");
        await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, dadoDeBaja: true, nombre: "Baja");

        var controller = ListarController(db);
        var action = await controller.Listar(new ListarEstablecimientosRequest(), default);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var pagina = Assert.IsType<PaginaDto<EstablecimientoDto>>(ok.Value);

        Assert.Equal(2, pagina.Total);
        Assert.Equal(2, pagina.Items.Count);

        var farmaciaDto = pagina.Items.Single(i => i.EstablecimientoId == farmacia.EstablecimientoId);
        Assert.NotNull(farmaciaDto.Licencia);
        Assert.Equal("Vigente", farmaciaDto.Licencia!.Estado);

        var boticaDto = pagina.Items.Single(i => i.Nombre == "Botica B");
        Assert.Null(boticaDto.Licencia);
    }

    [Fact]
    public async Task DarDeBaja_marca_establecimiento_y_licencia()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia);
        var licencia = await SembrarLicenciaAsync(
            db,
            establecimiento.EstablecimientoId,
            CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento);

        var controller = DarDeBajaController(db);
        var result = await controller.DarDeBaja(establecimiento.EstablecimientoId, default);

        Assert.IsType<NoContentResult>(result);

        var establecimientoGuardado = await db.Establecimientos
            .IgnoreQueryFilters()
            .SingleAsync(e => e.EstablecimientoId == establecimiento.EstablecimientoId);
        var licenciaGuardada = await db.Licencias
            .IgnoreQueryFilters()
            .SingleAsync(l => l.LicenciaSanitariaId == licencia.LicenciaSanitariaId);

        Assert.True(establecimientoGuardado.DadoDeBaja);
        Assert.True(licenciaGuardada.DadoDeBaja);
    }

    [Fact]
    public async Task DarDeBaja_sin_licencia_solo_marca_el_establecimiento()
    {
        await using var db = await CrearBaseAsync();
        var establecimiento = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Botica);

        var controller = DarDeBajaController(db);
        var result = await controller.DarDeBaja(establecimiento.EstablecimientoId, default);

        Assert.IsType<NoContentResult>(result);

        var establecimientoGuardado = await db.Establecimientos
            .IgnoreQueryFilters()
            .SingleAsync(e => e.EstablecimientoId == establecimiento.EstablecimientoId);

        Assert.True(establecimientoGuardado.DadoDeBaja);
        Assert.Equal(0, await db.Licencias.IgnoreQueryFilters().CountAsync());
    }

    [Fact]
    public async Task DarDeBaja_ya_dado_de_baja_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var baja = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, dadoDeBaja: true);

        var controller = DarDeBajaController(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => controller.DarDeBaja(baja.EstablecimientoId, default));
    }

    [Fact]
    public async Task DarDeBaja_otra_empresa_devuelve_404()
    {
        await using var db = await CrearBaseAsync();
        var ajena = await SembrarEstablecimientoAsync(
            db, CatalogoIds.TiposEstablecimiento.Farmacia, empresaId: Guid.NewGuid());

        var controller = DarDeBajaController(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => controller.DarDeBaja(ajena.EstablecimientoId, default));
    }
}
