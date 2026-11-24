using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Features.Requisitos;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Persistence;
using Sentinel.Infrastructure.Seeding;
using Sentinel.Infrastructure.Services;
using Sentinel.Tests.Builders;
using Sentinel.Tests.Requisitos;

namespace Sentinel.Tests.Requisitos;

// Pruebas del motor (H11 BE-10): reglas, copia al checklist, independencia del catálogo,
// dependencia (bloqueo/desbloqueo) y aislamiento entre empresas.
public class MotorRequisitosTests
{
    private static readonly DateOnly Hoy = new(2026, 11, 18);

    // ---------- Reglas ----------

    [Fact]
    public async Task Regla_que_aplica_asigna_documento_y_copia_requisitos()
    {
        var empresa = Guid.NewGuid();
        await using var db = await BaseSembradaAsync(empresa);
        var producto = ProductoBuilder.Crear(empresa, categoriaProductoId: CatalogoIds.CategoriasProducto.Medicamento);

        await new MotorRequisitos(db).AplicarAProductoAsync(producto, default);
        await db.SaveChangesAsync(default);

        Assert.Equal(CatalogoIds.TiposDocumento.RegistroSanitarioProducto, producto.TipoDocumentoId);
        var filas = await FilasDeProductoAsync(db, producto.ProductoId);
        Assert.Equal(6, filas.Count); // plantilla nueva de REG-MED
        Assert.All(filas, f => Assert.Equal(CatalogoIds.TiposTramite.AltaProducto, f.TipoTramiteId));
        Assert.All(filas, f => Assert.Equal(empresa, f.EmpresaId));
    }

    [Fact]
    public async Task Regla_que_no_aplica_deja_documento_nulo_y_sin_checklist()
    {
        var empresa = Guid.NewGuid();
        await using var db = await BaseSembradaAsync(empresa);

        var producto = ProductoBuilder.Crear(empresa, categoriaProductoId: CatalogoIds.CategoriasProducto.Cosmetico);

        await new MotorRequisitos(db).AplicarAProductoAsync(producto, default);
        await db.SaveChangesAsync(default);

        Assert.Null(producto.TipoDocumentoId);
        Assert.Empty(await FilasDeProductoAsync(db, producto.ProductoId));
    }

    [Fact]
    public async Task Con_varias_reglas_gana_la_de_menor_id()
    {
        var empresa = Guid.NewGuid();
        await using var db = await BaseSembradaAsync(empresa);

        // Otra regla que aplica al mismo producto pero con id mayor que R3 (id 3): no debe ganar.
        db.ReglasAplicabilidad.Add(new ReglaAplicabilidad
        {
            ReglaAplicabilidadId = 99,
            TipoDocumentoId = CatalogoIds.TiposDocumento.LicenciaSanitariaProducto,
            CategoriaProductoId = CatalogoIds.CategoriasProducto.Medicamento
        });
        await db.SaveChangesAsync(default);

        var producto = ProductoBuilder.Crear(empresa, categoriaProductoId: CatalogoIds.CategoriasProducto.Medicamento);
        await new MotorRequisitos(db).AplicarAProductoAsync(producto, default);

        Assert.Equal(CatalogoIds.TiposDocumento.RegistroSanitarioProducto, producto.TipoDocumentoId);
    }

    [Fact]
    public async Task Establecimiento_drogueria_asigna_licencia_drogueria()
    {
        var empresa = Guid.NewGuid();
        await using var db = await BaseSembradaAsync(empresa);
        var establecimiento = EstablecimientoBuilder.Crear(empresa, tipoEstablecimientoId: CatalogoIds.TiposEstablecimiento.Drogueria);
        db.Establecimientos.Add(establecimiento);
        await db.SaveChangesAsync(default);

        var licencia = LicenciaBuilder.Crear(empresa, establecimiento.EstablecimientoId);
        db.Licencias.Add(licencia);
        await new MotorRequisitos(db).AplicarALicenciaAsync(licencia, default);
        await db.SaveChangesAsync(default);

        Assert.Equal(CatalogoIds.TiposDocumento.LicenciaSanitariaProducto, licencia.TipoDocumentoId);
        Assert.Equal(5, db.ChecklistRequisitos.Count(c => c.LicenciaSanitariaId == licencia.LicenciaSanitariaId));
    }

