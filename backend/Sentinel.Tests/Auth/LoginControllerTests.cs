using Microsoft.AspNetCore.Mvc;
using Sentinel.API.Controllers.Auth;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.Auth;
using Sentinel.Application.Features.Auth.Login;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Services;

namespace Sentinel.Tests.Auth;

public class LoginControllerTests
{
    private static readonly Guid EmpresaId = Guid.NewGuid();

    [Fact]
    public async Task Correo_inexistente_lanza_INVALID_CREDENTIALS()
    {
        var usuarios = new FakeUsuarioRepository();
        var controller = CrearController(usuarios);
        usuarios.Usuarios.Add(Usuario("admin@demo.test", "clave-segura", activo: true));

        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => controller.Login(new("nadie@demo.test", "clave-segura"), default));

        Assert.Equal("INVALID_CREDENTIALS", ex.Code);
        Assert.Equal("Correo o contraseña incorrectos", ex.Message);
    }

    [Fact]
    public async Task Contrasena_erronea_lanza_INVALID_CREDENTIALS()
    {
        var usuarios = new FakeUsuarioRepository();
        var controller = CrearController(usuarios);
        usuarios.Usuarios.Add(Usuario("admin@demo.test", "clave-segura", activo: true));

        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => controller.Login(new("admin@demo.test", "clave-equivocada"), default));

        Assert.Equal("INVALID_CREDENTIALS", ex.Code);
        Assert.Equal("Correo o contraseña incorrectos", ex.Message);
    }

    [Fact]
    public async Task Usuario_inactivo_lanza_INVALID_CREDENTIALS()
    {
        var usuarios = new FakeUsuarioRepository();
        var controller = CrearController(usuarios);
        usuarios.Usuarios.Add(Usuario("admin@demo.test", "clave-segura", activo: false));

        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => controller.Login(new("admin@demo.test", "clave-segura"), default));

        Assert.Equal("INVALID_CREDENTIALS", ex.Code);
        Assert.Equal("Correo o contraseña incorrectos", ex.Message);
    }

    [Fact]
    public async Task Credenciales_correctas_devuelve_tokens_y_usuario_con_empresa()
    {
        var usuarios = new FakeUsuarioRepository();
        var uow = new FakeUnitOfWork();
        var controller = CrearController(usuarios, uow, new FakeTokenService("access-token", "refresh-token"));
        usuarios.Usuarios.Add(Usuario("admin@demo.test", "clave-segura", activo: true));

        var result = await controller.Login(new("admin@demo.test", "clave-segura"), default);
        var response = RespuestaOk<SesionResponse>(result);

        Assert.Equal("access-token", response.Tokens.AccessToken);
        Assert.Equal("refresh-token", response.Tokens.RefreshToken);
        Assert.True(response.Tokens.ExpiraEn > DateTime.UtcNow);
        Assert.Equal("admin@demo.test", response.Usuario.Correo);
        Assert.Equal(EmpresaId, response.Usuario.Empresa.EmpresaId);
        Assert.Equal("Farmacia Demo", response.Usuario.Empresa.Nombre);
        Assert.Equal(1, uow.Guardados);
    }

    // El correo se normaliza: el login funciona aunque el usuario escriba mayúsculas o espacios.
    [Fact]
    public async Task Login_normaliza_el_correo()
    {
        var usuarios = new FakeUsuarioRepository();
        var controller = CrearController(usuarios);
        usuarios.Usuarios.Add(Usuario("admin@demo.test", "clave-segura", activo: true));

        var result = await controller.Login(new("  ADMIN@DEMO.TEST  ", "clave-segura"), default);
        var response = RespuestaOk<SesionResponse>(result);

        Assert.Equal("admin@demo.test", response.Usuario.Correo);
    }

    private static LoginController CrearController(
        FakeUsuarioRepository usuarios,
        FakeUnitOfWork? uow = null,
        FakeTokenService? tokenService = null)
    {
        var empresas = new FakeEmpresaRepository();
        empresas.Empresas.Add(new Empresa
        {
            EmpresaId = EmpresaId,
            Nombre = "Farmacia Demo",
            FechaRegistro = DateTime.UtcNow
        });

        return new LoginController(
            usuarios,
            empresas,
            new BCryptPasswordHasher(),
            tokenService ?? new FakeTokenService("a", "r"),
            uow ?? new FakeUnitOfWork(),
            new LoginValidator());
    }

    private static T RespuestaOk<T>(ActionResult<T> result)
        => Assert.IsType<T>(Assert.IsType<OkObjectResult>(result.Result).Value);

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

    private sealed class FakeTokenService(string accessToken, string refreshToken) : ITokenService
    {
        public Task<AuthTokens> EmitirAsync(Usuario usuario, CancellationToken ct)
            => Task.FromResult(new AuthTokens(accessToken, refreshToken, DateTime.UtcNow.AddMinutes(15)));

        public Task<AuthTokens> RotarAsync(Usuario usuario, Guid reemplazaA, CancellationToken ct)
            => Task.FromResult(new AuthTokens(accessToken, refreshToken, DateTime.UtcNow.AddMinutes(15)));

        public string HashRefreshToken(string refreshToken) => refreshToken;
    }
}
