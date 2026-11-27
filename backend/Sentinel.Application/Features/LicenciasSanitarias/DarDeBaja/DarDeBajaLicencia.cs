using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common.Exceptions;

namespace Sentinel.Application.Features.LicenciasSanitarias.DarDeBaja;

public sealed class DarDeBajaLicencia(
    IEstablecimientoRepository establecimientos,
    ILicenciaRepository licencias,
    IUnitOfWork unitOfWork)
{
    private const string MensajeNoEncontrado = "El establecimiento no existe o no tiene licencia sanitaria.";

    public async Task ManejarAsync(Guid establecimientoId, CancellationToken ct)
    {
        // El filtro global ya excluye lo de otra empresa y lo dado de baja → 404.
        _ = await establecimientos.ObtenerAsync(establecimientoId, ct)
            ?? throw new NotFoundException(MensajeNoEncontrado);

        // Sin licencia, ya dada de baja o sin documento asignado (ninguna regla aplicó) → 404.
        var licencia = await licencias.ObtenerPorEstablecimientoParaModificarAsync(establecimientoId, ct);
        if (licencia?.TipoDocumentoId is null)
            throw new NotFoundException(MensajeNoEncontrado);

        // TODO(H11): el checklist de la licencia no se marca como dado de baja porque
        // ChecklistRequisito no implementa ISoftDelete. El endpoint de checklist (P5) debe
        // devolver 404 si la licencia/establecimiento están dados de baja, o agregar soft-delete.
        // Ver Docs/sentinel-p4-pendientes.md.

        // Solo la licencia: el establecimiento no se da de baja.
        licencia.DadoDeBaja = true;

        await unitOfWork.SaveChangesAsync(ct);
    }
}