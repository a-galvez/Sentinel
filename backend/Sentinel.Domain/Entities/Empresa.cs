namespace Sentinel.Domain.Entities;

public class Empresa
{
    public int EmpresaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }   
}