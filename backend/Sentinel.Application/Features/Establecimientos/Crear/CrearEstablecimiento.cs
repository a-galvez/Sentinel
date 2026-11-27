using FluentValidation;
using FluentValidation.Results;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Features.Catalogos.TipoEstablecimiento;
using Sentinel.Application.Features.LicenciasSanitarias;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Features.Establecimientos.Crear;

public class CrearEstablecimiento(
    IEstablecimientoRepository establecimientos,
    ILicenciaRepository licencias,
    ICatalogoRepository catalogos,
    IDocumentoRepository documentos,
    IMotorRequisitos motor,
    ICurrentTenant tenant,
    IUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<EstablecimientoCreadoDto> ManejarAsync(CrearEstablecimientoRequest request, CancellationToken ct)
    {
        // El tipo debe existir en el catálogo (catálogo global: sin filtro de empresa).
        // Dependencia P2: GET /catalogos/tipos-establecimiento aún no existe y el frontend
        // lo necesita para poblar la lista. Ver Docs/sentinel-p4-pendientes.md.
        var tipo = (await catalogos.TiposEstablecimientoAsync(ct))
            .FirstOrDefault(t => t.TipoEstablecimientoId == request.TipoEstablecimientoId)
            ?? throw new ValidationException(new[]
            {
                new ValidationFailure("tipoEstablecimientoId", "El tipo de establecimiento no existe.")
            });

        var establecimiento = new Establecimiento
        {
            EstablecimientoId = Guid.NewGuid(),
            EmpresaId = tenant.EmpresaId,
            TipoEstablecimientoId = tipo.TipoEstablecimientoId,
            Nombre = request.Nombre.Trim(),
            Ubicacion = request.Ubicacion.Trim()
        };
        establecimientos.Agregar(establecimiento);

        var dto = new EstablecimientoDto
        {
            EstablecimientoId = establecimiento.EstablecimientoId,
            Nombre = establecimiento.Nombre,
            Ubicacion = establecimiento.Ubicacion,
            Tipo = new TipoEstablecimientoDto { Id = tipo.TipoEstablecimientoId, Nombre = tipo.Nombre }
        };

        // El motor solo elige el documento. Si ninguna regla aplica: el establecimiento se
        // guarda sin licencia ni checklist (H11) y la respuesta lleva Licencia = null.
        var tipoDocumentoId = await motor.ElegirDocumentoParaEstablecimientoAsync(establecimiento, ct);
        if (tipoDocumentoId is { } documentoId)
        {
            var tipoDocumento = await documentos.ObtenerTipoDocumentoAsync(documentoId, ct)
                ?? throw new InvalidOperationException($"La regla apunta a un tipo de documento inexistente ({documentoId}).");

            var licencia = new LicenciaSanitaria
            {
                LicenciaSanitariaId = Guid.NewGuid(),
                EstablecimientoId = establecimiento.EstablecimientoId,
                EmpresaId = tenant.EmpresaId,
                TipoDocumentoId = documentoId,
                Estado = EstadoVencimiento.NoVerificado   // sin número ni fechas; sin checklist
            };
            licencias.Agregar(licencia);

            dto.Licencia = licencia.ToDto(tipoDocumento, tieneChecklist: false, clock.HoyHonduras);
        }

        await unitOfWork.SaveChangesAsync(ct);   // un solo guardado: establecimiento + licencia

        return new EstablecimientoCreadoDto
        {
            Establecimiento = dto,
            Mensaje = tipoDocumentoId is null
                ? "No existe una regla que aplique a este establecimiento. Se guardó sin licencia sanitaria ni checklist."
                : null
        };
    }
}
