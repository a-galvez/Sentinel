using Sentinel.Domain.Common;

namespace Sentinel.Domain.Entities;

public class Producto : ITenantEntity, ISoftDelete
{
    public Guid ProductoId { get; set; }
    public Guid EmpresaId { get; set; }
    public int CategoriaProductoId { get; set; }
    public int? TipoDocumentoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public bool DadoDeBaja { get; set; }
}
