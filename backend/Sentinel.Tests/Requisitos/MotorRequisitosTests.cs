
using Sentinel.Domain.Constantes;
using Sentinel.Infrastructure.Seeding;
using Sentinel.Infrastructure.Services;
using Sentinel.Tests.Builders;

namespace Sentinel.Tests.Requisitos;

// Pruebas del motor: solo responde "qué documento le toca".
// Se siembra el catálogo real (R1..R3) con CatalogoSeeder.
public class MotorRequisitosTests
{
    private static readonly Guid EmpresaId = Guid.NewGuid();

    private static async Task<(Infrastructure.Persistence.AppDbContext db, MotorRequisitos motor)> PrepararAsync()
    {
        var db = DbContextEnMemoria.Crear();
        await new CatalogoSeeder(db).SembrarAsync(default);
        return (db, new MotorRequisitos(db));
    }

    // R3: Medicamento → REG-MED.
    [Fact]
    public async Task Producto_medicamento_devuelve_registro_de_medicamento()
    {
        var (db, motor) = await PrepararAsync();
        await using var _ = db;

        var producto = ProductoBuilder.Crear(EmpresaId,
            categoriaProductoId: CatalogoIds.CategoriasProducto.Medicamento);

        var resultado = await motor.ElegirDocumentoParaProductoAsync(producto, default);

        Assert.Equal(CatalogoIds.TiposDocumento.RegistroSanitarioProducto, resultado);
    }

    // Sin regla para la categoría: null (H11 "no aplica").
    [Theory]
    [InlineData(CatalogoIds.CategoriasProducto.Cosmetico)]
    [InlineData(CatalogoIds.CategoriasProducto.Alimento)]
    [InlineData(CatalogoIds.CategoriasProducto.DispositivoMedico)]
    public async Task Producto_sin_regla_devuelve_null(int categoria)
    {
        var (db, motor) = await PrepararAsync();
        await using var _ = db;

        var producto = ProductoBuilder.Crear(EmpresaId, categoriaProductoId: categoria);

        var resultado = await motor.ElegirDocumentoParaProductoAsync(producto, default);

        Assert.Null(resultado);
    }

    // R1: Farmacia → LIC-FARM.
    [Fact]
    public async Task Establecimiento_farmacia_devuelve_licencia_funcionamiento()
    {
        var (db, motor) = await PrepararAsync();
        await using var _ = db;

        var establecimiento = EstablecimientoBuilder.Crear(EmpresaId,
            tipoEstablecimientoId: CatalogoIds.TiposEstablecimiento.Farmacia);

        var resultado = await motor.ElegirDocumentoParaEstablecimientoAsync(establecimiento, default);

        Assert.Equal(CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento, resultado);
    }

    // R2: Droguería → LIC-DROG.
    [Fact]
    public async Task Establecimiento_drogueria_devuelve_licencia_sanitaria()
    {
        var (db, motor) = await PrepararAsync();
        await using var _ = db;

        var establecimiento = EstablecimientoBuilder.Crear(EmpresaId,
            tipoEstablecimientoId: CatalogoIds.TiposEstablecimiento.Drogueria);

        var resultado = await motor.ElegirDocumentoParaEstablecimientoAsync(establecimiento, default);

        Assert.Equal(CatalogoIds.TiposDocumento.LicenciaSanitariaProducto, resultado);
    }

    // Botica no tiene regla: null.
    [Fact]
    public async Task Establecimiento_sin_regla_devuelve_null()
    {
        var (db, motor) = await PrepararAsync();
        await using var _ = db;

        var establecimiento = EstablecimientoBuilder.Crear(EmpresaId,
            tipoEstablecimientoId: CatalogoIds.TiposEstablecimiento.Botica);

        var resultado = await motor.ElegirDocumentoParaEstablecimientoAsync(establecimiento, default);

        Assert.Null(resultado);
    }

    // Si varias reglas aplican, gana la de menor ReglaAplicabilidadId (H11 BE-4).
    // TODO: cambiar el criterio a Prioridad cuando ese campo exista.
    [Fact]
    public async Task Cuando_dos_reglas_aplican_gana_la_de_menor_id()
    {
        var (db, motor) = await PrepararAsync();
        await using var _ = db;

        // Regla adicional para Farmacia: compite directamente con R1 (id 1).
        // En InMemory el id lo asigna EF, así que leemos el que le tocó al guardar.
        var regla = new Domain.Entities.ReglaAplicabilidad
        {
            TipoDocumentoId = CatalogoIds.TiposDocumento.RegistroPropietarioEstablecimiento,
            TipoEstablecimientoId = CatalogoIds.TiposEstablecimiento.Farmacia,
            CategoriaProductoId = null
        };
        db.ReglasAplicabilidad.Add(regla);
        await db.SaveChangesAsync();

        // Premisa del test: la regla nueva tiene id mayor que R1, si no el desempate
        // no probaría nada y el test podría pasar por accidente.
        Assert.True(regla.ReglaAplicabilidadId > CatalogoIds.Reglas.LicenciaFarmacia);

        var establecimiento = EstablecimientoBuilder.Crear(EmpresaId,
            tipoEstablecimientoId: CatalogoIds.TiposEstablecimiento.Farmacia);

        var resultado = await motor.ElegirDocumentoParaEstablecimientoAsync(establecimiento, default);

        // Ambas reglas aplican; gana R1 por tener el id menor.
        Assert.Equal(CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento, resultado);
    }

    // Una regla con las dos condiciones en null aplica a cualquier producto.
    [Fact]
    public async Task Regla_cualquiera_aplica_a_cualquier_categoria()
    {
        var (db, motor) = await PrepararAsync();
        await using var _ = db;

        // Sin condiciones: aplica a cualquier categoría y a cualquier tipo de establecimiento.
        db.ReglasAplicabilidad.Add(new Domain.Entities.ReglaAplicabilidad
        {
            TipoDocumentoId = CatalogoIds.TiposDocumento.PermisoImportacionProducto,
            TipoEstablecimientoId = null,
            CategoriaProductoId = null
        });
        await db.SaveChangesAsync();

        // Cosmético no tiene regla propia, así que la única candidata es esta.
        var producto = ProductoBuilder.Crear(EmpresaId,
            categoriaProductoId: CatalogoIds.CategoriasProducto.Cosmetico);

        var resultado = await motor.ElegirDocumentoParaProductoAsync(producto, default);

        Assert.Equal(CatalogoIds.TiposDocumento.PermisoImportacionProducto, resultado);
    }

    // El motor es de solo lectura: no agrega ni modifica entidades.
    [Fact]
    public async Task Motor_no_modifica_el_change_tracker()
    {
        var (db, motor) = await PrepararAsync();
        await using var _ = db;

        db.ChangeTracker.Clear(); // descarta el seeder

        var producto = ProductoBuilder.Crear(EmpresaId,
            categoriaProductoId: CatalogoIds.CategoriasProducto.Medicamento);

        await motor.ElegirDocumentoParaProductoAsync(producto, default);

        Assert.DoesNotContain(db.ChangeTracker.Entries(),
            e => e.State != Microsoft.EntityFrameworkCore.EntityState.Unchanged);
    }
}