    // ---------- Copia independiente del catálogo ----------

    [Fact]
    public async Task Cambiar_el_catalogo_no_altera_un_checklist_ya_creado()
    {
        var empresa = Guid.NewGuid();
        await using var db = await BaseSembradaAsync(empresa);
        var producto = ProductoBuilder.Crear(empresa, categoriaProductoId: CatalogoIds.CategoriasProducto.Medicamento);

        await new MotorRequisitos(db).AplicarAProductoAsync(producto, default);
        await db.SaveChangesAsync(default);
        var antes = await FilasDeProductoAsync(db, producto.ProductoId);

        // Se agrega un requisito nuevo a la plantilla después de crear el checklist.
        db.DocumentosRequisito.Add(new DocumentoRequisito
        {
            TipoDocumentoId = CatalogoIds.TiposDocumento.RegistroSanitarioProducto,
            TipoTramiteId = CatalogoIds.TiposTramite.AltaProducto,
            RequisitoId = CatalogoIds.Requisitos.Etiqueta + 100, // id que no choca
            Obligatorio = true,
            Orden = 99
        });
        await db.SaveChangesAsync(default);

        var despues = await FilasDeProductoAsync(db, producto.ProductoId);
        Assert.Equal(antes.Count, despues.Count);
    }

    // ---------- Dependencia (bloqueo / desbloqueo) ----------

    [Fact]
    public async Task Producto_medicamento_queda_bloqueado_sin_licencia_de_drogueria()
    {
        var empresa = Guid.NewGuid();
        await using var db = await BaseSembradaAsync(empresa);
        var producto = ProductoBuilder.Crear(empresa, categoriaProductoId: CatalogoIds.CategoriasProducto.Medicamento);
        db.Productos.Add(producto);
        await new MotorRequisitos(db).AplicarAProductoAsync(producto, default);
        await db.SaveChangesAsync(default);

        var checklist = await LeerAsync(db, producto.ProductoId);

        var bloqueo = Assert.Single(checklist.Bloqueo);
        Assert.Equal(CatalogoIds.TiposDocumento.LicenciaSanitariaProducto, bloqueo.TipoDocumentoId);
    }

    [Fact]
    public async Task Se_desbloquea_al_existir_una_licencia_de_drogueria_vigente()
    {
        var empresa = Guid.NewGuid();
        await using var db = await BaseSembradaAsync(empresa);
        var producto = ProductoBuilder.Crear(empresa, categoriaProductoId: CatalogoIds.CategoriasProducto.Medicamento);
        db.Productos.Add(producto);
        await new MotorRequisitos(db).AplicarAProductoAsync(producto, default);

        var establecimiento = EstablecimientoBuilder.Crear(empresa, tipoEstablecimientoId: CatalogoIds.TiposEstablecimiento.Drogueria);
        db.Establecimientos.Add(establecimiento);
        var licencia = LicenciaBuilder.Crear(empresa, establecimiento.EstablecimientoId,
            fechaEmision: Hoy, fechaVencimiento: Hoy.AddYears(1));
        licencia.TipoDocumentoId = CatalogoIds.TiposDocumento.LicenciaSanitariaProducto;
        db.Licencias.Add(licencia);
        await db.SaveChangesAsync(default);

        var checklist = await LeerAsync(db, producto.ProductoId);

        Assert.Empty(checklist.Bloqueo);
    }

    [Fact]
    public async Task Una_licencia_vencida_no_desbloquea()
    {
        var empresa = Guid.NewGuid();
        await using var db = await BaseSembradaAsync(empresa);
        var producto = ProductoBuilder.Crear(empresa, categoriaProductoId: CatalogoIds.CategoriasProducto.Medicamento);
        db.Productos.Add(producto);
        await new MotorRequisitos(db).AplicarAProductoAsync(producto, default);

        var establecimiento = EstablecimientoBuilder.Crear(empresa, tipoEstablecimientoId: CatalogoIds.TiposEstablecimiento.Drogueria);
        db.Establecimientos.Add(establecimiento);
        var licencia = LicenciaBuilder.Crear(empresa, establecimiento.EstablecimientoId,
            fechaEmision: Hoy.AddYears(-2), fechaVencimiento: Hoy.AddDays(-1));
        licencia.TipoDocumentoId = CatalogoIds.TiposDocumento.LicenciaSanitariaProducto;
        db.Licencias.Add(licencia);
        await db.SaveChangesAsync(default);

        var checklist = await LeerAsync(db, producto.ProductoId);

        Assert.Single(checklist.Bloqueo);
    }

