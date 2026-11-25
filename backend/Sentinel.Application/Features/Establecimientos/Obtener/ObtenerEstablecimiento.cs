using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Application.Features.Catalogos.TipoEstablecimiento;
using Sentinel.Application.Features.LicenciasSanitarias;

namespace Sentinel.Application.Features.Establecimientos.Obtener;

public sealed class ObtenerEstablecimiento(
    IEstablecimientoRepository establecimientos,
    ILicenciaRepository licencias,
    IChecklistRepository checklists,
    IDocumentoRepository documentos,
    ICatalogoRepository catalogos,
    IClock clock)
{
    public async Task<EstablecimientoDto> ManejarAsync(Guid establecimientoId, CancellationToken ct)
    {
        // El filtro global ya excluye lo de otra empresa y lo dado de baja - 404.
        var establecimiento = await establecimientos.ObtenerAsync(establecimientoId, ct)
            ?? throw new NotFoundException("Establecimiento no encontrado.");

        var nombreDeTipo = (await catalogos.TiposEstablecimientoAsync(ct))
            .FirstOrDefault(t => t.TipoEstablecimientoId == establecimiento.TipoEstablecimientoId)?.Nombre
            ?? string.Empty;

        var dto = new EstablecimientoDto
        {
            EstablecimientoId = establecimiento.EstablecimientoId,
            Nombre = establecimiento.Nombre,
            Ubicacion = establecimiento.Ubicacion,
            Tipo = new TipoEstablecimientoDto
            {
                Id = establecimiento.TipoEstablecimientoId,
                Nombre = nombreDeTipo
            }
        };

        // Sin licencia o sin tipo de documento (ninguna regla aplicó): licencia null.
        var licencia = await licencias.ObtenerPorEstablecimientoAsync(establecimientoId, ct);
        if (licencia?.TipoDocumentoId is not { } tipoDocumentoId)
            return dto;

        var tipoDocumento = await documentos.ObtenerTipoDocumentoAsync(tipoDocumentoId, ct);
        if (tipoDocumento is null)
            return dto;

        var tieneChecklist = await checklists.ExisteParaLicenciaAsync(licencia.LicenciaSanitariaId, ct);

        dto.Licencia = licencia.ToDto(tipoDocumento, tieneChecklist, clock.HoyHonduras);
        return dto;
    }
}