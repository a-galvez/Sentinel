namespace Sentinel.Application.Features.Establecimientos.Listar;

public class ListarEstablecimientosRequest
{
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 20;
}