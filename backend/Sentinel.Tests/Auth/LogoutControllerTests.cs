using Microsoft.AspNetCore.Mvc;
using Sentinel.API.Controllers.Auth;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Features.Auth.Refresh;
using Sentinel.Domain.Entities;

namespace Sentinel.Tests.Auth;

public class LogoutControllerTests
{
    private static readonly DateTime Ahora = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Revoca_un_token_valido_y_responde_204()
    {
        var tokens = new FakeRefreshTokenRepository();
        var token = Token("token-1");
        tokens.Tokens.Add(token);
        var uow = new FakeUnitOfWork();
        var controller = CrearController(tokens, uow);

        var result = await controller.Logout(new("token-1"), default);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(Ahora, token.RevocadoEn);
        Assert.Equal(1, uow.Guardados);
    }

    [Fact]
    public async Task Es_idempotente_si_el_token_ya_estaba_revocado()
    {
        var tokens = new FakeRefreshTokenRepository();
        var token = Token("token-1");
        token.RevocadoEn = Ahora.AddDays(-1);
        tokens.Tokens.Add(token);
        var uow = new FakeUnitOfWork();
        var controller = CrearController(tokens, uow);

        var result = await controller.Logout(new("token-1"), default);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(Ahora.AddDays(-1), token.RevocadoEn);
        Assert.Equal(0, uow.Guardados);
    }

    [Fact]
    public async Task Token_inexistente_responde_204()
    {
        var tokens = new FakeRefreshTokenRepository();
        var uow = new FakeUnitOfWork();
        var controller = CrearController(tokens, uow);

        var result = await controller.Logout(new("no-existe"), default);

        Assert.IsType<NoContentResult>(result);
        Assert.Equal(0, uow.Guardados);
    }

    private static LogoutController CrearController(FakeRefreshTokenRepository tokens, FakeUnitOfWork uow)
        => new(tokens, new FakeTokenService(), new FakeClock(Ahora), uow, new RefreshValidator());

    private static RefreshToken Token(string hash) => new()
    {
        RefreshTokenId = Guid.NewGuid(),
        UsuarioId = Guid.NewGuid(),
        TokenHash = hash,
        Expira = Ahora.AddDays(7)
    };

    private sealed class FakeTokenService : ITokenService
    {
        public Task<AuthTokens> EmitirAsync(Usuario usuario, CancellationToken ct)
            => throw new NotSupportedException();

        public Task<AuthTokens> RotarAsync(Usuario usuario, Guid reemplazaA, CancellationToken ct)
            => throw new NotSupportedException();

        public string HashRefreshToken(string refreshToken) => refreshToken;
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
        public int Guardados { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken ct)
        {
            Guardados++;
            return Task.FromResult(0);
        }
    }

    private sealed class FakeClock(DateTime ahora) : IClock
    {
        public DateOnly HoyHonduras => DateOnly.FromDateTime(ahora.AddHours(-6));
        public DateTime UtcAhora => ahora;
    }
}
