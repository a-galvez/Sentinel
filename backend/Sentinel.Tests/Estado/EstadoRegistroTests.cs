using Sentinel.Domain.Calculos;
using Sentinel.Domain.Enums;

namespace Sentinel.Tests.Estado;

public class EstadoRegistroTests
{
    private static readonly DateOnly Hoy = new(2026, 6, 15);

    [Theory]
    [InlineData(EstadoVencimiento.NoVerificado)]
    [InlineData(EstadoVencimiento.EnTramite)]
    public void Estados_previos_a_la_emision_se_respetan(EstadoVencimiento estado)
    {
        Assert.Equal(
            estado,
            EstadoVencimientoCalculator.CalcularRegistro(estado, null, Hoy));
    }

    [Theory]
    [InlineData(-1, EstadoVencimiento.Vencido)]
    [InlineData(0, EstadoVencimiento.PorVencer)]
    [InlineData(90, EstadoVencimiento.PorVencer)]
    [InlineData(91, EstadoVencimiento.Vigente)]
    public void Registro_emitido_calcula_el_estado_por_fecha(
        int dias, EstadoVencimiento esperado)
    {
        var vencimiento = Hoy.AddDays(dias);

        var estado = EstadoVencimientoCalculator.CalcularRegistro(
            EstadoVencimiento.Vigente, vencimiento, Hoy);

        Assert.Equal(esperado, estado);
    }

    [Fact]
    public void Registro_emitido_sin_fecha_conserva_el_estado_guardado()
    {
        Assert.Equal(
            EstadoVencimiento.Vigente,
            EstadoVencimientoCalculator.CalcularRegistro(
                EstadoVencimiento.Vigente, null, Hoy));
    }
}
