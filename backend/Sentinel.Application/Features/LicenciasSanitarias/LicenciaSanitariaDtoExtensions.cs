using Sentinel.Application.Features.TiposDocumento;
using Sentinel.Domain.Calculos;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Features.LicenciasSanitarias;

public static class LicenciaSanitariaDtoExtensions
{
    public static LicenciaSanitariaDto ToDto(this LicenciaSanitaria licencia, TipoDocumento tipo, bool tieneChecklist, DateOnly hoy)
    {
        var vencimiento = licencia.FechaVencimiento;
        var estado = vencimiento is { } v
            ? EstadoVencimientoCalculator.Calcular(v, hoy)
            : licencia.Estado;                     // NoVerificado o EnTramite

        return new LicenciaSanitariaDto
        {
            LicenciaId = licencia.LicenciaSanitariaId,
            EstablecimientoId = licencia.EstablecimientoId,
            TipoDocumento = tipo.ToResumenDto(),
            Estado = estado.ToContrato(),
            Numero = licencia.NumeroLicencia,
            FechaEmision = licencia.FechaEmision,
            FechaVencimiento = vencimiento,
            DiasParaVencer = vencimiento is { } f ? EstadoVencimientoCalculator.DiasRestantes(f, hoy) : null,
            RequiereVerificacion = estado == EstadoVencimiento.NoVerificado,
            TieneChecklist = tieneChecklist
        };
    }
}
