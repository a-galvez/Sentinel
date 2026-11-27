
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

    // Vigente = tiene fecha de vencimiento y no está vencida.
    // No verificado y En trámite (sin fecha) no cuentan, como pide H11.
    public async Task<List<int>> TiposVigentesEnEmpresaAsync(
        Guid empresaId, IReadOnlyList<int> tiposDocumento, CancellationToken ct)
    {
        if (tiposDocumento.Count == 0) return [];

        var hoy = HoyHonduras();
        var tipos = tiposDocumento.Distinct().ToList();

        // El filtro global de baja lógica ya excluye lo dado de baja.
        var licencias = await db.Licencias
            .Where(l => l.EmpresaId == empresaId
                        && l.FechaVencimiento != null
                        && l.TipoDocumentoId != null
                        && tipos.Contains(l.TipoDocumentoId.Value))
            .Select(l => new { Tipo = l.TipoDocumentoId!.Value, FechaVencimiento = l.FechaVencimiento!.Value })
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

    public async Task<IEnumerable<LicenciaSanitaria>> GetByEstablecimientosAsync(
    IReadOnlyList<Guid> establecimientoIds, CancellationToken ct)
    {
        if (establecimientoIds.Count == 0) return [];

        var ids = establecimientoIds.ToList();
        return await db.Licencias
            .AsNoTracking()
            .Where(l => ids.Contains(l.EstablecimientoId))
            .ToListAsync(ct);
    }

    public void Agregar(LicenciaSanitaria licencia)
    => db.Licencias.Add(licencia);

    public Task<LicenciaSanitaria?> ObtenerPorEstablecimientoAsync(Guid establecimientoId, CancellationToken ct)
    => db.Licencias
        .AsNoTracking()
        .SingleOrDefaultAsync(l => l.EstablecimientoId == establecimientoId, ct);

    public Task<LicenciaSanitaria?> ObtenerPorEstablecimientoParaModificarAsync(Guid establecimientoId, CancellationToken ct)
    => db.Licencias
        .SingleOrDefaultAsync(l => l.EstablecimientoId == establecimientoId, ct);

    public Task<int> CountAsync(EstadoVencimiento? estado, DateOnly hoy, CancellationToken ct)
    => Filtrar(estado, hoy).CountAsync(ct);

    public async Task<IEnumerable<LicenciaSanitaria>> GetPagedAsync(
        EstadoVencimiento? estado, DateOnly hoy, int pagina, int tamanoPagina, CancellationToken ct)
        => await Filtrar(estado, hoy)
            .AsNoTracking()
            .OrderBy(l => l.FechaVencimiento)
            .ThenBy(l => l.LicenciaSanitariaId)
            .Skip((pagina - 1) * tamanoPagina)
            .Take(tamanoPagina)
            .ToListAsync(ct);

    //los rangos de fecha replican EstadoVencimientoCalculator.Calcular, para que el filtro y el estado que se muestra nunca se contradigan.
    private IQueryable<LicenciaSanitaria> Filtrar(EstadoVencimiento? estado, DateOnly hoy)
    {
        // El Any sobre Establecimientos hereda el filtro global (empresa y baja): total e items coinciden.
        var consulta = db.Licencias.Where(l =>
            l.TipoDocumentoId != null
            && db.Establecimientos.Any(e => e.EstablecimientoId == l.EstablecimientoId));

        if (estado is null) return consulta;

        var limite = hoy.AddDays(EstadoVencimientoCalculator.DiasUmbralPorVencer);

        return estado switch
        {
            EstadoVencimiento.Vencido => consulta.Where(l => l.FechaVencimiento < hoy),
            EstadoVencimiento.PorVencer => consulta.Where(l => l.FechaVencimiento >= hoy && l.FechaVencimiento <= limite),
            EstadoVencimiento.Vigente => consulta.Where(l => l.FechaVencimiento > limite),
            // NoVerificado y EnTramite no tienen fechas. Exigir FechaVencimiento nula evita
            // que una licencia ya emitida (con fechas) aparezca por un Estado guardado obsoleto.
            _ => consulta.Where(l => l.FechaVencimiento == null && l.Estado == estado)
        };
    }
}