    [Fact]
    public async Task Checklist_respeta_el_orden_del_catalogo_y_el_avance()
    {
        var empresa = Guid.NewGuid();
        await using var db = await BaseSembradaAsync(empresa);
        var producto = ProductoBuilder.Crear(empresa, categoriaProductoId: CatalogoIds.CategoriasProducto.Medicamento);
        db.Productos.Add(producto);
        await new MotorRequisitos(db).AplicarAProductoAsync(producto, default);
        await db.SaveChangesAsync(default);

        var checklist = await LeerAsync(db, producto.ProductoId);

        Assert.Equal(Enumerable.Range(1, 6), checklist.Requisitos.Select(r => r.Orden));
        Assert.Equal(0, checklist.Avance.Cumplidos);
        Assert.Equal(6, checklist.Avance.Total);
        Assert.Equal("REG-MED", checklist.TipoDocumento!.Codigo);
    }

    // ---------- Aislamiento entre empresas ----------

    [Fact]
    public async Task Checklist_de_otra_empresa_responde_no_encontrado()
    {
        var empresaA = Guid.NewGuid();
        var tenant = new TenantMutable(empresaA);

        await using var db = DbContextEnMemoria.Crear(tenant);
        await new CatalogoSeeder(db).SembrarAsync(default);

        var producto = ProductoBuilder.Crear(empresaA, categoriaProductoId: CatalogoIds.CategoriasProducto.Medicamento);
        db.Productos.Add(producto);
        await new MotorRequisitos(db).AplicarAProductoAsync(producto, default);
        await db.SaveChangesAsync(default);

        var reader = new ChecklistReader(db, new EvaluadorDependencias(db, new RelojFijo(Hoy)));

        // Con la empresa correcta sí lo ve.
        Assert.NotNull(await reader.ParaProductoAsync(producto.ProductoId, default));

        // Con otra empresa en sesión, el filtro global lo esconde → 404.
        tenant.CambiarEmpresa(Guid.NewGuid());
        await Assert.ThrowsAsync<Sentinel.Application.Common.Exceptions.NotFoundException>(
            () => reader.ParaProductoAsync(producto.ProductoId, default));
    }

    // ---------- Helpers ----------

    private static async Task<AppDbContext> BaseSembradaAsync(Guid enSesion)
    {
        // Con sesión: el filtro global deja ver las filas de esta empresa (las de prueba).
        var ctx = DbContextEnMemoria.Crear(new FakeTenant(enSesion));
        await new CatalogoSeeder(ctx).SembrarAsync(default);
        return ctx;
    }

    private static Task<List<ChecklistRequisito>> FilasDeProductoAsync(AppDbContext db, Guid productoId)
        => db.ChecklistRequisitos.Where(c => c.ProductoId == productoId).ToListAsync();

    private static Task<Sentinel.Application.Features.Requisitos.ChecklistDto> LeerAsync(AppDbContext db, Guid productoId)
        => new Sentinel.Application.Features.Requisitos.ChecklistReader(db, new EvaluadorDependencias(db, new RelojFijo(Hoy)))
            .ParaProductoAsync(productoId, default);

    private sealed class RelojFijo(DateOnly hoy) : IClock
    {
        public DateOnly HoyHonduras => hoy;
        public DateTime UtcAhora => hoy.ToDateTime(TimeOnly.MinValue);
    }

    private sealed class TenantMutable(Guid empresaId) : ICurrentTenant
    {
        private Guid _empresaId = empresaId;
        public void CambiarEmpresa(Guid empresaId) => _empresaId = empresaId;
        public bool TieneSesion => true;
        public Guid EmpresaId => _empresaId;
        public Guid UsuarioId => Guid.NewGuid();
    }
}
