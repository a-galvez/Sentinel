namespace Sentinel.Application.Features.LicenciasSanitarias.Listar;

public class ListarLicenciasRequest
{
    public string? Estado { get; set; }   // NoVerificado | EnTramite | Vigente | PorVencer | Vencido
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 20;
}
