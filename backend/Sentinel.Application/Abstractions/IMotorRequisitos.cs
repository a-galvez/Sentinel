using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions;

public interface IMotorRequisitos
{
    // No llaman SaveChanges: dejan el TipoDocumento y el checklist en el DbContext.
    Task AplicarAProductoAsync(Producto producto, CancellationToken ct);
    Task AplicarALicenciaAsync(LicenciaSanitaria licencia, CancellationToken ct);
}
