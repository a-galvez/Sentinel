
using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions.Repositories;

public interface ILicenciaRepository
{
    Task<LicenciaSanitaria?> ObtenerAsync(Guid licenciaId, CancellationToken ct);

    Task<int?> TipoEstablecimientoDeLicenciaAsync(Guid licenciaId, CancellationToken ct);

    Task<List<int>> TiposVigentesEnEmpresaAsync(Guid empresaId, IReadOnlyList<int> tiposDocumento, CancellationToken ct);
}
