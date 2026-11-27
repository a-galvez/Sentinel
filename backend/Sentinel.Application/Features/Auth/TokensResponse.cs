namespace Sentinel.Application.Features.Auth;

// Par de tokens según el contrato TokensResponse.
// `ExpiraEn` es el vencimiento del access token.
public class TokensResponse
{
    public TokensResponse(string accessToken, string refreshToken, DateTime expiraEn)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiraEn = expiraEn;
    }

    public string AccessToken { get; }
    public string RefreshToken { get; }
    public DateTime ExpiraEn { get; }
}
