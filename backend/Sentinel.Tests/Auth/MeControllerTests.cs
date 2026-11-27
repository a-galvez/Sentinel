using Microsoft.AspNetCore.Mvc;
using Sentinel.API.Controllers.Auth;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.Auth;
using Sentinel.Domain.Entities;

namespace Sentinel.Tests.Auth;

public class MeControllerTests
{
    private static readonly Guid EmpresaId = Guid.NewGuid();
    private static readonly Guid UsuarioId = Guid.NewGuid();

    [Fact]
    public async Task Devuelve_usuario_y_empresa_de_la_sesion()
    {
        var usuarios = new FakeUsuarioRepository();
        usuarios.Usuarios.Add(Usuario(activo: true));

        var empresas = new FakeEmpresaRepository();
        empresas.Empresas.Add(new Empresa
        {
            EmpresaId = EmpresaId,
            Nombre = "Farmacia Demo",
            FechaRegistro = DateTime.UtcNow
        });

        var controller = new MeController(new FakeTenant(), usuarios, empresas);

        var result = await controller.Me(default);
        var response = RespuestaOk<UsuarioActual>(result);

        Assert.Equal(UsuarioId, response.UsuarioId);
        Assert.Equal("María López", response.Nombre);
        Assert.Equal("maria@demo.test", response.Correo);
        Assert.Equal(EmpresaId, response.Empresa.EmpresaId);
        Assert.Equal("Farmacia Demo", response.Empresa.Nombre);
    }

    [Fact]
    public async Task Usuario_inexistente_lanza_401()
    {
        var controller = new MeController(new FakeTenant(), new FakeUsuarioRepository(), new FakeEmpresaRepository());

        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => controller.Me(default));

        Assert.Equal("UNAUTHORIZED", ex.Code);
    }

    [Fact]
    public async Task Usuario_inactivo_lanza_401()
    {
        var usuarios = new FakeUsuarioRepository();
        usuarios.Usuarios.Add(Usuario(activo: false));

        var controller = new MeController(new FakeTenant(), usuarios, new FakeEmpresaRepository());

        var ex = await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => controller.Me(default));

        Assert.Equal("UNAUTHORIZED", ex.Code);
    }

    [Fact]
    public async Task Empresa_inexistente_lanza_404()
    {
        var usuarios = new FakeUsuarioRepository();
        usuarios.Usuarios.Add(Usuario(activo: true));

        var controller = new MeController(new FakeTenant(), usuarios, new FakeEmpresaRepository());

        await Assert.ThrowsAsync<NotFoundException>(() => controller.Me(default));
    }

    private static T RespuestaOk<T>(ActionResult<T> result)
        => Assert.IsType<T>(Assert.IsType<OkObjectResult>(result.Result).Value);

    private static Usuario Usuario(bool activo) => new()
    {
        UsuarioId = UsuarioId,
        EmpresaId = EmpresaId,
        Nombre = "María López",
        Correo = "maria@demo.test",
        PasswordHash = "hash",
        Activo = activo
    };

    private sealed class FakeTenant : ICurrentTenant
    {
        public bool TieneSesion => true;
        public Guid EmpresaId => MeControllerTests.EmpresaId;
        public Guid UsuarioId => MeControllerTests.UsuarioId;
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

    private sealed class FakeEmpresaRepository : IEmpresaRepository
    {
        public List<Empresa> Empresas { get; } = [];

        public Task<Empresa?> ObtenerAsync(Guid empresaId, CancellationToken ct)
            => Task.FromResult(Empresas.SingleOrDefault(e => e.EmpresaId == empresaId));

        public void Agregar(Empresa empresa) => Empresas.Add(empresa);
    }
}
