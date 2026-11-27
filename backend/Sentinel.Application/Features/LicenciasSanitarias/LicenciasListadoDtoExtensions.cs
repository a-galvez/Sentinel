using Sentinel.Domain.Entities;

namespace Sentinel.Application.Features.LicenciasSanitarias;

public static class LicenciaListadoDtoExtensions
{
    public static LicenciaListadoDto ToListadoDto(
        this LicenciaSanitaria licencia, Establecimiento establecimiento, TipoDocumento tipo,
        bool tieneChecklist, DateOnly hoy)
    {
        var dto = licencia.ToDto(tipo, tieneChecklist, hoy);

        return new LicenciaListadoDto
        {
            LicenciaId = dto.LicenciaId,
            Establecimiento = new LicenciaEstablecimientoDto
            {
                EstablecimientoId = establecimiento.EstablecimientoId,
                Nombre = establecimiento.Nombre
            },
            TipoDocumento = dto.TipoDocumento,
            Estado = dto.Estado,
            Numero = dto.Numero,
            FechaEmision = dto.FechaEmision,
            FechaVencimiento = dto.FechaVencimiento,
            DiasParaVencer = dto.DiasParaVencer,
            RequiereVerificacion = dto.RequiereVerificacion,
            TieneChecklist = dto.TieneChecklist
        };
    }
}
