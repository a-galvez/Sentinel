using Sentinel.Domain.Common;
using Sentinel.Domain.Enums;

namespace Sentinel.Domain.Entities;

public class RegistroSanitario : ITenantEntity, ISoftDelete
{
    public Guid RegistroSanitarioId { get; set; }
    public Guid ProductoId { get; set; }
    public Guid EmpresaId { get; set; }
    public int TipoDocumentoId { get; set; }
    public string? NumeroRegistro { get; set; }
    public DateOnly? FechaEmision { get; set; }
    public DateOnly? FechaVencimiento { get; set; }
    public EstadoVencimiento Estado { get; set; } = EstadoVencimiento.NoVerificado;
    public bool DadoDeBaja { get; set; }

    public Producto? Producto { get; set; }
}
