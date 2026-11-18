using Sentinel.Domain.Calculos;
using Sentinel.Domain.Enums;

namespace Sentinel.Tests.Estado;

public class EstadoVencimientoCalculatorTests
{
    private static readonly DateOnly Hoy = new(2026, 6, 15);

    [Theory]
    [InlineData(-1, EstadoVencimiento.Vencido)]
    [InlineData(0, EstadoVencimiento.PorVencer)]
    [InlineData(1, EstadoVencimiento.PorVencer)]
    [InlineData(89, EstadoVencimiento.PorVencer)]
    [InlineData(90, EstadoVencimiento.PorVencer)]
    [InlineData(91, EstadoVencimiento.Vigente)]
    public void Calcula_los_limites_del_estado(int dias, EstadoVencimiento esperado)
    {
        var vencimiento = Hoy.AddDays(dias);
        Assert.Equal(esperado, EstadoVencimientoCalculator.Calcular(vencimiento, Hoy));
    }

    [Fact]
    public void DiasRestantes_es_negativo_si_ya_vencio()
    {
        Assert.Equal(-5, EstadoVencimientoCalculator.DiasRestantes(Hoy.AddDays(-5), Hoy));
    }
}
