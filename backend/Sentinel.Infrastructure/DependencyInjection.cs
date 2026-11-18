using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Application.Abstractions;
using Sentinel.Infrastructure.Persistence;
using Sentinel.Infrastructure.Services;

namespace Sentinel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection s, IConfiguration cfg)
    {
        var connectionString = cfg.GetConnectionString("PostgresConnection")
                               ?? throw new InvalidOperationException("Falta la cadena de conexión 'PostgresConnection'.");

        s.AddHttpContextAccessor();
        s.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));
        s.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        s.AddScoped<ICurrentTenant, HttpCurrentTenant>();
        s.AddSingleton<IClock, HondurasClock>();
        s.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        // Motor vacío hasta M6. RequisitosModule de P5 lo reemplaza (ver RequisitosModule).
        s.AddScoped<IMotorRequisitos, MotorRequisitosVacio>();

        return s;
    }
}
