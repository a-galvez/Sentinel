namespace Sentinel.Domain.Entities;

using Sentinel.Domain.Common;
using Sentinel.Domain.Enums;

public class ChecklistRequisito : ITenantEntity
{
    public int ChecklistRequisitoId { get; set; }
    public int EmpresaId { get; set; }
    public int RequisitoId { get; set; }
    public int TipoTramiteId { get; set; }
    public int? ProductoId { get; set; }         
    public int? LicenciaSanitariaId { get; set; }
    public int? ResponsableUsuarioId { get; set; }
    public int? CumplidoPorUsuarioId { get; set; }
    public EstadoRequisito Estado { get; set; } = EstadoRequisito.Pendiente;
    public DateOnly? FechaLimite { get; set; }
    public DateTime? FechaCumplimiento { get; set; } // UTC
}