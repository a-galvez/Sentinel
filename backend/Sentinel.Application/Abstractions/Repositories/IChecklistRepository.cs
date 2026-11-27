
using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions.Repositories;

public sealed record FilaConRequisito(ChecklistRequisito Fila, Requisito Requisito);

public interface IChecklistRepository
{
    Task<ChecklistRequisito?> ObtenerPorIdAsync(Guid checklistRequisitoId, CancellationToken ct);

    Task<List<FilaConRequisito>> FilasDeProductoAsync(Guid productoId, CancellationToken ct);
    Task<List<FilaConRequisito>> FilasDeLicenciaAsync(Guid licenciaId, CancellationToken ct);

    Task<bool> ExisteParaProductoAsync(Guid productoId, CancellationToken ct);
    Task<bool> ExisteParaLicenciaAsync(Guid licenciaId, CancellationToken ct);

    void Agregar(ChecklistRequisito fila);

    Task<HashSet<Guid>> LicenciaIdsConChecklistAsync(IReadOnlyList<Guid> licenciaIds, CancellationToken ct);
}
