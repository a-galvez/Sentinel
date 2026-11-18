using Microsoft.Extensions.DependencyInjection;

namespace Sentinel.API.Modules;

// Catálogos (categorías de producto → P2, tipos de establecimiento → P4).
// Ambos dueños registran lo suyo aquí; por eso el archivo lo congela P1 y se edita por PR chico.
public static class CatalogosModule
{
    public static IServiceCollection AddCatalogosModule(this IServiceCollection s) => s;
}
