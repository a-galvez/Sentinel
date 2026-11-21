using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions;

namespace Sentinel.Application.Features.Requisitos;

// Lee los nombres de los usuarios involucrados (responsable / quién cumplió) en una consulta.
// Se usa en el checklist y en el PATCH para no repetir la misma consulta.
internal static class NombresUsuarios
{
    public static async Task<Dictionary<Guid, string>> CargarAsync(
        IAppDbContext db, IEnumerable<Guid?> ids, CancellationToken ct)
    {
        var distintos = ids.Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();
        if (distintos.Count == 0) return [];

        return await db.Usuarios
            .Where(u => distintos.Contains(u.UsuarioId))
            .ToDictionaryAsync(u => u.UsuarioId, u => u.Nombre, ct);
    }

    public static string? Buscar(IReadOnlyDictionary<Guid, string> nombres, Guid? id)
        => id.HasValue && nombres.TryGetValue(id.Value, out var n) ? n : null;
}
