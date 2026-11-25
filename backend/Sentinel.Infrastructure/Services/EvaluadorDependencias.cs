using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions;
using Sentinel.Domain.Calculos;
using Sentinel.Domain.Enums;

namespace Sentinel.Infrastructure.Services;

// Implementación de IEvaluaDependencias (H11 BE-7). Evalúa DOCUMENTO_DEPENDENCIA al consultar:
// no se guarda nada. Una dependencia se cumple si la empresa tiene una licencia de ese tipo no
// dada de baja y no vencida (fecha de vencimiento >= hoy).
public sealed class EvaluadorDependencias(IAppDbContext db, IClock clock) : IEvaluaDependencias
{
    public async Task<IReadOnlyList<DependenciaPendiente>> SinCumplirAsync(
        int tipoDocumentoId, Guid empresaId, CancellationToken ct)
    {
        var requiere = await db.DocumentosDependencia
            .Where(d => d.TipoDocumentoId == tipoDocumentoId)
            .Select(d => d.RequiereTipoDocumentoId)
            .ToListAsync(ct);

        if (requiere.Count == 0) return [];

        var hoy = clock.HoyHonduras;

        // Se filtra por empresa explícitamente: el filtro global depende de que haya sesión.
        // El filtro global de baja lógica ya deja fuera lo dado de baja.
        var licencias = await db.Licencias
            .Where(l => l.EmpresaId == empresaId && l.TipoDocumentoId != null && requiere.Contains(l.TipoDocumentoId.Value))
            .Select(l => new { l.TipoDocumentoId, l.FechaVencimiento })
            .ToListAsync(ct);

        var bloqueantes = new List<DependenciaPendiente>();
        foreach (var requerido in requiere)
        {
            // Sin fecha de vencimiento (No verificado / En trámite) no cuenta como cumplida.
            var vigente = licencias.Any(l =>
                l.TipoDocumentoId == requerido
                && l.FechaVencimiento is { } vencimiento
                && EstadoVencimientoCalculator.Calcular(vencimiento, hoy) != EstadoVencimiento.Vencido);

            if (!vigente)
            {
                var nombre = await db.TiposDocumento
                    .Where(t => t.TipoDocumentoId == requerido)
                    .Select(t => t.Nombre)
                    .SingleOrDefaultAsync(ct) ?? $"Tipo de documento {requerido}";

                bloqueantes.Add(new DependenciaPendiente(requerido, nombre));
            }
        }

        return bloqueantes;
    }
}
