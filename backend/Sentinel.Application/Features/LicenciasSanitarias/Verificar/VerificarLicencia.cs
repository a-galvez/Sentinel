using FluentValidation;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Domain.Calculos;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Features.LicenciasSanitarias.Verificar;

public sealed class VerificarLicencia(
    IEstablecimientoRepository establecimientos,
    ILicenciaRepository licencias,
    IChecklistRepository checklists,
    IDocumentoRepository documentos,
    IUnitOfWork unitOfWork,
    IClock clock,
    IValidator<VerificarLicenciaRequest> validator)
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

        // Si ya tiene fechas, aunque el estado guardado haya quedado obsoleto, ya fue emitida.
        if (licencia.Estado != EstadoVencimiento.NoVerificado || licencia.FechaVencimiento is not null)
            throw new ConflictException("La licencia ya fue verificada.");

        // Orden del contrato: 404 (existencias) → 409 (estado) → 400 (cuerpo).
        await validator.ValidateAndThrowAsync(request, ct);

        var tipoDocumento = await documentos.ObtenerTipoDocumentoAsync(tipoDocumentoId, ct)
            ?? throw new InvalidOperationException($"La licencia apunta a un tipo de documento inexistente ({tipoDocumentoId}).");

        var hoy = clock.HoyHonduras;

        if (request.TieneDocumento == true)
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
            licencia.Estado = EstadoVencimiento.EnTramite;

            // El motor ya no crea checklist: se copia aquí desde el documento. Sin duplicar.
            // TODO(H11): el contrato pide 409 si dos verificaciones concurrentes intentan crear
            // el checklist; hoy no hay transacción/índice único que lo garantice (TOCTOU).
            // Ver Docs/sentinel-p4-pendientes.md.
            if (!await checklists.ExisteParaLicenciaAsync(licencia.LicenciaSanitariaId, ct))
            {
                const int tramite = CatalogoIds.TiposTramite.AltaEstablecimiento;
                var requisitos = await documentos.RequisitosDeDocumentoAsync(tipoDocumentoId, tramite, ct);
                var ordenes = await documentos.OrdenesDeDocumentoAsync(tipoDocumentoId, tramite, ct);

                foreach (var requisitoId in requisitos)
                {
                    checklists.Agregar(new ChecklistRequisito
                    {
                        ChecklistRequisitoId = Guid.NewGuid(),
                        EmpresaId = licencia.EmpresaId,
                        RequisitoId = requisitoId,
                        TipoTramiteId = tramite,
                        LicenciaSanitariaId = licencia.LicenciaSanitariaId,
                        Estado = EstadoRequisito.Pendiente,
                        Orden = ordenes.GetValueOrDefault(requisitoId)
                    });
                }
            }
        }

        await unitOfWork.SaveChangesAsync(ct);   // un solo guardado: licencia + checklist

        var tieneChecklist = await checklists.ExisteParaLicenciaAsync(licencia.LicenciaSanitariaId, ct);
        return licencia.ToDto(tipoDocumento, tieneChecklist, hoy);
    }
}
