namespace Sentinel.Domain.Entities;

using Sentinel.Domain.Enums;
public class TipoDocumento
{
    public int TipoDocumentoId { get; set; }
    public int AmbitoDocumentoId { get; set; }
    public string Codigo { get; set; } = string.Empty;   // índice único
    public string Nombre { get; set; } = string.Empty;
    public string AutoridadEmisora { get; set; } = string.Empty;
    public string BaseLegal { get; set; } = string.Empty;
    public int VigenciaMeses { get; set; }
    public bool Activo { get; set; } = true;

    public AmbitoDocumento? AmbitoDocumento { get; set; }
}
