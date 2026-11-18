using Microsoft.Extensions.DependencyInjection;

namespace Sentinel.API.Modules;

// P3 · Listado de licencias con estado (H7).
// Dueño: P3. Handlers en Application/Features/Licencias.
public static class LicenciasModule
{
    public static IServiceCollection AddLicenciasModule(this IServiceCollection s) => s;
}
