using Sentinel.Domain.Common;
using Sentinel.Domain.Enums;

namespace Sentinel.Domain.Entities;

public class ChecklistRequisito : ITenantEntity
{
    public Guid ChecklistRequisitoId { get; set; }
    public Guid EmpresaId { get; set; }
    public int RequisitoId { get; set; }
    public int TipoTramiteId { get; set; }
    public Guid? ProductoId { get; set; }
    public Guid? LicenciaSanitariaId { get; set; }
    public Guid? ResponsableUsuarioId { get; set; }
    public Guid? CumplidoPorUsuarioId { get; set; }
    public EstadoRequisito Estado { get; set; } = EstadoRequisito.Pendiente;
    public DateOnly? FechaLimite { get; set; }
    public DateTime? FechaCumplimiento { get; set; } // UTC
    public int Orden { get; set; }
}
