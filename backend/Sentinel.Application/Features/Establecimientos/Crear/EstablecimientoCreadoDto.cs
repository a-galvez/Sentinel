namespace Sentinel.Application.Features.Establecimientos;

public class EstablecimientoCreadoDto
{
    public EstablecimientoDto Establecimiento { get; set; } = new();
    public string? Mensaje { get; set; }   // presente solo si ninguna regla aplicó
}
