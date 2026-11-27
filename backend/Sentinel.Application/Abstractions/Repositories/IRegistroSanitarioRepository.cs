using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions.Repositories;

public interface IRegistroSanitarioRepository
{
    Task<RegistroSanitario?> ObtenerAsync(Guid registroSanitarioId, CancellationToken ct);

    Task<RegistroSanitario?> ObtenerPorProductoAsync(Guid productoId, CancellationToken ct);

    Task<List<RegistroSanitario>> ObtenerPorProductosAsync(
        IReadOnlyCollection<Guid> productoIds,
        CancellationToken ct);

    Task<bool> ExisteNumeroAsync(
        Guid empresaId,
        string numero,
        Guid? excluirRegistroSanitarioId,
        CancellationToken ct);

    void Agregar(RegistroSanitario registro);
}
