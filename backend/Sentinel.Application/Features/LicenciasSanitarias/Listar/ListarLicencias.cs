using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Features.LicenciasSanitarias.Listar;

public sealed class ListarLicencias(
    ILicenciaRepository licencias,
    IEstablecimientoRepository establecimientos,
    IChecklistRepository checklists,
    IDocumentoRepository documentos,
    IClock clock)
{
    public async Task<PaginaDto<LicenciaListadoDto>> ManejarAsync(
        ListarLicenciasRequest request, CancellationToken ct)
    {
        var hoy = clock.HoyHonduras;

        // El validator ya garantizó que el texto es un nombre válido del enum.
        // Dependencia P2: GET /catalogos/estados-vencimiento aún no existe y el frontend
        // lo necesita para el selector de filtro. Ver Docs/sentinel-p4-pendientes.md.
        EstadoVencimiento? estado = request.Estado is null
            ? null
            : Enum.Parse<EstadoVencimiento>(request.Estado);

        var total = await licencias.CountAsync(estado, hoy, ct);
        var listaLicencias = (await licencias.GetPagedAsync(estado, hoy, request.Pagina, request.TamanoPagina, ct))
            .ToList();

        // Solo lo que necesita la página, cada cosa en una sola consulta (sin N+1).
        var establecimientosPorId = (await establecimientos.GetByIdsAsync(
                listaLicencias.Select(l => l.EstablecimientoId).Distinct().ToList(), ct))
            .ToDictionary(e => e.EstablecimientoId);

        var tiposDocumento = (await documentos.ObtenerTiposDocumentoAsync(
                listaLicencias.Select(l => l.TipoDocumentoId!.Value).Distinct().ToList(), ct))
            .ToDictionary(t => t.TipoDocumentoId);

        // TODO(H11): mantener este flag alineado con el GET de checklist cuando exista
        // (incluido soft-delete del checklist, si P5 lo agrega). Ver Docs/sentinel-p4-pendientes.md.
        var licenciasConChecklist = await checklists.LicenciaIdsConChecklistAsync(
            listaLicencias.Select(l => l.LicenciaSanitariaId).ToList(), ct);

        var items = listaLicencias
            .Select(l => l.ToListadoDto(
                establecimientosPorId[l.EstablecimientoId],
                tiposDocumento[l.TipoDocumentoId!.Value],
                licenciasConChecklist.Contains(l.LicenciaSanitariaId),
                hoy))
            .ToList();

        return new PaginaDto<LicenciaListadoDto>
        {
            Pagina = request.Pagina,
            TamanoPagina = request.TamanoPagina,
            Total = total,
            Items = items
        };
    }
}