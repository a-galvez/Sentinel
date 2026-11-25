using Sentinel.Application.Features.TiposDocumento;

namespace Sentinel.Application.Features.LicenciasSanitarias;

public class LicenciaSanitariaDto
{
    public Guid LicenciaId { get; set; }
    public Guid EstablecimientoId { get; set; }
    public TipoDocumentoResumenDto TipoDocumento { get; set; } = new();
    public string Estado { get; set; } = string.Empty;   // Vigente | PorVencer | Vencido | NoVerificado | EnTramite
    public string? Numero { get; set; }
    public DateOnly? FechaEmision { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public int? DiasParaVencer { get; set; }
    public bool RequiereVerificacion { get; set; }
    public bool TieneChecklist { get; set; }
}