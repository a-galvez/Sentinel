namespace Sentinel.Application.Common;

// Normaliza el correo para que la unicidad y el login no dependan de mayúsculas ni espacios.
public static class Correo
{
    public static string Normalizar(string correo) => correo.Trim().ToLowerInvariant();
}
