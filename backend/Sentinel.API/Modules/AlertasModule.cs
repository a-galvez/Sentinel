using Microsoft.Extensions.DependencyInjection;

namespace Sentinel.API.Modules;

// P4 · Alertas de vencimiento (H8).
// Dueño: P4. Handlers en Application/Features/Alertas.
public static class AlertasModule
{
    public static IServiceCollection AddAlertasModule(this IServiceCollection s) => s;
}
