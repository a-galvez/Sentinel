namespace Sentinel.Domain.Entities;

public class RefreshToken
{
    public Guid RefreshTokenId { get; set; }
    public Guid UsuarioId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime Expira { get; set; }
    public DateTime? RevocadoEn { get; set; }
    public Guid? ReemplazadoPorId { get; set; }
}
