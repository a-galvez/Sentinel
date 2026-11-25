using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Features.Establecimientos.Listar;
using Sentinel.Infrastructure.Repositories;
using Sentinel.Application.Features.Establecimientos.Crear;
using Sentinel.Application.Features.Establecimientos.Obtener;
using Sentinel.Application.Features.Establecimientos.DarDeBaja;

namespace Sentinel.API.Modules;

public static class EstablecimientosModule
{
    public static IServiceCollection AddEstablecimientosModule(this IServiceCollection s)
    {
        s.AddScoped<IEstablecimientoRepository, EstablecimientoRepository>();
        s.AddScoped<ILicenciaRepository, LicenciaRepository>();
        s.AddScoped<ListarEstablecimientos>();
        s.AddScoped<CrearEstablecimiento>();
        s.AddScoped<ObtenerEstablecimiento>();
        s.AddScoped<DarDeBajaEstablecimiento>();
        return s;
    }
}
