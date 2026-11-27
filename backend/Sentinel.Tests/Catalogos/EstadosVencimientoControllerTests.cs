using Microsoft.AspNetCore.Mvc;
using Sentinel.API.Controllers.Catalogos;
using Sentinel.Application.Features.Catalogos;

namespace Sentinel.Tests.Catalogos;

public class EstadosVencimientoControllerTests
{
    [Fact]
    public void Devuelve_los_cinco_estados_en_el_orden_del_contrato()
    {
        var controller = new EstadosVencimientoController();

        var result = controller.Listar();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IReadOnlyList<EstadoCatalogoItemDto>>(ok.Value);

        Assert.Equal(5, items.Count);
        Assert.Equal(
            new[] { "NoVerificado", "EnTramite", "Vigente", "PorVencer", "Vencido" },
            items.Select(i => i.Codigo));
        Assert.Equal("No verificado", items[0].Nombre);
        Assert.Equal("En trámite", items[1].Nombre);
        Assert.Equal("Vigente", items[2].Nombre);
        Assert.Equal("Por vencer", items[3].Nombre);
        Assert.Equal("Vencido", items[4].Nombre);
    }
}
