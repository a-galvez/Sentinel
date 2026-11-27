namespace Sentinel.Application.Abstractions;

// Resultado de emitir/rotar una sesión. `ExpiraEn` es el vencimiento del access token.
public class AuthTokens
{
    public AuthTokens(string accessToken, string refreshToken, DateTime expiraEn)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiraEn = expiraEn;
    }

    public string AccessToken { get; }
    public string RefreshToken { get; }
    public DateTime ExpiraEn { get; }
}
