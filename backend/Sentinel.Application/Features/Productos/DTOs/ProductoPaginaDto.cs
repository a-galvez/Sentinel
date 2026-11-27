namespace Sentinel.Application.Features.Productos.DTOs;

public class ProductoPaginaDto
{
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int Total { get; set; }
    public List<ProductoDto> Items { get; set; } = new();
}
