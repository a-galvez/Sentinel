namespace Sentinel.Application.Features.Productos.DTOs;

public class CategoriaProductoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public class TipoDocumentoResumenDto
{
    public int TipoDocumentoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string AutoridadEmisora { get; set; } = string.Empty;
    public string BaseLegal { get; set; } = string.Empty;
    public int VigenciaMeses { get; set; }
}

public class RegistroSanitarioDto
{
    public Guid RegistroId { get; set; }
    public Guid ProductoId { get; set; }
    public TipoDocumentoResumenDto? TipoDocumento { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? Numero { get; set; }
    public DateOnly? FechaEmision { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public int? DiasParaVencer { get; set; }
    public bool RequiereVerificacion { get; set; }
    public bool TieneChecklist { get; set; }
}

public class ProductoDto
{
    public Guid ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public CategoriaProductoDto Categoria { get; set; } = new();
    public RegistroSanitarioDto? Registro { get; set; }
}

public class CrearProductoResponse
{
    public ProductoDto Producto { get; set; } = new();
    public string? Mensaje { get; set; }
}
