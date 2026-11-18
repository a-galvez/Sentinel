using Microsoft.Extensions.DependencyInjection;

namespace Sentinel.API.Modules;

// P5 · Dashboard de riesgo (H10).
// Dueño: P5. Handlers en Application/Features/Dashboard.
public static class DashboardModule
{
    public static IServiceCollection AddDashboardModule(this IServiceCollection s) => s;
}
