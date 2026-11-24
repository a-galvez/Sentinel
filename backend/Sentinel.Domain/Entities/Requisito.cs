namespace Sentinel.Domain.Entities;

public class Requisito
{
    public int RequisitoId { get; set; }
    public int TipoRequisitoId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public TipoRequisito? TipoRequisito { get; set; }
}
