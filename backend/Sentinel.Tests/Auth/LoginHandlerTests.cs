using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.Auth.Login;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Persistence;
using Sentinel.Infrastructure.Services;

namespace Sentinel.Tests.Auth;

public class LoginHandlerTests
{
    private static readonly Guid EmpresaId = Guid.NewGuid();

    [Fact]
    public async Task Correo_inexistente_lanza_INVALID_CREDENTIALS()
    {
        var handler = CrearHandler(out var db);
        db.Usuarios.Add(Usuario("admin@demo.test", "clave-segura", activo: true));
        await db.SaveChangesAsync(default);

        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => handler.ManejarAsync(new("nadie@demo.test", "clave-segura"), default));

        Assert.Equal("INVALID_CREDENTIALS", ex.Code);
        Assert.Equal("Correo o contraseña incorrectos", ex.Message);
    }

    [Fact]
    public async Task Contrasena_erronea_lanza_INVALID_CREDENTIALS()
    {
        var handler = CrearHandler(out var db);
        db.Usuarios.Add(Usuario("admin@demo.test", "clave-segura", activo: true));
        await db.SaveChangesAsync(default);

        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => handler.ManejarAsync(new("admin@demo.test", "clave-equivocada"), default));

        Assert.Equal("INVALID_CREDENTIALS", ex.Code);
        Assert.Equal("Correo o contraseña incorrectos", ex.Message);
    }

    [Fact]
    public async Task Usuario_inactivo_lanza_INVALID_CREDENTIALS()
    {
        var handler = CrearHandler(out var db);
        db.Usuarios.Add(Usuario("admin@demo.test", "clave-segura", activo: false));
        await db.SaveChangesAsync(default);

        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => handler.ManejarAsync(new("admin@demo.test", "clave-segura"), default));

        Assert.Equal("INVALID_CREDENTIALS", ex.Code);
        Assert.Equal("Correo o contraseña incorrectos", ex.Message);
    }

    [Fact]
    public async Task Credenciales_correctas_devuelve_tokens()
    {
        var handler = CrearHandler(out var db, tokenService: new FakeTokenService("access-token", "refresh-token"));
        db.Usuarios.Add(Usuario("admin@demo.test", "clave-segura", activo: true));
        await db.SaveChangesAsync(default);

        var response = await handler.ManejarAsync(new("admin@demo.test", "clave-segura"), default);

        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal("access-token", response.AccessToken);
        Assert.Equal("refresh-token", response.RefreshToken);
        Assert.Equal("admin@demo.test", response.User.Correo);
        Assert.True(response.User.Activo);
    }

    // Login público (sin sesión): comprueba que no lo oculta el filtro global por empresa.
    [Fact]
    public async Task Login_funciona_sin_sesion_de_empresa()
    {
        var db = DbContextEnMemoria();
        db.Usuarios.Add(Usuario("admin@demo.test", "clave-segura", activo: true));
        await db.SaveChangesAsync(default);

        var handler = new LoginHandler(db, new BCryptPasswordHasher(), new FakeTokenService("access-token", "refresh-token"));

        var response = await handler.ManejarAsync(new("admin@demo.test", "clave-segura"), default);

        Assert.NotNull(response);
    }

    private static LoginHandler CrearHandler(
        out AppDbContext db,
        FakeTokenService? tokenService = null)
    {
        db = DbContextEnMemoria();
        return new LoginHandler(db, new BCryptPasswordHasher(), tokenService ?? new FakeTokenService("a", "r"));
    }

    private static AppDbContext DbContextEnMemoria()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"sentinel-auth-{Guid.NewGuid()}")
            .Options;
        return new AppDbContext(options, new FakeTenantSinSesion());
    }

    private static Usuario Usuario(string correo, string contrasena, bool activo)
        => new()
        {
            UsuarioId = Guid.NewGuid(),
            EmpresaId = EmpresaId,
            Nombre = "Administrador",
            Correo = correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(contrasena),
            Activo = activo
        };

    private sealed class FakeTokenService(string accessToken, string refreshToken) : ITokenService
    {
        public Task<AuthTokens> EmitirAsync(Usuario usuario, CancellationToken ct)
            => Task.FromResult(new AuthTokens(accessToken, refreshToken));

        public Task<AuthTokens> RotarAsync(Usuario usuario, Guid reemplazaA, CancellationToken ct)
            => Task.FromResult(new AuthTokens(accessToken, refreshToken));

        public string HashRefreshToken(string refreshToken) => refreshToken;
    }

    private sealed class FakeTenantSinSesion : ICurrentTenant
    {
        public bool TieneSesion => false;
        public Guid EmpresaId => Guid.Empty;
        public Guid UsuarioId => Guid.Empty;
    }
}
