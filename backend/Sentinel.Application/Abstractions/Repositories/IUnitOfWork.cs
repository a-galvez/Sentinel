
namespace Sentinel.Application.Abstractions.Repositories;

// Punto único para persistir cambios de un caso de uso.
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
