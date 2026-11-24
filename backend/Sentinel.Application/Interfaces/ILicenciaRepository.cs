using Sentinel.Domain.Entities;

namespace Sentinel.Application.Interfaces;

// Solo la operación base. Las consultas por empresa/tipo y vigencia
// las agrega el encargado de licencias.
public interface ILicenciaRepository
{
    Task<LicenciaSanitaria?> ObtenerAsync(Guid licenciaId, CancellationToken ct);
}
