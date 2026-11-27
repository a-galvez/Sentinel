using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Sentinel.API.Controllers.Auth;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.Auth;
using Sentinel.Application.Features.Auth.Refresh;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Services;

namespace Sentinel.Tests.Auth;

public class RefreshControllerTests
{
    private static readonly DateTime Ahora = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Rotacion_revoca_el_anterior_y_entrega_uno_nuevo()
    {
        var ctx = new Contexto();

        // Login previo: emite un refresh token válido.
        var login = await ctx.TokenService.EmitirAsync(ctx.Usuario, default);

        var controller = CrearController(ctx);
        var result = await controller.Refresh(new(login.RefreshToken), default);
        var response = RespuestaOk<TokensResponse>(result);

        Assert.NotEqual(login.RefreshToken, response.RefreshToken);
        Assert.True(response.ExpiraEn > Ahora);

        // El token anterior queda revocado.
        var viejo = ctx.Tokens.Tokens
            .SingleOrDefault(t => t.TokenHash == ctx.TokenService.HashRefreshToken(login.RefreshToken));
        Assert.NotNull(viejo);
        Assert.NotNull(viejo!.RevocadoEn);

        // El nuevo queda vinculado al anterior.
        var nuevo = ctx.Tokens.Tokens
            .SingleOrDefault(t => t.TokenHash == ctx.TokenService.HashRefreshToken(response.RefreshToken));
        Assert.NotNull(nuevo);
        Assert.Equal(viejo!.RefreshTokenId, nuevo!.ReemplazadoPorId);
    }

    [Fact]
    public async Task Reutilizar_un_token_revocado_lanza_401()
    {
        var ctx = new Contexto();
        var login = await ctx.TokenService.EmitirAsync(ctx.Usuario, default);
        var controller = CrearController(ctx);

        // Primer uso: rota (revoca el token).
        _ = await controller.Refresh(new(login.RefreshToken), default);

        // Segundo uso del mismo token → 401 INVALID_REFRESH_TOKEN.
        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => controller.Refresh(new(login.RefreshToken), default));

        Assert.Equal("INVALID_REFRESH_TOKEN", ex.Code);
    }

    [Fact]
    public async Task Token_inexistente_lanza_401()
    {
        var ctx = new Contexto();
        var controller = CrearController(ctx);

        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => controller.Refresh(new("token-que-no-existe"), default));

        Assert.Equal("INVALID_REFRESH_TOKEN", ex.Code);
    }

    [Fact]
    public async Task Token_expirado_lanza_401()
    {
        var ctx = new Contexto();
        var login = await ctx.TokenService.EmitirAsync(ctx.Usuario, default);

        // Reloj futuro: ya pasó la expiración (7 días por defecto).
        var controller = CrearController(ctx, Ahora.AddDays(8));

        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => controller.Refresh(new(login.RefreshToken), default));

        Assert.Equal("INVALID_REFRESH_TOKEN", ex.Code);
    }

    private static RefreshController CrearController(Contexto ctx, DateTime? ahora = null)
        => new(
            ctx.Usuarios,
            ctx.Tokens,
            new FakeClock(ahora ?? Ahora),
            ctx.TokenService,
            new FakeUnitOfWork(),
            new RefreshValidator());

    private static T RespuestaOk<T>(ActionResult<T> result)
        => Assert.IsType<T>(Assert.IsType<OkObjectResult>(result.Result).Value);

    private sealed class Contexto
    {
        public FakeUsuarioRepository Usuarios { get; } = new();
        public FakeRefreshTokenRepository Tokens { get; } = new();
        public FakeClock Clock { get; } = new(Ahora);
        public JwtTokenService TokenService { get; }
        public Usuario Usuario { get; }

        public Contexto()
        {
            Usuario = new Usuario
            {
                UsuarioId = Guid.NewGuid(),
                EmpresaId = Guid.NewGuid(),
                Nombre = "Administrador",
                Correo = "admin@demo.test",
                PasswordHash = "hash",
                Activo = true
            };
            Usuarios.Usuarios.Add(Usuario);

            var cfg = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Secret"] = "clave-de-prueba-con-al-menos-32-caracteres!",
                    ["Jwt:Issuer"] = "sentinel-test",
                    ["Jwt:Audience"] = "sentinel-test"
                })
                .Build();

            TokenService = new JwtTokenService(Tokens, Clock, cfg);
        }
    }

    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        public List<Usuario> Usuarios { get; } = [];

        public Task<Usuario?> ObtenerPorCorreoAsync(string correoNormalizado, CancellationToken ct)
            => Task.FromResult(Usuarios.SingleOrDefault(u => u.Correo == correoNormalizado));

        public Task<bool> ExistePorCorreoAsync(string correoNormalizado, CancellationToken ct)
            => Task.FromResult(Usuarios.Any(u => u.Correo == correoNormalizado));

        public Task<Usuario?> ObtenerPorIdAsync(Guid usuarioId, CancellationToken ct)
            => Task.FromResult(Usuarios.SingleOrDefault(u => u.UsuarioId == usuarioId));

        public Task<Usuario?> ObtenerPorIdIgnorandoFiltrosAsync(Guid usuarioId, CancellationToken ct)
            => Task.FromResult(Usuarios.SingleOrDefault(u => u.UsuarioId == usuarioId));

        public Task<bool> ExisteActivoEnEmpresaAsync(Guid usuarioId, CancellationToken ct)
            => Task.FromResult(Usuarios.Any(u => u.UsuarioId == usuarioId && u.Activo));

        public void Agregar(Usuario usuario) => Usuarios.Add(usuario);

        public Task<Dictionary<Guid, string>> NombresPorIdsAsync(IEnumerable<Guid> ids, CancellationToken ct)
        {
            var distintos = ids.Distinct().ToList();
            return Task.FromResult(Usuarios
                .Where(u => distintos.Contains(u.UsuarioId))
                .ToDictionary(u => u.UsuarioId, u => u.Nombre));
        }
    }

    private sealed class FakeRefreshTokenRepository : IRefreshTokenRepository
    {
        public List<RefreshToken> Tokens { get; } = [];

        public Task<RefreshToken?> ObtenerPorHashAsync(string hash, CancellationToken ct)
            => Task.FromResult(Tokens.SingleOrDefault(t => t.TokenHash == hash));

        public void Agregar(RefreshToken token) => Tokens.Add(token);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken ct) => Task.FromResult(0);
    }

    private sealed class FakeClock(DateTime utcAhora) : IClock
    {
        public DateOnly HoyHonduras => DateOnly.FromDateTime(utcAhora.AddHours(-6));
        public DateTime UtcAhora => utcAhora;
    }
}
