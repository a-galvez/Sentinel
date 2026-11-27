namespace Sentinel.Application.Features.Productos.DTOs;

public class ProductoListadoQuery
{
    public string? Q { get; set; }
    public string? Estado { get; set; }
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 20;
}
