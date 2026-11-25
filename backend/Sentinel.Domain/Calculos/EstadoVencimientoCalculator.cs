using Sentinel.Domain.Enums;

namespace Sentinel.Domain.Calculos;

public static class EstadoVencimientoCalculator
{
    public const int DiasUmbralPorVencer = 90;

    public static int DiasRestantes(DateOnly vencimiento, DateOnly hoy)
        => vencimiento.DayNumber - hoy.DayNumber;

    public static EstadoVencimiento Calcular(DateOnly vencimiento, DateOnly hoy)
    {
        var dias = DiasRestantes(vencimiento, hoy);
        if (dias < 0) return EstadoVencimiento.Vencido;
        if (dias <= DiasUmbralPorVencer) return EstadoVencimiento.PorVencer;
        return EstadoVencimiento.Vigente;
    }

    public static EstadoVencimiento Resolver(EstadoVencimiento almacenado, DateOnly? vencimiento, DateOnly hoy)
    => vencimiento is { } fecha ? Calcular(fecha, hoy) : almacenado;

    public static int? DiasRestantes(DateOnly? vencimiento, DateOnly hoy)
        => vencimiento is { } fecha ? DiasRestantes(fecha, hoy) : null;
}
