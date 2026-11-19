using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.Auth.Refresh;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Persistence;
using Sentinel.Infrastructure.Services;

namespace Sentinel.Tests.Auth;

public class RefreshHandlerTests
{
    private static readonly DateTime Ahora = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Rotacion_revoca_el_anterior_y_entrega_uno_nuevo()
    {
        CrearContexto(out var db, out var tokenService, out var usuario);

        // Login previo: emite un refresh token válido.
        var login = await tokenService.EmitirAsync(usuario, default);

        var handler = new RefreshHandler(db, new FakeClock(Ahora), tokenService);
        var response = await handler.ManejarAsync(new(login.RefreshToken), default);

        Assert.Equal("Bearer", response.TokenType);
        Assert.NotEqual(login.RefreshToken, response.RefreshToken);
        Assert.Equal(usuario.UsuarioId, response.User.Id);

        // El token anterior queda revocado.
        var viejo = await db.RefreshTokens
            .SingleOrDefaultAsync(t => t.TokenHash == tokenService.HashRefreshToken(login.RefreshToken));
        Assert.NotNull(viejo);
        Assert.NotNull(viejo!.RevocadoEn);

        // El nuevo queda vinculado al anterior.
        var nuevo = await db.RefreshTokens
            .SingleOrDefaultAsync(t => t.TokenHash == tokenService.HashRefreshToken(response.RefreshToken));
        Assert.NotNull(nuevo);
        Assert.Equal(viejo!.RefreshTokenId, nuevo!.ReemplazadoPorId);
    }

    [Fact]
    public async Task Reutilizar_un_token_revocado_lanza_401()
    {
        CrearContexto(out var db, out var tokenService, out var usuario);

        var login = await tokenService.EmitirAsync(usuario, default);
        var handler = new RefreshHandler(db, new FakeClock(Ahora), tokenService);

        // Primer uso: rota (revoca el token).
        _ = await handler.ManejarAsync(new(login.RefreshToken), default);

        // Segundo uso del mismo token → 401 INVALID_REFRESH_TOKEN.
        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => handler.ManejarAsync(new(login.RefreshToken), default));

        Assert.Equal("INVALID_REFRESH_TOKEN", ex.Code);
    }

    [Fact]
    public async Task Token_inexistente_lanza_401()
    {
        CrearContexto(out var db, out var tokenService, out _);
        var handler = new RefreshHandler(db, new FakeClock(Ahora), tokenService);

        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => handler.ManejarAsync(new("token-que-no-existe"), default));

        Assert.Equal("INVALID_REFRESH_TOKEN", ex.Code);
    }

    [Fact]
    public async Task Token_expirado_lanza_401()
    {
        CrearContexto(out var db, out var tokenService, out var usuario);

        var login = await tokenService.EmitirAsync(usuario, default);

        // Reloj futuro: ya pasó la expiración (7 días por defecto).
        var futuro = new FakeClock(Ahora.AddDays(8));
        var handler = new RefreshHandler(db, futuro, tokenService);

        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => handler.ManejarAsync(new(login.RefreshToken), default));

        Assert.Equal("INVALID_REFRESH_TOKEN", ex.Code);
    }

    private static void CrearContexto(
        out AppDbContext db,
        out JwtTokenService tokenService,
        out Usuario usuario)
    {
        db = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"sentinel-refresh-{Guid.NewGuid()}")
                .Options,
            new FakeTenantSinSesion());

        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "clave-de-prueba-con-al-menos-32-caracteres!",
                ["Jwt:Issuer"] = "sentinel-test",
                ["Jwt:Audience"] = "sentinel-test"
            })
            .Build();

        tokenService = new JwtTokenService(db, new FakeClock(Ahora), cfg);

        usuario = new Usuario
        {
            UsuarioId = Guid.NewGuid(),
            EmpresaId = Guid.NewGuid(),
            Nombre = "Administrador",
            Correo = "admin@demo.test",
            PasswordHash = "hash",
            Activo = true
        };
        db.Usuarios.Add(usuario);
        db.SaveChanges();
    }

    private sealed class FakeClock(DateTime utcAhora) : IClock
    {
        public DateOnly HoyHonduras => DateOnly.FromDateTime(utcAhora.AddHours(-6));
        public DateTime UtcAhora => utcAhora;
    }

    private sealed class FakeTenantSinSesion : ICurrentTenant
    {
        public bool TieneSesion => false;
        public Guid EmpresaId => Guid.Empty;
        public Guid UsuarioId => Guid.Empty;
    }
}
