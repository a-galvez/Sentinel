using Sentinel.Domain.Entities;

namespace Sentinel.Application.Abstractions;

public interface IMotorRequisitos
{
    Task AsignarDocumentoAlProductoAsync(Producto producto, CancellationToken ct);
    Task AsignarDocumentoALicenciaAsync(LicenciaSanitaria licenciaSanitaria, CancellationToken ct);

    Task CrearChecklistDeProductoAsync(Producto producto, CancellationToken ct);
    Task CrearChecklistDeLicenciaAsync(LicenciaSanitaria licenciaSanitaria, CancellationToken ct);
}
