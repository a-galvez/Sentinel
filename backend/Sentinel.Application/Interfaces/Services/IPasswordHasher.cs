namespace Sentinel.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string contrasena);
    bool Verificar(string hash, string contrasena);
}
