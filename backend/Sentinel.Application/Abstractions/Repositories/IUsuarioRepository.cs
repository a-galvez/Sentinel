
using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorCorreoAsync(string correoNormalizado, CancellationToken ct);
    Task<bool> ExistePorCorreoAsync(string correoNormalizado, CancellationToken ct);
    Task<Usuario?> ObtenerPorIdAsync(Guid usuarioId, CancellationToken ct);
    Task<Usuario?> ObtenerPorIdIgnorandoFiltrosAsync(Guid usuarioId, CancellationToken ct);

    Task<bool> ExisteActivoEnEmpresaAsync(Guid usuarioId, CancellationToken ct);

    void Agregar(Usuario usuario);

    Task<Dictionary<Guid, string>> NombresPorIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
}
