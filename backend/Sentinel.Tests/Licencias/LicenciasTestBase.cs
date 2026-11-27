using Sentinel.API.Controllers.Licencias;
using Sentinel.Application.Features.LicenciasSanitarias.DarDeBaja;
using Sentinel.Application.Features.LicenciasSanitarias.Listar;
using Sentinel.Application.Features.LicenciasSanitarias.Obtener;
using Sentinel.Application.Features.LicenciasSanitarias.RegistrarEmision;
using Sentinel.Application.Features.LicenciasSanitarias.Verificar;
using Sentinel.Infrastructure.Persistence;
using Sentinel.Infrastructure.Repositories;
using Sentinel.Tests.Establecimientos;

namespace Sentinel.Tests.Licencias;

public abstract class LicenciasTestBase : EstablecimientosTestBase
{
    protected LicenciasController ObtenerLicenciaController(AppDbContext db) => Nuevo(db);
    protected LicenciasController ListarLicenciasController(AppDbContext db) => Nuevo(db);
    protected LicenciasController VerificarLicenciaController(AppDbContext db) => Nuevo(db);
    protected LicenciasController RegistrarEmisionLicenciaController(AppDbContext db) => Nuevo(db);
    protected LicenciasController DarDeBajaLicenciaController(AppDbContext db) => Nuevo(db);

    private LicenciasController Nuevo(AppDbContext db)
        => new(
            new ListarLicencias(
                new LicenciaRepository(db),
                new EstablecimientoRepository(db),
                new ChecklistRepository(db),
                new DocumentoRepository(db),
                Clock),
            new ListarLicenciasValidator(),
            new ObtenerLicencia(
                new EstablecimientoRepository(db),
                new LicenciaRepository(db),
                new ChecklistRepository(db),
                new DocumentoRepository(db),
                Clock),
            new RegistrarEmisionLicencia(
                new EstablecimientoRepository(db),
                new LicenciaRepository(db),
                new ChecklistRepository(db),
                new DocumentoRepository(db),
                new UnitOfWork(db),
                Clock),
            new RegistrarEmisionLicenciaValidator(),
            new DarDeBajaLicencia(
                new EstablecimientoRepository(db),
                new LicenciaRepository(db),
                new UnitOfWork(db)),
            new VerificarLicencia(
                new EstablecimientoRepository(db),
                new LicenciaRepository(db),
                new ChecklistRepository(db),
                new DocumentoRepository(db),
                new UnitOfWork(db),
                Clock,
                new VerificarLicenciaValidator()));
}
