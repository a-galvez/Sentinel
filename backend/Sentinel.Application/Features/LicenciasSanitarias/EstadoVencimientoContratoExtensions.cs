using Sentinel.Domain.Enums;

namespace Sentinel.Application.Features.LicenciasSanitarias;

public static class EstadoVencimientoContratoExtensions
{
    public static string ToContrato(this EstadoVencimiento estado) => estado switch
    {
        EstadoVencimiento.Vigente => "Vigente",
        EstadoVencimiento.PorVencer => "PorVencer",
        EstadoVencimiento.Vencido => "Vencido",
        EstadoVencimiento.NoVerificado => "NoVerificado",
        EstadoVencimiento.EnTramite => "EnTramite",
        _ => throw new ArgumentOutOfRangeException(nameof(estado), estado, null)
    };
}
