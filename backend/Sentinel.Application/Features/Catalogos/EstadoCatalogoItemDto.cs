namespace Sentinel.Application.Features.Catalogos;

// Estado de vencimiento para filtros y semáforo.
// `Codigo` coincide con el nombre del enum EstadoVencimiento.
public class EstadoCatalogoItemDto
{
    public string Codigo { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
}
