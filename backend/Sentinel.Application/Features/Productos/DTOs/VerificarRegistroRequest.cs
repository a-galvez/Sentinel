namespace Sentinel.Application.Features.Productos.DTOs;

public class VerificarRegistroRequest
{
    public bool? TieneDocumento { get; set; }
    public string? Numero { get; set; }
    public DateOnly? FechaEmision { get; set; }
}
