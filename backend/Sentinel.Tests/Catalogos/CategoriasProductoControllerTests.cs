using Microsoft.AspNetCore.Mvc;
using Sentinel.API.Controllers.Catalogos;
using Sentinel.Application.Features.Catalogos;
using Sentinel.Domain.Entities;

namespace Sentinel.Tests.Catalogos;

public class CategoriasProductoControllerTests
{
    [Fact]
    public async Task Devuelve_categorias_mapeadas_a_id_y_nombre()
    {
        var catalogos = new FakeCatalogoRepository();
        catalogos.Categorias.Add(new CategoriaProducto { CategoriaProductoId = 1, Nombre = "Medicamento" });
        catalogos.Categorias.Add(new CategoriaProducto { CategoriaProductoId = 4, Nombre = "Alimento" });

        var controller = new CategoriasProductoController(catalogos);
        var result = await controller.Listar(default);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IReadOnlyList<CatalogoItemDto>>(ok.Value);

        Assert.Collection(items,
            item =>
            {
                Assert.Equal(1, item.Id);
                Assert.Equal("Medicamento", item.Nombre);
            },
            item =>
            {
                Assert.Equal(4, item.Id);
                Assert.Equal("Alimento", item.Nombre);
            });
    }

    [Fact]
    public async Task Devuelve_lista_vacia_si_no_hay_categorias()
    {
        var controller = new CategoriasProductoController(new FakeCatalogoRepository());

        var result = await controller.Listar(default);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IReadOnlyList<CatalogoItemDto>>(ok.Value);
        Assert.Empty(items);
    }
}
