using Sentinel.API.Controllers.Productos;
using Sentinel.API.Controllers.RegistrosSanitarios;
using Sentinel.Application.Features.Productos.Validators;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Persistence;
using Sentinel.Infrastructure.Repositories;
using Sentinel.Infrastructure.Seeding;
using Sentinel.Infrastructure.Services;
using Sentinel.Tests.Builders;
using Sentinel.Tests.Helpers;
using Sentinel.Tests.Requisitos;

namespace Sentinel.Tests.Productos;

public abstract class ProductosTestBase
{
    protected static readonly DateOnly Hoy = new(2026, 6, 15);
    protected readonly Guid EmpresaId = Guid.NewGuid();
    protected readonly Guid UsuarioId = Guid.NewGuid();

    protected async Task<AppDbContext> CrearBaseAsync()
    {
        var db = DbContextEnMemoria.Crear(new FakeTenant(EmpresaId, UsuarioId));
        await new CatalogoSeeder(db).SembrarAsync(default);

        var empresa = EmpresaBuilder.Crear("Farmacia Demo");
        empresa.EmpresaId = EmpresaId;
        db.Empresas.Add(empresa);

        await db.SaveChangesAsync(default);
        return db;
    }

    protected ProductosController CrearProductosController(AppDbContext db)
        => new(
            new ProductoRepository(db),
            new RegistroSanitarioRepository(db),
            new CatalogoRepository(db),
            new DocumentoRepository(db),
            new ChecklistRepository(db),
            new EmpresaRepository(db),
            new MotorRequisitos(db),
            new FakeTenant(EmpresaId, UsuarioId),
            new RelojFijo(Hoy),
            new UnitOfWork(db),
            new CrearProductoValidator(),
            new ProductoListadoValidator());

    protected RegistrosSanitariosController CrearRegistrosController(AppDbContext db)
        => new(
            new ProductoRepository(db),
            new RegistroSanitarioRepository(db),
            new DocumentoRepository(db),
            new ChecklistRepository(db),
            new FakeTenant(EmpresaId, UsuarioId),
            new RelojFijo(Hoy),
            new UnitOfWork(db),
            new VerificarRegistroValidator());

    protected Producto ProductoMedicamento()
        => ProductoBuilder.Crear(
            EmpresaId,
            categoriaProductoId: CatalogoIds.CategoriasProducto.Medicamento);

    protected RegistroSanitario RegistroNoVerificado(Guid productoId)
        => RegistroSanitarioBuilder.Crear(
            EmpresaId,
            productoId,
            tipoDocumentoId: CatalogoIds.TiposDocumento.RegistroSanitarioProducto);

    protected async Task<(Producto Producto, RegistroSanitario Registro)>
        SembrarProductoConRegistroAsync(AppDbContext db)
    {
        var producto = ProductoMedicamento();
        var registro = RegistroNoVerificado(producto.ProductoId);

        db.Productos.Add(producto);
        db.RegistrosSanitarios.Add(registro);
        await db.SaveChangesAsync(default);

        return (producto, registro);
    }

    protected async Task<Producto> SembrarProductoAsync(
        AppDbContext db,
        string nombre,
        EstadoVencimiento estado,
        string? numero = null,
        DateOnly? fechaEmision = null,
        DateOnly? fechaVencimiento = null,
        bool dadoDeBaja = false,
        Guid? empresaId = null)
    {
        var empresa = empresaId ?? EmpresaId;

        var producto = ProductoBuilder.Crear(
            empresa,
            nombre: nombre,
            categoriaProductoId: CatalogoIds.CategoriasProducto.Medicamento);

        producto.DadoDeBaja = dadoDeBaja;

        var registro = RegistroSanitarioBuilder.Crear(
            empresa,
            producto.ProductoId,
            numeroRegistro: numero,
            fechaEmision: fechaEmision,
            fechaVencimiento: fechaVencimiento,
            estado: estado);

        db.Productos.Add(producto);
        db.RegistrosSanitarios.Add(registro);
        await db.SaveChangesAsync(default);

        return producto;
    }
}
