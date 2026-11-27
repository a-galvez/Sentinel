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

    //NoVerificado y EnTramite son estados previos a la emisión. Una vez emitido se recalcula desde la fecha vencimiento.
    public static EstadoVencimiento CalcularRegistro(
        EstadoVencimiento estado, DateOnly? fechaVencimiento, DateOnly hoy)
    {
        if (estado is EstadoVencimiento.NoVerificado or EstadoVencimiento.EnTramite)
            return estado;

        return fechaVencimiento is null ? estado : Calcular(fechaVencimiento.Value, hoy);
    }
}
