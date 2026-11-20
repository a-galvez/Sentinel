using Sentinel.Application.Abstractions;

namespace Sentinel.Infrastructure.Services;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string contrasena) => BCrypt.Net.BCrypt.HashPassword(contrasena);

    public bool Verificar(string hash, string contrasena)
    {
        // Un hash malformado no debe tumbar el login con un 500:
        // se trata como una contraseña incorrecta (INVALID_CREDENTIALS).
        try
        {
            return BCrypt.Net.BCrypt.Verify(contrasena, hash);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
