
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
    public async Task Siembra_las_cinco_reglas_de_la_plantilla()
    {
        await using var db = DbContextEnMemoria.Crear();

        await new CatalogoSeeder(db).SembrarAsync(default);

        Assert.Equal(5, db.DocumentosRequisito.Count());
    }

    [Fact]
    public async Task Plantilla_producto_tiene_dos_requisitos_ordenados()
    {
        await using var db = DbContextEnMemoria.Crear();
        await new CatalogoSeeder(db).SembrarAsync(default);

        var altaProducto = 1; // CatalogoIds.TiposTramite.AltaProducto
        var filas = db.DocumentosRequisito
            .Where(d => d.TipoTramiteId == altaProducto)
            .OrderBy(d => d.Orden)
            .ToList();

        Assert.Equal(2, filas.Count);
        Assert.Equal(1, filas[0].Orden);
        Assert.Equal(2, filas[1].Orden);
        Assert.All(filas, f => Assert.True(f.Obligatorio));
    }

    [Fact]
    public async Task Plantilla_establecimiento_tiene_tres_requisitos()
    {
        await using var db = DbContextEnMemoria.Crear();
        await new CatalogoSeeder(db).SembrarAsync(default);

        var altaEstablecimiento = 2; // CatalogoIds.TiposTramite.AltaEstablecimiento
        var filas = db.DocumentosRequisito
            .Where(d => d.TipoTramiteId == altaEstablecimiento)
            .ToList();

        Assert.Equal(3, filas.Count);
    }

    private static (int tiposDocumento, int requisitos, int tiposTramite, int documentosRequisito) ContarTodo(
        Sentinel.Infrastructure.Persistence.AppDbContext db)
        => (db.TiposDocumento.Count(), db.Requisitos.Count(), db.TiposTramite.Count(), db.DocumentosRequisito.Count());
}
