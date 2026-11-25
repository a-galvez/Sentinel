using Microsoft.Extensions.DependencyInjection;
using Sentinel.Application.Features.LicenciasSanitarias.DarDeBaja;
using Sentinel.Application.Features.LicenciasSanitarias.Listar;
using Sentinel.Application.Features.LicenciasSanitarias.Obtener;
using Sentinel.Application.Features.LicenciasSanitarias.RegistrarEmision;
using Sentinel.Application.Features.LicenciasSanitarias.Verificar;

namespace Sentinel.API.Modules;

public static class LicenciasModule
{
    public static IServiceCollection AddLicenciasModule(this IServiceCollection s)
    {
        s.AddScoped<ListarLicencias>();
        s.AddScoped<ObtenerLicencia>();
        s.AddScoped<RegistrarEmisionLicencia>();
        s.AddScoped<DarDeBajaLicencia>();
        s.AddScoped<VerificarLicencia>();
        return s;
    }
}
