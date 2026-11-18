using Sentinel.Application.Abstractions;

namespace Sentinel.Infrastructure.Services;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string contrasena) => BCrypt.Net.BCrypt.HashPassword(contrasena);
    public bool Verificar(string hash, string contrasena) => BCrypt.Net.BCrypt.Verify(contrasena, hash);
}
