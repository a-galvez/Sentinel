using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Domain.Calculos;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Features.LicenciasSanitarias.Verificar;

public sealed class VerificarLicencia(
    IEstablecimientoRepository establecimientos,
    ILicenciaRepository licencias,
    IChecklistRepository checklists,
    IDocumentoRepository documentos,
    IMotorRequisitos motor,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    private const string MensajeNoEncontrado = "El establecimiento no existe o no tiene licencia sanitaria.";

    public async Task<LicenciaSanitariaDto> ManejarAsync(
        Guid establecimientoId, VerificarLicenciaRequest request, CancellationToken ct)
    {
        // El filtro global excluye otra empresa y lo dado de baja → 404.
        _ = await establecimientos.ObtenerAsync(establecimientoId, ct)
            ?? throw new NotFoundException(MensajeNoEncontrado);

        var licencia = await licencias.ObtenerPorEstablecimientoParaModificarAsync(establecimientoId, ct);
        if (licencia?.TipoDocumentoId is not { } tipoDocumentoId)
            throw new NotFoundException(MensajeNoEncontrado);

        if (licencia.Estado != EstadoVencimiento.NoVerificado)                 // ← estado
            throw new ConflictException("La licencia ya fue verificada.");

        var tipoDocumento = await documentos.ObtenerTipoDocumentoAsync(tipoDocumentoId, ct)
            ?? throw new InvalidOperationException($"La licencia apunta a un tipo de documento inexistente ({tipoDocumentoId}).");

        var hoy = clock.HoyHonduras;

        if (request.TieneLicencia == true)
        {
            var fechaEmision = request.FechaEmision!.Value;
            var fechaVencimiento = fechaEmision.AddMonths(tipoDocumento.VigenciaMeses);

            licencia.NumeroLicencia = request.Numero!.Trim();
            licencia.FechaEmision = fechaEmision;
            licencia.FechaVencimiento = fechaVencimiento;
            licencia.Estado = EstadoVencimientoCalculator.Calcular(fechaVencimiento, hoy);
        }
        else
        {
            licencia.Estado = EstadoVencimiento.EnTramite;                     // ← estado

            // Checklist solo cuando el usuario dice "no la tengo" (H6/H11). Sin duplicar.
            if (!await checklists.ExisteParaLicenciaAsync(licencia.LicenciaSanitariaId, ct))
                await motor.AplicarALicenciaAsync(licencia, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);   // un solo guardado: licencia + checklist

        var tieneChecklist = await checklists.ExisteParaLicenciaAsync(licencia.LicenciaSanitariaId, ct);
        return licencia.ToDto(tipoDocumento, tieneChecklist, hoy);
    }
}