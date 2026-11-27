using Sentinel.Application.Features.Productos.DTOs;
using Sentinel.Application.Features.Productos.Validators;

namespace Sentinel.Tests.Productos;

public class ProductoValidatorsTests
{
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

    [Fact]
    public void Editar_acepta_solo_nombre()
    {
        var validator = new EditarProductoValidator();

        var resultado = validator.Validate(
            new EditarProductoRequest { Nombre = "Paracetamol" });

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Editar_rechaza_nombre_vacio()
    {
        var validator = new EditarProductoValidator();

        var resultado = validator.Validate(
            new EditarProductoRequest { Nombre = "   " });

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(EditarProductoRequest.Nombre));
    }

    [Fact]
    public void Editar_rechaza_nombre_largo()
    {
        var validator = new EditarProductoValidator();

        var resultado = validator.Validate(
            new EditarProductoRequest { Nombre = new string('a', 201) });

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(EditarProductoRequest.Nombre));
    }

    [Fact]
    public void Editar_rechaza_categoria_no_positiva()
    {
        var validator = new EditarProductoValidator();

        var resultado = validator.Validate(
            new EditarProductoRequest { CategoriaProductoId = 0 });

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(EditarProductoRequest.CategoriaProductoId));
    }

    [Fact]
    public void Emision_acepta_numero_y_fecha()
    {
        var validator = new EmisionRegistroValidator();

        var resultado = validator.Validate(new EmisionRequest
        {
            Numero = "RS-001",
            FechaEmision = new DateOnly(2026, 10, 15)
        });

        Assert.True(resultado.IsValid);
    }

    [Fact]
    public void Emision_rechaza_sin_numero()
    {
        var validator = new EmisionRegistroValidator();

        var resultado = validator.Validate(new EmisionRequest
        {
            Numero = "  ",
            FechaEmision = new DateOnly(2026, 10, 15)
        });

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(EmisionRequest.Numero));
    }

    [Fact]
    public void Emision_rechaza_sin_fecha()
    {
        var validator = new EmisionRegistroValidator();

        var resultado = validator.Validate(new EmisionRequest
        {
            Numero = "RS-001"
        });

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(EmisionRequest.FechaEmision));
    }

    [Fact]
    public void Emision_rechaza_numero_largo()
    {
        var validator = new EmisionRegistroValidator();

        var resultado = validator.Validate(new EmisionRequest
        {
            Numero = new string('n', 101),
            FechaEmision = new DateOnly(2026, 10, 15)
        });

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(EmisionRequest.Numero));
    }
}
