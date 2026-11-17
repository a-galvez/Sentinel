namespace Sentinel.Domain.Entities;

using Sentinel.Domain.Common;

public class Producto : ITenantEntity, ISoftDelete
{
    public int ProductoId { get; set; }
    public int EmpresaId { get; set; }
    public int CategoriaProductoId { get; set; }
    public int? TipoDocumentoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Fabricante { get; set; } = string.Empty;
    public string NumeroRegistro { get; set; } = string.Empty;
    public DateOnly FechaEmision { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public bool DadoDeBaja { get; set; }
}