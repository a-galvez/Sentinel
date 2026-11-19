
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Infrastructure.Seeding;

namespace Sentinel.API.Modules;

// P5 · Motor de requisitos, checklist y PATCH (H11).
// Dueño: P5. Aquí se registra la implementación real de IMotorRequisitos (M6),
// sustituyendo a la implementación vacía de Infrastructure.
public static class RequisitosModule
{
    public static IServiceCollection AddRequisitosModule(this IServiceCollection s)
    {
        s.AddScoped<CatalogoSeeder>();
        s.AddHostedService<CatalogoSeederHostedService>();

        // PR2: cuando escribas MotorRequisitos, va aquí:
        // s.AddScoped<IMotorRequisitos, MotorRequisitos>();

        return s;
    }
}
