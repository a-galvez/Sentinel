using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Features.Requisitos;

// Construye el checklist de un producto o una licencia (H11 BE-6) y evalúa las
// dependencias al consultar (H11 BE-7). Lo reutiliza el expediente de H9.
public sealed class ChecklistReader(IAppDbContext db, IEvaluaDependencias dependencias)
{
    public async Task<ChecklistDto> ParaProductoAsync(Guid productoId, CancellationToken ct)
    {
        // El filtro global ya excluye lo de otra empresa y lo dado de baja → 404.
        var producto = await db.Productos
            .SingleOrDefaultAsync(p => p.ProductoId == productoId, ct)
            ?? throw new NotFoundException("Producto no encontrado.");

        return await ConstruirAsync(
            producto.TipoDocumentoId, producto.EmpresaId,
            productoId: producto.ProductoId, licenciaId: null, ct);
    }

    public async Task<ChecklistDto> ParaLicenciaAsync(Guid licenciaId, CancellationToken ct)
    {
        var licencia = await db.Licencias
            .SingleOrDefaultAsync(l => l.LicenciaSanitariaId == licenciaId, ct)
            ?? throw new NotFoundException("Licencia no encontrada.");

        return await ConstruirAsync(
            licencia.TipoDocumentoId, licencia.EmpresaId,
            productoId: null, licenciaId: licencia.LicenciaSanitariaId, ct);
    }

    private async Task<ChecklistDto> ConstruirAsync(
        int? tipoDocumentoId, Guid empresaId, Guid? productoId, Guid? licenciaId, CancellationToken ct)
    {
        if (tipoDocumentoId is null)
            return new ChecklistDto(null, null, [], new AvanceDto(0, 0), []);

        var tipoDocumento = await db.TiposDocumento
            .Where(t => t.TipoDocumentoId == tipoDocumentoId)
            .Select(t => new TipoDocumentoDto(t.TipoDocumentoId, t.Codigo, t.Nombre))
            .SingleOrDefaultAsync(ct);

        // Las filas del checklist, su requisito y el orden del catálogo se leen de una.
        var filas = await db.ChecklistRequisitos
            .Where(c => (productoId != null && c.ProductoId == productoId)
                        || (licenciaId != null && c.LicenciaSanitariaId == licenciaId))
            .Join(db.Requisitos, c => c.RequisitoId, r => r.RequisitoId, (c, r) => new { c, r })
            .OrderBy(x => x.c.TipoTramiteId).ThenBy(x => x.r.RequisitoId)
            .ToListAsync(ct);

        var tipoTramiteId = filas.Select(f => f.c.TipoTramiteId).FirstOrDefault();
        var tramite = await TramiteAsync(tipoTramiteId, ct);
        var ordenes = await OrdenesAsync(tipoDocumentoId.Value, tipoTramiteId, ct);

        var nombresUsuarios = await NombresUsuarios.CargarAsync(
            db,
            filas.Select(f => f.c.ResponsableUsuarioId).Concat(filas.Select(f => f.c.CumplidoPorUsuarioId)),
            ct);

        var requisitos = filas
            .Select(f => new RequisitoDto(
                Id: f.c.ChecklistRequisitoId,
                Orden: ordenes.TryGetValue(f.r.RequisitoId, out var orden) ? orden : 0,
                Descripcion: f.r.Descripcion,
                Tipo: f.r.TipoRequisitoId,
                Estado: f.c.Estado,
                ResponsableId: f.c.ResponsableUsuarioId,
                ResponsableNombre: NombresUsuarios.Buscar(nombresUsuarios, f.c.ResponsableUsuarioId),
                FechaLimite: f.c.FechaLimite,
                CumplidoPorId: f.c.CumplidoPorUsuarioId,
                CumplidoPorNombre: NombresUsuarios.Buscar(nombresUsuarios, f.c.CumplidoPorUsuarioId),
                CumplidoEn: f.c.FechaCumplimiento))
            .OrderBy(r => r.Orden)
            .ToList();

        var avance = new AvanceDto(
            Cumplidos: requisitos.Count(r => r.Estado == EstadoRequisito.Cumplido),
            Total: requisitos.Count);

        var bloqueo = (await dependencias.SinCumplirAsync(tipoDocumentoId.Value, empresaId, ct))
            .Select(d => new BloqueoPorDependenciaDto(d.TipoDocumentoId, d.TipoDocumentoNombre))
            .ToList();

        return new ChecklistDto(tramite, tipoDocumento, requisitos, avance, bloqueo);
    }

    private async Task<TramiteDto?> TramiteAsync(int? tipoTramiteId, CancellationToken ct)
    {
        if (tipoTramiteId is null) return null;
        return await db.TiposTramite
            .Where(t => t.TipoTramiteId == tipoTramiteId)
            .Select(t => new TramiteDto(t.TipoTramiteId, t.Nombre))
            .SingleOrDefaultAsync(ct);
    }

    // Orden de presentación según el catálogo (DocumentoRequisito.Orden).
    private async Task<Dictionary<int, int>> OrdenesAsync(int tipoDocumentoId, int tipoTramiteId, CancellationToken ct)
        => await db.DocumentosRequisito
            .Where(dr => dr.TipoDocumentoId == tipoDocumentoId && dr.TipoTramiteId == tipoTramiteId)
            .ToDictionaryAsync(dr => dr.RequisitoId, dr => dr.Orden, ct);
}
