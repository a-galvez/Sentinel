namespace Sentinel.Application.Features.Auth.Refresh;

// DTO de entrada del POST /auth/refresh y POST /auth/logout. El contrato usa `refreshToken`.
public class RefreshRequest
{
    public RefreshRequest()
    {
    }

    public RefreshRequest(string refreshToken)
    {
        RefreshToken = refreshToken;
    }

    public string RefreshToken { get; init; } = string.Empty;
}
