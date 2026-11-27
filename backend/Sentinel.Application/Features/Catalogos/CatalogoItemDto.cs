namespace Sentinel.Application.Features.Catalogos;

// Elemento de catálogo simple: categorías de producto y tipos de establecimiento.
// El contrato lo llama CatalogoItem; usamos int porque la BD usa int.
public class CatalogoItemDto
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
}
