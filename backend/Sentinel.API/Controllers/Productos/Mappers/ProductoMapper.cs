using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Features.Productos.DTOs;
using Sentinel.Domain.Calculos;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.Application.Features.Productos.Mappers;

public static class ProductoMapper
{
    public static async Task<ProductoDto> ProductoAsync(
        Producto producto,
        RegistroSanitario? registro,
        bool tieneChecklist,
        ICatalogoRepository catalogos,
        IDocumentoRepository documentos,
        IClock clock,
        CancellationToken ct)
    {
        var categorias = await catalogos.CategoriasProductoAsync(ct);
        var categoria = categorias.Single(
            c => c.CategoriaProductoId == producto.CategoriaProductoId);

        return new ProductoDto
        {
            ProductoId = producto.ProductoId,
            Nombre = producto.Nombre,
            Categoria = new CategoriaProductoDto
            {
                Id = categoria.CategoriaProductoId,
                Nombre = categoria.Nombre
            },
            Registro = registro is null
                ? null
                : await RegistroAsync(registro, tieneChecklist, documentos, clock, ct)
        };
    }

    public static async Task<RegistroSanitarioDto> RegistroAsync(
        RegistroSanitario registro,
        bool tieneChecklist,
        IDocumentoRepository documentos,
        IClock clock,
        CancellationToken ct)
    {
        TipoDocumento? tipo = null;

        if (registro.TipoDocumentoId is int tipoDocumentoId)
        {
            tipo = await documentos.ObtenerTipoDocumentoAsync(tipoDocumentoId, ct);
        }

        var hoy = clock.HoyHonduras;

        var estado = EstadoVencimientoCalculator.CalcularRegistro(
            registro.Estado, registro.FechaVencimiento, hoy);

        var dias = registro.FechaVencimiento is { } fecha
            ? EstadoVencimientoCalculator.DiasRestantes(fecha, hoy)
            : (int?)null;

        return new RegistroSanitarioDto
        {
            RegistroId = registro.RegistroSanitarioId,
            ProductoId = registro.ProductoId,
            TipoDocumento = tipo is null
                ? null
                : new TipoDocumentoResumenDto
                {
                    TipoDocumentoId = tipo.TipoDocumentoId,
                    Codigo = tipo.Codigo,
                    Nombre = tipo.Nombre,
                    AutoridadEmisora = tipo.AutoridadEmisora,
                    BaseLegal = tipo.BaseLegal,
                    VigenciaMeses = tipo.VigenciaMeses
                },
            Estado = estado.ToString(),
            Numero = registro.NumeroRegistro,
            FechaEmision = registro.FechaEmision,
            FechaVencimiento = registro.FechaVencimiento,
            DiasParaVencer = dias,
            RequiereVerificacion =
                registro.Estado == EstadoVencimiento.NoVerificado,
            TieneChecklist = tieneChecklist
        };
    }
}
