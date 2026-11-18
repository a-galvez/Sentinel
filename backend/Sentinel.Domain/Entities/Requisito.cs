namespace Sentinel.Domain.Entities;

using Sentinel.Domain.Enums;

public class Requisito
{
    public int RequisitoId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public TipoRequisito TipoRequisito { get; set; }
    public bool Activo { get; set; } = true;
}
