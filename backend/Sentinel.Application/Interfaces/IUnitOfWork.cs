
namespace Sentinel.Application.Abstractions.Repositories;

// Punto único para persistir cambios de un caso de uso. Sustituye a IAppDbContext.SaveChangesAsync.
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
