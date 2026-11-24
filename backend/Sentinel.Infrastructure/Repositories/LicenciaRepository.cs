
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Domain.Calculos;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Infrastructure.Repositories;

public sealed class LicenciaRepository(AppDbContext db) : ILicenciaRepository
{
    public Task<LicenciaSanitaria?> ObtenerAsync(Guid licenciaId, CancellationToken ct)
        => db.Licencias.SingleOrDefaultAsync(l => l.LicenciaSanitariaId == licenciaId, ct);

    public Task<int?> TipoEstablecimientoDeLicenciaAsync(Guid licenciaId, CancellationToken ct)
        => db.Licencias
            .Where(l => l.LicenciaSanitariaId == licenciaId)
            .Join(db.Establecimientos,
                  l => l.EstablecimientoId,
                  e => e.EstablecimientoId,
                  (l, e) => (int?)e.TipoEstablecimientoId)
            .SingleOrDefaultAsync(ct);

    // ⚠️ "Vigente" hoy = tiene fecha de vencimiento y no está vencida.
    // Cuando el Paso 3 introduzca EstadoVencimientoId, este cálculo se sustituye por
    // el estado almacenado (pasando el estado como filtro). El contrato no cambia.
    public async Task<List<int>> TiposVigentesEnEmpresaAsync(
        Guid empresaId, IReadOnlyList<int> tiposDocumento, CancellationToken ct)
    {
        if (tiposDocumento.Count == 0) return [];

        var hoy = HoyHonduras();
        var tipos = tiposDocumento.Distinct().ToList();

        // El filtro global de baja lógica ya excluye lo dado de baja.
        var licencias = await db.Licencias
            .Where(l => l.EmpresaId == empresaId
                        && l.TipoDocumentoId != null
                        && tipos.Contains(l.TipoDocumentoId.Value))
            .Select(l => new { Tipo = l.TipoDocumentoId!.Value, l.FechaVencimiento })
            .ToListAsync(ct);

        return licencias
            .Where(l => EstadoVencimientoCalculator.Calcular(l.FechaVencimiento, hoy) != EstadoVencimiento.Vencido)
            .Select(l => l.Tipo)
            .Distinct()
            .ToList();
    }

    // Honduras es UTC-6 todo el año (mismo criterio que HondurasClock, pero aquí no
    // inyectamos IClock para no acoplar el repo a una abstracción de dominio).
    private static DateOnly HoyHonduras() => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-6));
}
