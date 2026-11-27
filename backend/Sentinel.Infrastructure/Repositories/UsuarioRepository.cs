
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Infrastructure.Repositories;

public sealed class UsuarioRepository(AppDbContext db) : IUsuarioRepository
{
    public Task<Usuario?> ObtenerPorCorreoAsync(string correoNormalizado, CancellationToken ct)
        => db.Usuarios
            .IgnoreQueryFilters()   // el login es público: no hay sesión, el filtro ocultaría
            .SingleOrDefaultAsync(u => u.Correo == correoNormalizado, ct);

    public Task<bool> ExistePorCorreoAsync(string correoNormalizado, CancellationToken ct)
        => db.Usuarios
            .IgnoreQueryFilters()   // el registro es público: la unicidad es global
            .AnyAsync(u => u.Correo == correoNormalizado, ct);

    public Task<Usuario?> ObtenerPorIdAsync(Guid usuarioId, CancellationToken ct)
        => db.Usuarios.SingleOrDefaultAsync(u => u.UsuarioId == usuarioId, ct);

    public Task<Usuario?> ObtenerPorIdIgnorandoFiltrosAsync(Guid usuarioId, CancellationToken ct)
        => db.Usuarios
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(u => u.UsuarioId == usuarioId, ct);

    public Task<bool> ExisteActivoEnEmpresaAsync(Guid usuarioId, CancellationToken ct)
        => db.Usuarios.AnyAsync(u => u.UsuarioId == usuarioId && u.Activo, ct);

    public void Agregar(Usuario usuario) => db.Usuarios.Add(usuario);

    public async Task<Dictionary<Guid, string>> NombresPorIdsAsync(IEnumerable<Guid> ids, CancellationToken ct)
    {
        var distintos = ids.Distinct().ToList();
        if (distintos.Count == 0) return [];

        return await db.Usuarios
            .Where(u => distintos.Contains(u.UsuarioId))
            .ToDictionaryAsync(u => u.UsuarioId, u => u.Nombre, ct);
    }
}
