using Sentinel.API.Controllers.Establecimientos;
using Sentinel.Application.Abstractions;
using Sentinel.Domain.Constantes;
using Sentinel.Application.Features.Establecimientos.Crear;
using Sentinel.Application.Features.Establecimientos.Listar;
using Sentinel.Application.Features.Establecimientos.Obtener;
using Sentinel.Application.Features.Establecimientos.DarDeBaja;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Persistence;
using Sentinel.Infrastructure.Repositories;
using Sentinel.Infrastructure.Seeding;
using Sentinel.Infrastructure.Services;
using Sentinel.Tests.Builders;
using Sentinel.Tests.Helpers;
using Sentinel.Tests.Requisitos;

namespace Sentinel.Tests.Establecimientos;

public abstract class EstablecimientosTestBase
{
    protected static readonly DateOnly Hoy = new(2026, 6, 15);
    protected readonly Guid EmpresaId = Guid.NewGuid();
    protected readonly Guid UsuarioId = Guid.NewGuid();

    protected FakeTenant Tenant => new(EmpresaId, UsuarioId);
    protected IClock Clock => new RelojFijo(Hoy);

    protected async Task<AppDbContext> CrearBaseAsync()
    {
        var db = DbContextEnMemoria.Crear(Tenant);
        await new CatalogoSeeder(db).SembrarAsync(default);
        return db;
    }

    protected EstablecimientosController CrearController(AppDbContext db) => Nuevo(db);
    protected EstablecimientosController ListarController(AppDbContext db) => Nuevo(db);
    protected EstablecimientosController ObtenerController(AppDbContext db) => Nuevo(db);
    protected EstablecimientosController DarDeBajaController(AppDbContext db) => Nuevo(db);

    private EstablecimientosController Nuevo(AppDbContext db)
        => new(
            new CrearEstablecimiento(
                new EstablecimientoRepository(db),
                new LicenciaRepository(db),
                new CatalogoRepository(db),
                new DocumentoRepository(db),
                new MotorRequisitos(db),
                Tenant,
                new UnitOfWork(db),
                Clock),
            new CrearEstablecimientoValidator(),
            new ListarEstablecimientos(
                new EstablecimientoRepository(db),
                new LicenciaRepository(db),
                new ChecklistRepository(db),
                new DocumentoRepository(db),
                new CatalogoRepository(db),
                Clock),
            new ListarEstablecimientosValidator(),
            new ObtenerEstablecimiento(
                new EstablecimientoRepository(db),
                new LicenciaRepository(db),
                new ChecklistRepository(db),
                new DocumentoRepository(db),
                new CatalogoRepository(db),
                Clock),
            new DarDeBajaEstablecimiento(
                new EstablecimientoRepository(db),
                new LicenciaRepository(db),
                new UnitOfWork(db)));

    protected async Task<Establecimiento> SembrarEstablecimientoAsync(
        AppDbContext db,
        int tipoEstablecimientoId,
        Guid? empresaId = null,
        bool dadoDeBaja = false,
        string? nombre = null)
    {
        var establecimiento = EstablecimientoBuilder.Crear(
            empresaId ?? EmpresaId,
            tipoEstablecimientoId: tipoEstablecimientoId,
            nombre: nombre ?? $"Establecimiento {Guid.NewGuid():N}");

        establecimiento.DadoDeBaja = dadoDeBaja;

        db.Establecimientos.Add(establecimiento);
        await db.SaveChangesAsync();

        return establecimiento;
    }

    protected async Task<LicenciaSanitaria> SembrarLicenciaAsync(
        AppDbContext db,
        Guid establecimientoId,
        int tipoDocumentoId,
        Guid? empresaId = null,
        DateOnly? fechaEmision = null,
        DateOnly? fechaVencimiento = null,
        bool dadoDeBaja = false,
        EstadoVencimiento estado = EstadoVencimiento.NoVerificado)
    {
        var licencia = new LicenciaSanitaria
        {
            LicenciaSanitariaId = Guid.NewGuid(),
            EstablecimientoId = establecimientoId,
            EmpresaId = empresaId ?? EmpresaId,
            TipoDocumentoId = tipoDocumentoId,
            Estado = estado,
            NumeroLicencia = fechaEmision is null ? null : "LIC-DEMO",
            FechaEmision = fechaEmision,
            FechaVencimiento = fechaVencimiento,
            DadoDeBaja = dadoDeBaja
        };

        db.Licencias.Add(licencia);
        await db.SaveChangesAsync();

        return licencia;
    }

    protected async Task<ChecklistRequisito> SembrarChecklistAsync(
        AppDbContext db,
        Guid licenciaId,
        Guid? empresaId = null)
    {
        var fila = new ChecklistRequisito
        {
            ChecklistRequisitoId = Guid.NewGuid(),
            EmpresaId = empresaId ?? EmpresaId,
            RequisitoId = CatalogoIds.Requisitos.SolicitudArsa,
            TipoTramiteId = CatalogoIds.TiposTramite.AltaEstablecimiento,
            LicenciaSanitariaId = licenciaId,
            Estado = EstadoRequisito.Pendiente,
            Orden = 1
        };

        db.ChecklistRequisitos.Add(fila);
        await db.SaveChangesAsync();

        return fila;
    }
}
