namespace Sentinel.Domain.Entities;

public class ReglaAplicabilidad   
{
    public int ReglaAplicabilidadId { get; set; }
    public int TipoDocumentoId { get; set; }
    public int? TipoEstablecimientoId { get; set; }
    public int? CategoriaProductoId { get; set; }
}