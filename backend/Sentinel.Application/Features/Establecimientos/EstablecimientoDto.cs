using Sentinel.Application.Features.Catalogos.TipoEstablecimiento;
using Sentinel.Application.Features.LicenciasSanitarias;

namespace Sentinel.Application.Features.Establecimientos;

public class EstablecimientoDto
{
    public Guid EstablecimientoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoEstablecimientoDto Tipo { get; set; } = new();
    public string Ubicacion { get; set; } = string.Empty;
    public LicenciaSanitariaDto? Licencia { get; set; }   //null si ninguna regla aplicó
}