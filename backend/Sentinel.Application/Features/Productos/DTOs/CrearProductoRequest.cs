namespace Sentinel.Application.Features.Productos.DTOs;

public class CrearProductoRequest
{
    public string Nombre { get; set; } = string.Empty;
    public int CategoriaProductoId { get; set; }
}
