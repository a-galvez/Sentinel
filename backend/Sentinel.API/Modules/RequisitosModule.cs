using Microsoft.Extensions.DependencyInjection;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Features.Requisitos;
using Sentinel.Infrastructure.Seeding;
using Sentinel.Infrastructure.Services;

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

        // M6: motor real y evaluador de dependencias (reemplazan MotorRequisitosVacio).
        s.AddScoped<IMotorRequisitos, MotorRequisitos>();
        s.AddScoped<IEvaluaDependencias, EvaluadorDependencias>();

        // Handlers del checklist y del PATCH.
        s.AddScoped<ChecklistReader>();
        s.AddScoped<ActualizarRequisitoHandler>();

        return s;
    }
}
