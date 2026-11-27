
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Infrastructure.Persistence;
using Sentinel.Infrastructure.Repositories;
using Sentinel.Infrastructure.Services;
//Motor Real
using Sentinel.Infrastructure.Seeding;

namespace Sentinel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection s, IConfiguration cfg)
    {
        var connectionString = cfg.GetConnectionString("PostgresConnection")
                               ?? throw new InvalidOperationException("Falta la cadena de conexión 'PostgresConnection'.");

        s.AddHttpContextAccessor();
        s.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));
        s.AddScoped<ICurrentTenant, HttpCurrentTenant>();
        s.AddSingleton<IClock, HondurasClock>();
        s.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();


        // Repositorios (patrón repositorio). Conviven con IAppDbContext hasta que los handlers
        // estén migrados; luego se elimina IAppDbContext.
        s.AddScoped<IUnitOfWork, UnitOfWork>();
        s.AddScoped<IUsuarioRepository, UsuarioRepository>();
        s.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        s.AddScoped<ICatalogoRepository, CatalogoRepository>();
        s.AddScoped<IDocumentoRepository, DocumentoRepository>();
        s.AddScoped<IChecklistRepository, ChecklistRepository>();
        s.AddScoped<ILicenciaRepository, LicenciaRepository>();
        s.AddScoped<IProductoRepository, ProductoRepository>();
        s.AddScoped<IRegistroSanitarioRepository, RegistroSanitarioRepository>();

        s.AddScoped<IMotorRequisitos, MotorRequisitos>();
        s.AddScoped<IEvaluaDependencias, EvaluadorDependencias>();

        s.AddScoped<CatalogoSeeder>();
        s.AddHostedService<CatalogoSeederHostedService>();

        return s;
    }
}
