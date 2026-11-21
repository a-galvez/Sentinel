
using Sentinel.Infrastructure.Seeding;

namespace Sentinel.Tests.Requisitos;

public class CatalogoSeederTests
{
    [Fact]
    public async Task Sembrar_dos_veces_no_duplica()
    {
        await using var db = DbContextEnMemoria.Crear();
        var seeder = new CatalogoSeeder(db);

        await seeder.SembrarAsync(default);
        var conteo1 = ContarTodo(db);

        await seeder.SembrarAsync(default);
        var conteo2 = ContarTodo(db);

        Assert.Equal(conteo1, conteo2);
    }

    [Fact]
    public async Task Siembra_las_plantillas_del_ejemplo_para_nueva()
    {
        await using var db = DbContextEnMemoria.Crear();

        await new CatalogoSeeder(db).SembrarAsync(default);

        // REG-MED (6) + LIC-FARM (5) + LIC-DROG (5), todas del trámite Nueva.
        Assert.Equal(16, db.DocumentosRequisito.Count());
    }

    [Fact]
    public async Task Siembra_las_tres_reglas_y_la_dependencia_del_ejemplo()
    {
        await using var db = DbContextEnMemoria.Crear();
        await new CatalogoSeeder(db).SembrarAsync(default);

        Assert.Equal(3, db.ReglasAplicabilidad.Count());
        Assert.Single(db.DocumentosDependencia);
    }

    [Fact]
    public async Task Plantilla_producto_tiene_seis_requisitos_ordenados()
    {
        await using var db = DbContextEnMemoria.Crear();
        await new CatalogoSeeder(db).SembrarAsync(default);

        var altaProducto = 1; // CatalogoIds.TiposTramite.AltaProducto
        var filas = db.DocumentosRequisito
            .Where(d => d.TipoDocumentoId == 1 && d.TipoTramiteId == altaProducto)
            .OrderBy(d => d.Orden)
            .ToList();

        Assert.Equal(6, filas.Count);
        Assert.Equal(Enumerable.Range(1, 6), filas.Select(f => f.Orden));
        Assert.All(filas, f => Assert.True(f.Obligatorio));
    }

    [Fact]
    public async Task Plantilla_licencia_drogueria_tiene_cinco_requisitos()
    {
        await using var db = DbContextEnMemoria.Crear();
        await new CatalogoSeeder(db).SembrarAsync(default);

        var altaEstablecimiento = 2; // CatalogoIds.TiposTramite.AltaEstablecimiento
        var filas = db.DocumentosRequisito
            .Where(d => d.TipoDocumentoId == 3 && d.TipoTramiteId == altaEstablecimiento)
            .ToList(); // 3 = LIC-DROG

        Assert.Equal(5, filas.Count);
    }

    private static (int tiposDocumento, int requisitos, int tiposTramite, int documentosRequisito) ContarTodo(
        Sentinel.Infrastructure.Persistence.AppDbContext db)
        => (db.TiposDocumento.Count(), db.Requisitos.Count(), db.TiposTramite.Count(), db.DocumentosRequisito.Count());
}
