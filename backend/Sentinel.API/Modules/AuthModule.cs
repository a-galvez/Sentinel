using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sentinel.API.Modules;

// P1 · Sesión y plataforma (H2, GET /usuarios).
// Dueño: P1. Handlers en Application/Features/Auth y Application/Features/Usuarios;
// controladores en API/Controllers/Auth y API/Controllers/Usuarios.
public static class AuthModule
{
    public static IServiceCollection AddAuthModule(this IServiceCollection s, IConfiguration cfg) => s;
}
