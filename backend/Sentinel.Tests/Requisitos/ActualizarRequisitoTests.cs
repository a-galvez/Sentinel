using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.Requisitos;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Persistence;
using Sentinel.Infrastructure.Seeding;
using Sentinel.Tests.Builders;

namespace Sentinel.Tests.Requisitos;

// H11 BE-8: PATCH /requisitos/{id}.
public class ActualizarRequisitoTests
{
    private static readonly DateOnly Hoy = new(2026, 11, 18);
    private static readonly DateTime Ahora = new(2026, 11, 18, 15, 30, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Cumplir_registra_usuario_y_hora()
    {
        var empresa = Guid.NewGuid();
        var usuario = Guid.NewGuid();
        await using var db = await PrepararAsync(empresa, usuario);
        var fila = db.ChecklistRequisitos.First();

        var handler = new ActualizarRequisitoHandler(db, new FakeTenant(empresa, usuario), new RelojFijo());

        var dto = await handler.ManejarAsync(
            fila.ChecklistRequisitoId, new RequisitoUpdateRequest(EstadoRequisito.Cumplido, null, null), default);

        Assert.Equal(EstadoRequisito.Cumplido, dto.Estado);
        Assert.Equal(usuario, dto.CumplidoPorId);
        Assert.Equal(Ahora, dto.CumplidoEn);
    }

    [Fact]
    public async Task Volver_a_pendiente_borra_quien_y_cuando()
    {
        var empresa = Guid.NewGuid();
        var usuario = Guid.NewGuid();
        await using var db = await PrepararAsync(empresa, usuario);
        var fila = db.ChecklistRequisitos.First();
        var handler = new ActualizarRequisitoHandler(db, new FakeTenant(empresa, usuario), new RelojFijo());

        await handler.ManejarAsync(fila.ChecklistRequisitoId,
            new RequisitoUpdateRequest(EstadoRequisito.Cumplido, null, null), default);
        var dto = await handler.ManejarAsync(fila.ChecklistRequisitoId,
            new RequisitoUpdateRequest(EstadoRequisito.Pendiente, null, null), default);

        Assert.Equal(EstadoRequisito.Pendiente, dto.Estado);
        Assert.Null(dto.CumplidoPorId);
        Assert.Null(dto.CumplidoEn);
    }

    [Fact]
    public async Task Asignar_responsable_y_fecha_limite()
    {
        var empresa = Guid.NewGuid();
        var usuario = Guid.NewGuid();
        await using var db = await PrepararAsync(empresa, usuario);
        var fila = db.ChecklistRequisitos.First();
        var handler = new ActualizarRequisitoHandler(db, new FakeTenant(empresa, usuario), new RelojFijo());

        var limite = new DateOnly(2026, 12, 15);
        var dto = await handler.ManejarAsync(fila.ChecklistRequisitoId,
            new RequisitoUpdateRequest(null, usuario, limite), default);

        Assert.Equal(usuario, dto.ResponsableId);
        Assert.Equal(limite, dto.FechaLimite);
    }

    [Fact]
    public async Task Responsable_de_otra_empresa_o_inexistente_da_400()
    {
        var empresa = Guid.NewGuid();
        var usuario = Guid.NewGuid();
        await using var db = await PrepararAsync(empresa, usuario);
        var fila = db.ChecklistRequisitos.First();
        var handler = new ActualizarRequisitoHandler(db, new FakeTenant(empresa, usuario), new RelojFijo());

        await Assert.ThrowsAsync<BadRequestException>(() => handler.ManejarAsync(
            fila.ChecklistRequisitoId, new RequisitoUpdateRequest(null, Guid.NewGuid(), null), default));
    }

    [Fact]
    public async Task Requisito_inexistente_da_404()
    {
        var empresa = Guid.NewGuid();
        var usuario = Guid.NewGuid();
        await using var db = await PrepararAsync(empresa, usuario);
        var handler = new ActualizarRequisitoHandler(db, new FakeTenant(empresa, usuario), new RelojFijo());

        await Assert.ThrowsAsync<NotFoundException>(() => handler.ManejarAsync(
            Guid.NewGuid(), new RequisitoUpdateRequest(EstadoRequisito.Cumplido, null, null), default));
    }

    private static async Task<AppDbContext> PrepararAsync(Guid empresa, Guid usuarioId)
    {
        var db = DbContextEnMemoria.Crear(new FakeTenant(empresa, usuarioId));
        await new CatalogoSeeder(db).SembrarAsync(default);

        var usuario = UsuarioBuilder.Crear(empresa);
        usuario.UsuarioId = usuarioId;
        db.Usuarios.Add(usuario);

        var producto = ProductoBuilder.Crear(empresa, categoriaProductoId: CatalogoIds.CategoriasProducto.Medicamento);
        db.Productos.Add(producto);
        await new Infrastructure.Services.MotorRequisitos(db).AplicarAProductoAsync(producto, default);
        await db.SaveChangesAsync(default);

        return db;
    }

    private sealed class RelojFijo : IClock
    {
        public DateOnly HoyHonduras => Hoy;
        public DateTime UtcAhora => Ahora;
    }
}
