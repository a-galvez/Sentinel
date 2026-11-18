using Microsoft.Extensions.DependencyInjection;

namespace Sentinel.API.Modules;

// P2 · Registro de empresa y alta de productos (H1, H3).
// Dueño: P2. Handlers en Application/Features/Registro y Application/Features/Productos/Crear;
// controladores en API/Controllers/Registro y API/Controllers/Productos.
public static class RegistroModule
{
    public static IServiceCollection AddRegistroModule(this IServiceCollection s) => s;
}
