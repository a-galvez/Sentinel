using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Features.Catalogos.TipoEstablecimiento;
using Sentinel.Application.Features.LicenciasSanitarias;
using Sentinel.Application.Features.TiposDocumento;
using Sentinel.Domain.Entities;

namespace Sentinel.Application.Features.Establecimientos.Crear;

public sealed class CrearEstablecimiento(
    IEstablecimientoRepository establecimientos,
    ILicenciaRepository licencias,
    IDocumentoRepository documentos,
    ICatalogoRepository catalogos,
    ICurrentTenant tenant,
    IUnitOfWork unitOfWork)
{
    public async Task<EstablecimientoCreadoDto> ManejarAsync(
        CrearEstablecimientoRequest request, CancellationToken ct)
    {
        var establecimiento = new Establecimiento
        {
            EstablecimientoId = Guid.NewGuid(),
            EmpresaId = tenant.EmpresaId,
            TipoEstablecimientoId = request.TipoEstablecimientoId,
            Nombre = request.Nombre.Trim(),
            Ubicacion = request.Ubicacion.Trim()
        };
        establecimientos.Agregar(establecimiento);

        var nombreDeTipo = (await catalogos.TiposEstablecimientoAsync(ct))
            .First(t => t.TipoEstablecimientoId == request.TipoEstablecimientoId).Nombre;

        var dto = new EstablecimientoDto
        {
            EstablecimientoId = establecimiento.EstablecimientoId,
            Nombre = establecimiento.Nombre,
            Ubicacion = establecimiento.Ubicacion,
            Tipo = new TipoEstablecimientoDto { Id = request.TipoEstablecimientoId, Nombre = nombreDeTipo }
        };

        // El motor elige un solo documento según el tipo (si varias reglas aplican, gana una).
        var tipoDocumentoId = await documentos.ElegirTipoDocumentoParaEstablecimientoAsync(
            request.TipoEstablecimientoId, ct);

        // Ninguna regla aplicó: se guarda sin licencia ni checklist y se avisa con un mensaje.
        if (tipoDocumentoId is null)
        {
            await unitOfWork.SaveChangesAsync(ct);
            return new EstablecimientoCreadoDto
            {
                Establecimiento = dto,
                Mensaje = "No existe una regla que aplique a este establecimiento. Se guardó sin licencia sanitaria ni checklist."
            };
        }

        var tipoDocumento = await documentos.ObtenerTipoDocumentoAsync(tipoDocumentoId.Value, ct)
            ?? throw new InvalidOperationException($"La regla apunta a un tipo de documento inexistente ({tipoDocumentoId}).");

        var licencia = new LicenciaSanitaria
        {
            LicenciaSanitariaId = Guid.NewGuid(),
            EstablecimientoId = establecimiento.EstablecimientoId,
            EmpresaId = tenant.EmpresaId,
            TipoDocumentoId = tipoDocumento.TipoDocumentoId
            // Número y fechas quedan en null hasta que el usuario los registre.
        };
        licencias.Agregar(licencia);

        await unitOfWork.SaveChangesAsync(ct);

        dto.Licencia = new LicenciaSanitariaDto
        {
            LicenciaId = licencia.LicenciaSanitariaId,
            EstablecimientoId = licencia.EstablecimientoId,
            TipoDocumento = tipoDocumento.ToResumenDto(),
            Estado = "NoVerificado",   // el enum EstadoVencimiento aún no tiene este valor
            Numero = null,
            FechaEmision = null,
            FechaVencimiento = null,
            DiasParaVencer = null,
            RequiereVerificacion = true,
            TieneChecklist = false
        };

        return new EstablecimientoCreadoDto { Establecimiento = dto };
    }
}