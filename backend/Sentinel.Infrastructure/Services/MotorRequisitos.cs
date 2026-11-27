using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Entities;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Infrastructure.Services;

/// <summary>
/// Motor de requisitos (H11). Resuelve una sola pregunta:
/// "¿qué documento le corresponde a este producto o establecimiento?".
///
/// Es una consulta pura: no escribe en entidades, no crea checklist y no llama a
/// SaveChanges. Los casos de uso de P3/P4 usan el resultado para crear el
/// <see cref="RegistroSanitario"/> o la <see cref="LicenciaSanitaria"/>.
///
/// Reglas:
/// - Si ninguna regla aplica devuelve <c>null</c>: el producto o establecimiento se guarda
///   sin documento, y quien llama muestra el mensaje de H11.
/// - Si varias reglas aplican, gana la de menor <c>ReglaAplicabilidadId</c> (H11 BE-4).
/// - Una condición nula en la regla significa "cualquiera".
/// </summary>

public sealed class MotorRequisitos(AppDbContext db) : IMotorRequisitos
{

    /// <summary>
    /// Devuelve el documento que aplica a la categoría del producto, o <c>null</c> si
    /// ninguna regla aplica.
    /// </summary>
    public Task<int?> ElegirDocumentoParaProductoAsync(Producto producto, CancellationToken ct)
        => ElegirTipoDocumentoAsync(

            categoriaProductoId: producto.CategoriaProductoId,
            tipoEstablecimientoId: null,
            ct);

    /// <summary>
    /// Devuelve el documento que aplica al tipo del establecimiento, o <c>null</c> si
    /// ninguna regla aplica.
    /// </summary>
    public Task<int?> ElegirDocumentoParaEstablecimientoAsync(
        Establecimiento establecimiento, CancellationToken ct
    ) => ElegirTipoDocumentoAsync(
        categoriaProductoId: null,
        tipoEstablecimientoId: establecimiento.TipoEstablecimientoId,
        ct);

    /// <summary>
    /// Busca en <c>REGLA_APLICABILIDAD</c> el documento que cumple las condiciones.
    ///
    /// Las condiciones son acumulativas (AND): si una regla tiene categoría y tipo a la vez,
    /// deben cumplirse ambas. Una condición nula se ignora, o sea que la regla aplica a
    /// cualquier valor de ese eje.
    ///
    /// Se ordena por <c>ReglaAplicabilidadId</c> para que la regla más antigua gane cuando
    /// varias coinciden. Es el desempate provisional hasta que exista <c>Prioridad</c>.
    /// </summary>
    private Task<int?> ElegirTipoDocumentoAsync(
        int? categoriaProductoId, int? tipoEstablecimientoId, CancellationToken ct
    ) => db.ReglasAplicabilidad
        .Where(r =>
            (r.CategoriaProductoId == null || r.CategoriaProductoId == categoriaProductoId)
            && (r.TipoEstablecimientoId == null || r.TipoEstablecimientoId == tipoEstablecimientoId))
        .OrderBy(r => r.ReglaAplicabilidadId)
        .Select(r => (int?)r.TipoDocumentoId)
        .FirstOrDefaultAsync(ct);
}
