using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Sentinel.API.Controllers.Auth;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.Auth;
using Sentinel.Application.Features.Auth.Registro;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Services;

namespace Sentinel.Tests.Auth;

public class RegistroControllerTests
{
    private static readonly DateTime Ahora = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Registro_crea_empresa_usuario_y_sesion_en_una_sola_transaccion()
    {
        var usuarios = new FakeUsuarioRepository();
        var empresas = new FakeEmpresaRepository();
        var uow = new FakeUnitOfWork();
        var controller = CrearController(usuarios, empresas, uow);

        var result = await controller.Registro(
            new("Farmacia Demo", "  María López  ", "  MARIA@DEMO.TEST  ", "clave-segura"),
            default);

        var response = RespuestaCreada(result);

        Assert.Equal("access-token", response.Tokens.AccessToken);
        Assert.Equal("refresh-token", response.Tokens.RefreshToken);
        Assert.Equal("María López", response.Usuario.Nombre);
        Assert.Equal("maria@demo.test", response.Usuario.Correo);
        Assert.Equal("Farmacia Demo", response.Usuario.Empresa.Nombre);

        var empresa = Assert.Single(empresas.Empresas);
        var usuario = Assert.Single(usuarios.Usuarios);

        Assert.Equal(empresa.EmpresaId, usuario.EmpresaId);
        Assert.Equal(Ahora, empresa.FechaRegistro);
        Assert.True(usuario.Activo);
        Assert.True(BCrypt.Net.BCrypt.Verify("clave-segura", usuario.PasswordHash));
        Assert.Equal(1, uow.Guardados);
    }

    [Fact]
    public async Task Correo_duplicado_lanza_conflicto_con_error_en_el_campo()
    {
        var usuarios = new FakeUsuarioRepository();
        usuarios.Usuarios.Add(new Usuario
        {
            UsuarioId = Guid.NewGuid(),
            EmpresaId = Guid.NewGuid(),
            Nombre = "Existente",
            Correo = "maria@demo.test",
            PasswordHash = "hash",
            Activo = true
        });

        var empresas = new FakeEmpresaRepository();
        var uow = new FakeUnitOfWork();
        var controller = CrearController(usuarios, empresas, uow);

        var ex = await Assert.ThrowsAsync<ConflictException>(
            () => controller.Registro(
                new("Farmacia Demo", "María López", "MARIA@DEMO.TEST", "clave-segura"),
                default));

        Assert.Equal("CORREO_YA_REGISTRADO", ex.Code);
        Assert.NotNull(ex.Errors);
        Assert.Equal("El correo ya está registrado.", ex.Errors!["correo"][0]);

        Assert.Empty(empresas.Empresas);
        Assert.Single(usuarios.Usuarios);
        Assert.Equal(0, uow.Guardados);
    }

    [Fact]
    public async Task Password_corta_lanza_validation_exception()
    {
        var controller = CrearController(
            new FakeUsuarioRepository(),
            new FakeEmpresaRepository(),
            new FakeUnitOfWork());

        var ex = await Assert.ThrowsAsync<ValidationException>(
            () => controller.Registro(
                new("Farmacia Demo", "María López", "maria@demo.test", "1234567"),
                default));

        Assert.Contains(ex.Errors, e => e.PropertyName == "Password");
    }

    private static RegistroController CrearController(
        FakeUsuarioRepository usuarios,
        FakeEmpresaRepository empresas,
        FakeUnitOfWork uow)
        => new(
            usuarios,
            empresas,
            new BCryptPasswordHasher(),
            new FakeTokenService(),
            uow,
            new FakeClock(Ahora),
            new RegistroValidator());

    private static SesionResponse RespuestaCreada(ActionResult<SesionResponse> result)
    {
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(201, objectResult.StatusCode);
        return Assert.IsType<SesionResponse>(objectResult.Value);
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

        public Task<Dictionary<Guid, string>> NombresPorIdsAsync(IEnumerable<Guid> ids, CancellationToken ct)
        {
            var distintos = ids.Distinct().ToList();
            return Task.FromResult(Usuarios
                .Where(u => distintos.Contains(u.UsuarioId))
                .ToDictionary(u => u.UsuarioId, u => u.Nombre));
        }

        public void Agregar(Usuario usuario) => Usuarios.Add(usuario);
    }

    private sealed class FakeEmpresaRepository : IEmpresaRepository
    {
        public List<Empresa> Empresas { get; } = [];

        public Task<Empresa?> ObtenerAsync(Guid empresaId, CancellationToken ct)
            => Task.FromResult(Empresas.SingleOrDefault(e => e.EmpresaId == empresaId));

        public void Agregar(Empresa empresa) => Empresas.Add(empresa);
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

    private sealed class FakeTokenService : ITokenService
    {
        public Task<AuthTokens> EmitirAsync(Usuario usuario, CancellationToken ct)
            => Task.FromResult(new AuthTokens("access-token", "refresh-token", Ahora.AddMinutes(15)));

        public Task<AuthTokens> RotarAsync(Usuario usuario, Guid reemplazaA, CancellationToken ct)
            => Task.FromResult(new AuthTokens("access-token", "refresh-token", Ahora.AddMinutes(15)));

        public string HashRefreshToken(string refreshToken) => refreshToken;
    }

    private sealed class FakeClock(DateTime ahora) : IClock
    {
        public DateOnly HoyHonduras => DateOnly.FromDateTime(ahora.AddHours(-6));
        public DateTime UtcAhora => ahora;
    }
}
