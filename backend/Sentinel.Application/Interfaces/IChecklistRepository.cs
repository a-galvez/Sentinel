using Sentinel.Domain.Entities;

namespace Sentinel.Application.Interfaces;

// Solo la operación base. Las consultas por producto/licencia y las escrituras
// las agrega el encargado del checklist.
public interface IChecklistRepository
{
    Task<ChecklistRequisito?> ObtenerPorIdAsync(Guid checklistRequisitoId, CancellationToken ct);
}
