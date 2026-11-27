using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Common.Exceptions;

namespace Sentinel.Application.Features.Establecimientos.DarDeBaja;

public sealed class DarDeBajaEstablecimiento(
    IEstablecimientoRepository establecimientos,
    ILicenciaRepository licencias,
    IUnitOfWork unitOfWork)
{
    public async Task ManejarAsync(Guid establecimientoId, CancellationToken ct)
    {
        // El filtro global ya excluye lo de otra empresa y lo dado de baja - 404.
        var establecimiento = await establecimientos.ObtenerParaModificarAsync(establecimientoId, ct)
            ?? throw new NotFoundException("Establecimiento no encontrado.");

        establecimiento.DadoDeBaja = true;

        // TODO(H11): el checklist de la licencia no se marca como dado de baja porque
        // ChecklistRequisito no implementa ISoftDelete. Hoy queda inaccesible porque la
        // licencia y el establecimiento sí se filtran por DadoDeBaja; cuando exista el
        // endpoint/consulta de checklist, filtrar por el padre o agregar soft-delete.
        // Ver Docs/sentinel-p4-pendientes.md.

        // La licencia se da de baja junto con el establecimiento. Si no tiene (ninguna regla aplicó)
        // o ya estaba dada de baja, no hay nada más que hacer.
        var licencia = await licencias.ObtenerPorEstablecimientoParaModificarAsync(establecimientoId, ct);
        if (licencia is not null)
            licencia.DadoDeBaja = true;

        // Un solo SaveChanges: o se da de baja todo, o nada.
        await unitOfWork.SaveChangesAsync(ct);
    }
}