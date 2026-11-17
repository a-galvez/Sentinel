namespace Sentinel.Domain.Entities;

public class RefreshToken
{
    public int RefreshTokenId { get; set; }
    public int UsuarioId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
    public DateTime? RevocadoEn { get; set; }
    public int? ReemplazadoPorId { get; set; }
}