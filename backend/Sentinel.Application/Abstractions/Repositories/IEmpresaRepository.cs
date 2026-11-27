using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions.Repositories;

public interface IEmpresaRepository
{
    Task<Empresa?> ObtenerAsync(Guid empresaId, CancellationToken ct);
}
