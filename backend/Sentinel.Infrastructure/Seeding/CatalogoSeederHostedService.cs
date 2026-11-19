
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sentinel.Infrastructure.Seeding;

// Corre el seeder al arrancar la API. Usa un scope propio porque el seeder es Scoped
// (depende del DbContext) y el hosted service es Singleton.
public sealed class CatalogoSeederHostedService(
    IServiceProvider sp,
    ILogger<CatalogoSeederHostedService> log) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = sp.CreateAsyncScope();
            var seeder = scope.ServiceProvider.GetRequiredService<CatalogoSeeder>();
            await seeder.SembrarAsync(ct);
            log.LogInformation("Catálogo sembrado/verificado.");
        }
        catch (Exception ex)
        {
            // No dejamos que un fallo del seed tumbe la API; se puede sembrar luego con reintento.
            log.LogError(ex, "Falló el sembrado del catálogo.");
        }
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
