using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common.Exceptions;

namespace Sentinel.Application.Features.LicenciasSanitarias.RegistrarEmision;

public sealed class RegistrarEmisionLicencia(
    IEstablecimientoRepository establecimientos,
    ILicenciaRepository licencias,
    IChecklistRepository checklists,
    IDocumentoRepository documentos,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    private const string MensajeNoEncontrado = "El establecimiento no existe o no tiene licencia sanitaria.";

    public async Task<LicenciaSanitariaDto> ManejarAsync(
        Guid establecimientoId, RegistrarEmisionLicenciaRequest request, CancellationToken ct)
    {
        // El filtro global ya excluye lo de otra empresa y lo dado de baja → 404.
        _ = await establecimientos.ObtenerAsync(establecimientoId, ct)
            ?? throw new NotFoundException(MensajeNoEncontrado);

        // Sin licencia, dada de baja o sin documento asignado (ninguna regla aplicó) → 404.
        var licencia = await licencias.ObtenerPorEstablecimientoParaModificarAsync(establecimientoId, ct);
        if (licencia?.TipoDocumentoId is not { } tipoDocumentoId)
            throw new NotFoundException(MensajeNoEncontrado);

        var tipoDocumento = await documentos.ObtenerTipoDocumentoAsync(tipoDocumentoId, ct)
            ?? throw new InvalidOperationException($"La licencia apunta a un tipo de documento inexistente ({tipoDocumentoId}).");

        // Aplica desde NoVerificado o EnTramite, y también sirve para corregir los datos.
        // El número de licencia no se valida como único (a diferencia del registro): no hay 409.
        var fechaEmision = request.FechaEmision!.Value;
        licencia.NumeroLicencia = request.Numero.Trim();
        licencia.FechaEmision = fechaEmision;
        licencia.FechaVencimiento = fechaEmision.AddMonths(tipoDocumento.VigenciaMeses);

        await unitOfWork.SaveChangesAsync(ct);

        // El checklist de un trámite previo se conserva (solo lectura): tieneChecklist sigue en true.
        var tieneChecklist = await checklists.ExisteParaLicenciaAsync(licencia.LicenciaSanitariaId, ct);

        return licencia.ToDto(tipoDocumento, tieneChecklist, clock.HoyHonduras);
    }
}