using Microsoft.Extensions.DependencyInjection;

namespace Sentinel.API.Modules;

// P5 · Motor de requisitos, checklist y PATCH (H11).
// Dueño: P5. Aquí se registra la implementación real de IMotorRequisitos (M6),
// sustituyendo a la implementación vacía de Infrastructure.
public static class RequisitosModule
{
    public static IServiceCollection AddRequisitosModule(this IServiceCollection s) => s;
}
