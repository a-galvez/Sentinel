using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Repositories;
using Sentinel.Tests.Builders;
using Sentinel.Tests.Requisitos;

namespace Sentinel.Tests.Productos;

public class RegistroSanitarioRepositoryTests
{
    [Fact]
    public async Task ObtenerPorProducto_devuelve_el_registro_de_la_empresa()
    {
        var empresa = Guid.NewGuid();
        await using var db = DbContextEnMemoria.Crear(new FakeTenant(empresa));

        var producto = ProductoBuilder.Crear(empresa);
        var registro = RegistroSanitarioBuilder.Crear(empresa, producto.ProductoId);

        db.Productos.Add(producto);
        db.RegistrosSanitarios.Add(registro);
        await db.SaveChangesAsync(default);

        var repo = new RegistroSanitarioRepository(db);

        var encontrado = await repo.ObtenerPorProductoAsync(producto.ProductoId, default);

        Assert.NotNull(encontrado);
        Assert.Equal(registro.RegistroSanitarioId, encontrado!.RegistroSanitarioId);
    }

    [Fact]
    public async Task ObtenerPorProducto_respeta_otra_empresa()
    {
        var empresaA = Guid.NewGuid();
        var empresaB = Guid.NewGuid();
        await using var db = DbContextEnMemoria.Crear(new FakeTenant(empresaA));

        var producto = ProductoBuilder.Crear(empresaB);
        var registro = RegistroSanitarioBuilder.Crear(empresaB, producto.ProductoId);

        db.Productos.Add(producto);
        db.RegistrosSanitarios.Add(registro);
        await db.SaveChangesAsync(default);

        var repo = new RegistroSanitarioRepository(db);

        Assert.Null(await repo.ObtenerPorProductoAsync(producto.ProductoId, default));
    }

    [Fact]
    public async Task ObtenerPorProducto_oculta_registro_dado_de_baja()
    {
        var empresa = Guid.NewGuid();
        await using var db = DbContextEnMemoria.Crear(new FakeTenant(empresa));

        var producto = ProductoBuilder.Crear(empresa);
        var registro = RegistroSanitarioBuilder.Crear(empresa, producto.ProductoId);
        registro.DadoDeBaja = true;

        db.Productos.Add(producto);
        db.RegistrosSanitarios.Add(registro);
        await db.SaveChangesAsync(default);

        var repo = new RegistroSanitarioRepository(db);

        Assert.Null(await repo.ObtenerPorProductoAsync(producto.ProductoId, default));
    }

    [Fact]
    public async Task ExisteNumero_es_por_empresa_e_incluye_dados_de_baja()
    {
        var empresaA = Guid.NewGuid();
        var empresaB = Guid.NewGuid();
        await using var db = DbContextEnMemoria.Crear(new FakeTenant(empresaA));

        var producto = ProductoBuilder.Crear(empresaA);
        var registro = RegistroSanitarioBuilder.Crear(
            empresaA,
            producto.ProductoId,
            numeroRegistro: "RS-001",
            estado: EstadoVencimiento.Vigente);

        registro.DadoDeBaja = true;

        db.Productos.Add(producto);
        db.RegistrosSanitarios.Add(registro);
        await db.SaveChangesAsync(default);

        var repo = new RegistroSanitarioRepository(db);

        Assert.True(await repo.ExisteNumeroAsync(empresaA, "RS-001", null, default));
        Assert.False(await repo.ExisteNumeroAsync(empresaB, "RS-001", null, default));
    }

    [Fact]
    public async Task ExisteNumero_excluye_el_registro_que_se_esta_editando()
    {
        var empresa = Guid.NewGuid();
        await using var db = DbContextEnMemoria.Crear(new FakeTenant(empresa));

        var producto = ProductoBuilder.Crear(empresa);
        var registro = RegistroSanitarioBuilder.Crear(
            empresa,
            producto.ProductoId,
            numeroRegistro: "RS-001",
            estado: EstadoVencimiento.Vigente);

        db.Productos.Add(producto);
        db.RegistrosSanitarios.Add(registro);
        await db.SaveChangesAsync(default);

        var repo = new RegistroSanitarioRepository(db);

        Assert.False(await repo.ExisteNumeroAsync(
            empresa, "RS-001", registro.RegistroSanitarioId, default));
    }
}
