namespace Sentinel.Application.Common;

public class PaginaDto<T>
{
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int Total { get; set; }
    public IReadOnlyList<T> Items { get; set; } = [];
}