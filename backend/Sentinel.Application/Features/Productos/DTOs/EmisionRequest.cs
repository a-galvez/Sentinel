namespace Sentinel.Application.Features.Productos.DTOs;

public class EmisionRequest
{
    public string Numero { get; set; } = string.Empty;
    public DateOnly? FechaEmision { get; set; }
}
