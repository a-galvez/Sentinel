namespace Sentinel.Application.Features.Auth;

// Respuesta de login y registro (H1/H2): tokens + usuario de la sesión.
public class SesionResponse
{
    public SesionResponse(TokensResponse tokens, UsuarioActual usuario)
    {
        Tokens = tokens;
        Usuario = usuario;
    }

    public TokensResponse Tokens { get; }
    public UsuarioActual Usuario { get; }
}
