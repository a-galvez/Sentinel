using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Common.Exceptions;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Features.Requisitos;

// H11 BE-8: PATCH /requisitos/{id}. Cambia estado, responsable y fecha límite.
// - Al cumplir: guarda el usuario del JWT y la fecha/hora (UTC).
// - Al volver a pendiente: borra ese registro.
// - El responsable debe ser un usuario activo de la empresa.
public sealed class ActualizarRequisitoHandler(IAppDbContext db, ICurrentTenant tenant, IClock clock)
{
    public async Task<RequisitoDto> ManejarAsync(Guid checklistRequisitoId, RequisitoUpdateRequest request, CancellationToken ct)
    {
        // El filtro global ya limita a la empresa de la sesión → 404 si es de otra.
        var fila = await db.ChecklistRequisitos
            .SingleOrDefaultAsync(c => c.ChecklistRequisitoId == checklistRequisitoId, ct)
            ?? throw new NotFoundException("Requisito no encontrado.");

        if (request.ResponsableId is { } responsableId)
        {
            if (responsableId == Guid.Empty)
            {
                fila.ResponsableUsuarioId = null;
            }
            else
            {
                var existe = await db.Usuarios
                    .AnyAsync(u => u.UsuarioId == responsableId && u.Activo, ct);

                if (!existe)
                    throw new BadRequestException("RESPONSABLE_NO_VALIDO", "El responsable debe ser un usuario activo de la empresa.");

                fila.ResponsableUsuarioId = responsableId;
            }
        }

        if (request.FechaLimite is { } fechaLimite)
            fila.FechaLimite = fechaLimite;

        if (request.Estado is { } estado)
            AplicarEstado(fila, estado);

        await db.SaveChangesAsync(ct);

        return await LeerAsync(fila, ct);
    }

    private void AplicarEstado(ChecklistRequisito fila, EstadoRequisito estado)
    {
        if (estado == EstadoRequisito.Cumplido && fila.Estado != EstadoRequisito.Cumplido)
        {
            fila.Estado = EstadoRequisito.Cumplido;
            fila.CumplidoPorUsuarioId = tenant.UsuarioId;
            fila.FechaCumplimiento = clock.UtcAhora;
        }
        else if (estado == EstadoRequisito.Pendiente && fila.Estado != EstadoRequisito.Pendiente)
        {
            fila.Estado = EstadoRequisito.Pendiente;
            fila.CumplidoPorUsuarioId = null;
            fila.FechaCumplimiento = null;
        }
    }

    private async Task<RequisitoDto> LeerAsync(ChecklistRequisito fila, CancellationToken ct)
    {
        var requisito = await db.Requisitos
            .SingleAsync(r => r.RequisitoId == fila.RequisitoId, ct);

        var nombres = await NombresUsuarios.CargarAsync(
            db, new[] { fila.ResponsableUsuarioId, fila.CumplidoPorUsuarioId }, ct);

        var orden = await OrdenAsync(fila, ct);

        return new RequisitoDto(
            Id: fila.ChecklistRequisitoId,
            Orden: orden,
            Descripcion: requisito.Descripcion,
            Tipo: requisito.TipoRequisitoId,
            Estado: fila.Estado,
            ResponsableId: fila.ResponsableUsuarioId,
            ResponsableNombre: NombresUsuarios.Buscar(nombres, fila.ResponsableUsuarioId),
            FechaLimite: fila.FechaLimite,
            CumplidoPorId: fila.CumplidoPorUsuarioId,
            CumplidoPorNombre: NombresUsuarios.Buscar(nombres, fila.CumplidoPorUsuarioId),
            CumplidoEn: fila.FechaCumplimiento);
    }

    // El orden de presentación sale del catálogo, usando el tipo de documento dueño de la fila
    // (producto o licencia).
    private async Task<int> OrdenAsync(ChecklistRequisito fila, CancellationToken ct)
    {
        int? tipoDocumentoId = fila.ProductoId is { } productoId
            ? await db.Productos.Where(p => p.ProductoId == productoId).Select(p => p.TipoDocumentoId).FirstOrDefaultAsync(ct)
            : await db.Licencias.Where(l => l.LicenciaSanitariaId == fila.LicenciaSanitariaId).Select(l => l.TipoDocumentoId).FirstOrDefaultAsync(ct);

        if (tipoDocumentoId is null) return 0;

        return await db.DocumentosRequisito
            .Where(dr => dr.TipoDocumentoId == tipoDocumentoId
                         && dr.TipoTramiteId == fila.TipoTramiteId
                         && dr.RequisitoId == fila.RequisitoId)
            .Select(dr => (int?)dr.Orden)
            .FirstOrDefaultAsync(ct) ?? 0;
    }
}
