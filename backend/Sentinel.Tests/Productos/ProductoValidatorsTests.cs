using Sentinel.Application.Features.Productos.DTOs;
using Sentinel.Application.Features.Productos.Validators;

namespace Sentinel.Tests.Productos;

public class ProductoValidatorsTests
{
    // ... tests de H3 quedan igual ...

    [Fact]
    public void Listado_rechaza_pagina_y_tamano_invalidos()
    {
        var validator = new ProductoListadoValidator();

        var request = new ProductoListadoQuery
        {
            Pagina = 0,
            TamanoPagina = 0
        };

        var resultado = validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(ProductoListadoQuery.Pagina));
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(ProductoListadoQuery.TamanoPagina));
    }

    [Fact]
    public void Listado_rechaza_estado_invalido()
    {
        var validator = new ProductoListadoValidator();

        var request = new ProductoListadoQuery
        {
            Estado = "Foo"
        };

        var resultado = validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(ProductoListadoQuery.Estado));
    }

    [Fact]
    public void Listado_acepta_estado_valido()
    {
        var validator = new ProductoListadoValidator();

        var request = new ProductoListadoQuery
        {
            Pagina = 1,
            TamanoPagina = 20,
            Estado = "Vencido"
        };

        var resultado = validator.Validate(request);

        Assert.True(resultado.IsValid);
    }
}
