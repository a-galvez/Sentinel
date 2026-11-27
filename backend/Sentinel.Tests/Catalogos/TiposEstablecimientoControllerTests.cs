using Microsoft.AspNetCore.Mvc;
using Sentinel.API.Controllers.Catalogos;
using Sentinel.Application.Features.Catalogos;
using Sentinel.Domain.Entities;

namespace Sentinel.Tests.Catalogos;

public class TiposEstablecimientoControllerTests
{
    [Fact]
    public async Task Devuelve_tipos_mapeados_a_id_y_nombre()
    {
        var catalogos = new FakeCatalogoRepository();
        catalogos.TiposEstablecimiento.Add(new TipoEstablecimiento { TipoEstablecimientoId = 1, Nombre = "Farmacia" });
        catalogos.TiposEstablecimiento.Add(new TipoEstablecimiento { TipoEstablecimientoId = 3, Nombre = "Droguería" });

        var controller = new TiposEstablecimientoController(catalogos);
        var result = await controller.Listar(default);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IReadOnlyList<CatalogoItemDto>>(ok.Value);

        Assert.Collection(items,
            item =>
            {
                Assert.Equal(1, item.Id);
                Assert.Equal("Farmacia", item.Nombre);
            },
            item =>
            {
                Assert.Equal(3, item.Id);
                Assert.Equal("Droguería", item.Nombre);
            });
    }

    [Fact]
    public async Task Devuelve_lista_vacia_si_no_hay_tipos()
    {
        var controller = new TiposEstablecimientoController(new FakeCatalogoRepository());

        var result = await controller.Listar(default);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IReadOnlyList<CatalogoItemDto>>(ok.Value);
        Assert.Empty(items);
    }
}
