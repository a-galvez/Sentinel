using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Entities;

namespace Sentinel.Infrastructure.Services;

// Motor real de requisitos (H11 BE-4, M6). Sustituye a MotorRequisitosVacio vía RequisitosModule.
//
// Qué hace: evalúa REGLA_APLICABILIDAD con la categoría del producto o el tipo del
// establecimiento, asigna el TipoDocumento (si hay varias reglas, la de menor id) y copia los
// requisitos del trámite "nueva" a CHECKLIST_REQUISITO.
//
// Qué NO hace: no llama SaveChanges. Deja el TipoDocumento y las filas del checklist en el
// DbContext para que el caso de uso del alta haga un solo SaveChanges (misma transacción).
public sealed class MotorRequisitos(IAppDbContext db) : IMotorRequisitos
{
    public async Task AplicarAProductoAsync(Producto producto, CancellationToken ct)
    {
        var tipoDocumentoId = await ElegirTipoDocumentoAsync(
            categoriaProductoId: producto.CategoriaProductoId,
            tipoEstablecimientoId: null,
            ct);

        producto.TipoDocumentoId = tipoDocumentoId;
        if (tipoDocumentoId is null) return;

        await CopiarRequisitosAsync(
            tipoDocumentoId.Value,
            CatalogoIds.TiposTramite.AltaProducto,
            producto.EmpresaId,
            productoId: producto.ProductoId,
            licenciaId: null,
            ct);
    }

    public async Task AplicarALicenciaAsync(LicenciaSanitaria licencia, CancellationToken ct)
    {
        // La licencia no trae el tipo del establecimiento: se lee de la navegación o de la BD.
        var tipoEstablecimientoId = licencia.Establecimiento?.TipoEstablecimientoId
            ?? await db.Establecimientos
                .Where(e => e.EstablecimientoId == licencia.EstablecimientoId)
                .Select(e => (int?)e.TipoEstablecimientoId)
                .SingleOrDefaultAsync(ct);

        var tipoDocumentoId = await ElegirTipoDocumentoAsync(
            categoriaProductoId: null,
            tipoEstablecimientoId: tipoEstablecimientoId,
            ct);

        licencia.TipoDocumentoId = tipoDocumentoId;
        if (tipoDocumentoId is null) return;

        await CopiarRequisitosAsync(
            tipoDocumentoId.Value,
            CatalogoIds.TiposTramite.AltaEstablecimiento,
            licencia.EmpresaId,
            productoId: null,
            licenciaId: licencia.LicenciaSanitariaId,
            ct);
    }

    // Reglas activas combinadas con O; una condición nula significa "cualquiera".
    // Si varias aplican, gana la de menor ReglaAplicabilidadId (H11 BE-4).
    private async Task<int?> ElegirTipoDocumentoAsync(
        int? categoriaProductoId, int? tipoEstablecimientoId, CancellationToken ct)
        => await db.ReglasAplicabilidad
            .Where(r =>
                (r.CategoriaProductoId == null || r.CategoriaProductoId == categoriaProductoId)
                && (r.TipoEstablecimientoId == null || r.TipoEstablecimientoId == tipoEstablecimientoId))
            .OrderBy(r => r.ReglaAplicabilidadId)
            .Select(r => (int?)r.TipoDocumentoId)
            .FirstOrDefaultAsync(ct);

    private async Task CopiarRequisitosAsync(
        int tipoDocumentoId, int tipoTramiteId, Guid empresaId,
        Guid? productoId, Guid? licenciaId, CancellationToken ct)
    {
        var requisitos = await db.DocumentosRequisito
            .Where(dr => dr.TipoDocumentoId == tipoDocumentoId && dr.TipoTramiteId == tipoTramiteId)
            .OrderBy(dr => dr.Orden)
            .Select(dr => dr.RequisitoId)
            .ToListAsync(ct);

        foreach (var requisitoId in requisitos)
        {
            await db.ChecklistRequisitos.AddAsync(new ChecklistRequisito
            {
                ChecklistRequisitoId = Guid.NewGuid(),
                EmpresaId = empresaId,
                RequisitoId = requisitoId,
                TipoTramiteId = tipoTramiteId,
                ProductoId = productoId,
                LicenciaSanitariaId = licenciaId
            }, ct);
        }
    }
}
