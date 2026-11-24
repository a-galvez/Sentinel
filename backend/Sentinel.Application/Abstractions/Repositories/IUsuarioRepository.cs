
using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorCorreoAsync(string correoNormalizado, CancellationToken ct);
    Task<Usuario?> ObtenerPorIdIgnorandoFiltrosAsync(Guid usuarioId, CancellationToken ct);

    Task<bool> ExisteActivoEnEmpresaAsync(Guid usuarioId, CancellationToken ct);

    Task<Dictionary<Guid, string>> NombresPorIdsAsync(IEnumerable<Guid> ids, CancellationToken ct);
}
