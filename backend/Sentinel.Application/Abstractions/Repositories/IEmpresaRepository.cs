using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions.Repositories;

public interface IEmpresaRepository
{
    Task<Empresa?> ObtenerAsync(Guid empresaId, CancellationToken ct);

    // Lo usará el registro (H1) para crear empresa + usuario en una sola transacción.
    void Agregar(Empresa empresa);
}
