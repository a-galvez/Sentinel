using Sentinel.Application.Features.TiposDocumento;

namespace Sentinel.Application.Features.LicenciasSanitarias;

public class LicenciaListadoDto
{
    public Guid LicenciaId { get; set; }
    public LicenciaEstablecimientoDto Establecimiento { get; set; } = new();
    public TipoDocumentoResumenDto TipoDocumento { get; set; } = new();
    public string Estado { get; set; } = string.Empty;
    public string? Numero { get; set; }
    public DateOnly? FechaEmision { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public int? DiasParaVencer { get; set; }
    public bool RequiereVerificacion { get; set; }
    public bool TieneChecklist { get; set; }
}
