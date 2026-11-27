namespace Sentinel.Application.Features.Establecimientos.Crear;

public class CrearEstablecimientoRequest
{
    public string Nombre { get; set; } = string.Empty;
    public int TipoEstablecimientoId { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
}
