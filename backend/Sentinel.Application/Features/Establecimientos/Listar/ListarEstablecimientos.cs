using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common;
using Sentinel.Application.Features.Catalogos.TipoEstablecimiento;
using Sentinel.Application.Features.LicenciasSanitarias;

namespace Sentinel.Application.Features.Establecimientos.Listar;

public sealed class ListarEstablecimientos(
    IEstablecimientoRepository establecimientos,
    ILicenciaRepository licencias,
    IChecklistRepository checklists,
    IDocumentoRepository documentos,
    ICatalogoRepository catalogos,
    IClock clock)
{
    public async Task<PaginaDto<EstablecimientoDto>> ManejarAsync(
        ListarEstablecimientosRequest request, CancellationToken ct)
    {
        var hoy = clock.HoyHonduras;

        var total = await establecimientos.CountAsync(ct);
        var listaEstablecimientos = (await establecimientos.GetPagedAsync(request.Pagina, request.TamanoPagina, ct))
            .ToList();

        // Solo las licencias de la página. Sin tipo de documento = ninguna regla aplicó: licencia null.
        var licenciasDePagina = (await licencias.GetByEstablecimientosAsync(
                listaEstablecimientos.Select(e => e.EstablecimientoId).ToList(), ct))
            .Where(l => l.TipoDocumentoId is not null)
            .ToList();
        var licenciasPorEstablecimiento = licenciasDePagina.ToLookup(l => l.EstablecimientoId);

        var tiposDocumento = (await documentos.ObtenerTiposDocumentoAsync(
                licenciasDePagina.Select(l => l.TipoDocumentoId!.Value).Distinct().ToList(), ct))
            .ToDictionary(t => t.TipoDocumentoId);
        var licenciasConChecklist = await checklists.LicenciaIdsConChecklistAsync(
            licenciasDePagina.Select(l => l.LicenciaSanitariaId).ToList(), ct);

        var nombresDeTipo = (await catalogos.TiposEstablecimientoAsync(ct))
            .ToDictionary(t => t.TipoEstablecimientoId, t => t.Nombre);

        var items = listaEstablecimientos
            .Select(e => new EstablecimientoDto
            {
                EstablecimientoId = e.EstablecimientoId,
                Nombre = e.Nombre,
                Ubicacion = e.Ubicacion,
                Tipo = new TipoEstablecimientoDto
                {
                    Id = e.TipoEstablecimientoId,
                    Nombre = nombresDeTipo.GetValueOrDefault(e.TipoEstablecimientoId, string.Empty)
                },
                Licencia = licenciasPorEstablecimiento[e.EstablecimientoId].FirstOrDefault() is { } licencia
                    && tiposDocumento.TryGetValue(licencia.TipoDocumentoId!.Value, out var tipo)
                        ? licencia.ToDto(tipo, licenciasConChecklist.Contains(licencia.LicenciaSanitariaId), hoy)
                        : null
            })
            .ToList();

        return new PaginaDto<EstablecimientoDto>
        {
            Pagina = request.Pagina,
            TamanoPagina = request.TamanoPagina,
            Total = total,
            Items = items
        };
    }
}