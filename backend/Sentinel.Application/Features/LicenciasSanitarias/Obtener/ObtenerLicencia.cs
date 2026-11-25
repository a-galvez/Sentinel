using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common.Exceptions;

namespace Sentinel.Application.Features.LicenciasSanitarias.Obtener;

public sealed class ObtenerLicencia(
    IEstablecimientoRepository establecimientos,
    ILicenciaRepository licencias,
    IChecklistRepository checklists,
    IDocumentoRepository documentos,
    IClock clock)
{
    private const string MensajeNoEncontrado = "El establecimiento no existe o no tiene licencia sanitaria.";

    public async Task<LicenciaSanitariaDto> ManejarAsync(Guid establecimientoId, CancellationToken ct)
    {
        // El filtro global ya excluye lo de otra empresa y lo dado de baja → 404.
        _ = await establecimientos.ObtenerAsync(establecimientoId, ct)
            ?? throw new NotFoundException(MensajeNoEncontrado);

        // Sin licencia, dada de baja o sin documento asignado (ninguna regla aplicó) → 404.
        var licencia = await licencias.ObtenerPorEstablecimientoAsync(establecimientoId, ct);
        if (licencia?.TipoDocumentoId is not { } tipoDocumentoId)
            throw new NotFoundException(MensajeNoEncontrado);

        var tipoDocumento = await documentos.ObtenerTipoDocumentoAsync(tipoDocumentoId, ct)
            ?? throw new InvalidOperationException($"La licencia apunta a un tipo de documento inexistente ({tipoDocumentoId}).");

        var tieneChecklist = await checklists.ExisteParaLicenciaAsync(licencia.LicenciaSanitariaId, ct);

        return licencia.ToDto(tipoDocumento, tieneChecklist, clock.HoyHonduras);
    }
}