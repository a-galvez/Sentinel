namespace Sentinel.Domain.Entities;

public class DocumentoRequisito   
{
    public int TipoDocumentoId { get; set; }
    public int TipoTramiteId { get; set; }
    public int RequisitoId { get; set; }
    public bool Obligatorio { get; set; } = true;
    public int Orden { get; set; }                       
}