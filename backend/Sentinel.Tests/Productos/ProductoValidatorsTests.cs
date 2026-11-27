using Sentinel.Application.Features.Productos.DTOs;
using Sentinel.Application.Features.Productos.Validators;

namespace Sentinel.Tests.Productos;

public class ProductoValidatorsTests
{
    [Fact]
    public void Crear_producto_exige_nombre_y_categoria()
    {
        var validator = new CrearProductoValidator();

        var request = new CrearProductoRequest
        {
            Nombre = string.Empty,
            CategoriaProductoId = 0
        };

        var resultado = validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(CrearProductoRequest.Nombre));
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(CrearProductoRequest.CategoriaProductoId));
    }

    [Fact]
    public void Verificar_exige_numero_y_fecha_cuando_tiene_documento()
    {
        var validator = new VerificarRegistroValidator();

        var request = new VerificarRegistroRequest
        {
            TieneDocumento = true
        };

        var resultado = validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(VerificarRegistroRequest.Numero));
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(VerificarRegistroRequest.FechaEmision));
    }

    [Fact]
    public void Verificar_rechaza_datos_cuando_no_tiene_documento()
    {
        var validator = new VerificarRegistroValidator();

        var request = new VerificarRegistroRequest
        {
            TieneDocumento = false,
            Numero = "RS-001",
            FechaEmision = new DateOnly(2026, 6, 15)
        };

        var resultado = validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(VerificarRegistroRequest.Numero));
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(VerificarRegistroRequest.FechaEmision));
    }

    [Fact]
    public void Verificar_exige_indicar_si_tiene_documento()
    {
        var validator = new VerificarRegistroValidator();

        var request = new VerificarRegistroRequest();

        var resultado = validator.Validate(request);

        Assert.False(resultado.IsValid);
        Assert.Contains(resultado.Errors,
            e => e.PropertyName == nameof(VerificarRegistroRequest.TieneDocumento));
    }
}
