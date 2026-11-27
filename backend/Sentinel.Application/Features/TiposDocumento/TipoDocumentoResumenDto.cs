namespace Sentinel.Application.Features.TiposDocumento;

public class TipoDocumentoResumenDto
{
    public int TipoDocumentoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string AutoridadEmisora { get; set; } = string.Empty;
    public string BaseLegal { get; set; } = string.Empty;
    public int VigenciaMeses { get; set; }
}
