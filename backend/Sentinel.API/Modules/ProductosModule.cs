using Microsoft.Extensions.DependencyInjection;

namespace Sentinel.API.Modules;

// P3 · Lectura, edición y baja de productos (H4, H5).
// Dueño: P3. Handlers en Application/Features/Productos/{Listar,Editar,Baja}.
public static class ProductosModule
{
    public static IServiceCollection AddProductosModule(this IServiceCollection s) => s;
}
