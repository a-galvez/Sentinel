using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Features.Productos.DTOs;
using Sentinel.Application.Features.Productos.Mappers;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.API.Controllers.Productos;

[ApiController]
[Authorize]
[Tags("Productos")]
[Route("api/v1/productos")]
public class ProductosController : ProductosApiControllerBase
{
    private readonly IProductoRepository _productos;
    private readonly IRegistroSanitarioRepository _registros;
    private readonly ICatalogoRepository _catalogos;
    private readonly IDocumentoRepository _documentos;
    private readonly IChecklistRepository _checklists;
    private readonly IEmpresaRepository _empresas;
    private readonly ICurrentTenant _tenant;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CrearProductoRequest> _crearValidator;

    public ProductosController(
        IProductoRepository productos,
        IRegistroSanitarioRepository registros,
        ICatalogoRepository catalogos,
        IDocumentoRepository documentos,
        IChecklistRepository checklists,
        IEmpresaRepository empresas,
        ICurrentTenant tenant,
        IClock clock,
        IUnitOfWork unitOfWork,
        IValidator<CrearProductoRequest> crearValidator)
    {
        _productos = productos;
        _registros = registros;
        _catalogos = catalogos;
        _documentos = documentos;
        _checklists = checklists;
        _empresas = empresas;
        _tenant = tenant;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _crearValidator = crearValidator;
    }

    [HttpPost]
    public async Task<IActionResult> Crear(
        [FromBody] CrearProductoRequest request,
        CancellationToken ct)
    {
        var validacion = await _crearValidator.ValidateAsync(request, ct);
        if (!validacion.IsValid)
            return ValidacionInvalida(validacion);

        var empresaId = _tenant.EmpresaId;

        var existeCategoria = await _catalogos.ExisteCategoriaProductoAsync(
            request.CategoriaProductoId, ct);

        if (!existeCategoria)
        {
            return Problema(
                400,
                "Datos inválidos",
                "La categoría no existe.",
                new Dictionary<string, string[]>
                {
                    ["categoriaProductoId"] = new[] { "La categoría no existe." }
                });
        }

        var empresa = await _empresas.ObtenerAsync(empresaId, ct);

        var tipoDocumentoId = await _productos.ElegirTipoDocumentoAsync(
            request.CategoriaProductoId, ct);

        var producto = new Producto
        {
            ProductoId = Guid.NewGuid(),
            EmpresaId = empresaId,
            CategoriaProductoId = request.CategoriaProductoId,
            TipoDocumentoId = tipoDocumentoId,
            Nombre = request.Nombre.Trim(),
            Fabricante = empresa?.Nombre ?? string.Empty,
            DadoDeBaja = false
        };

        _productos.Agregar(producto);

        RegistroSanitario? registro = null;

        if (tipoDocumentoId is not null)
        {
            registro = new RegistroSanitario
            {
                RegistroSanitarioId = Guid.NewGuid(),
                ProductoId = producto.ProductoId,
                EmpresaId = empresaId,
                TipoDocumentoId = tipoDocumentoId.Value,
                Estado = EstadoVencimiento.NoVerificado,
                DadoDeBaja = false
            };

            _registros.Agregar(registro);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var productoDto = await ProductoMapper.ProductoAsync(
            producto,
            registro,
            _catalogos,
            _documentos,
            _checklists,
            _clock,
            ct);

        var response = new CrearProductoResponse
        {
            Producto = productoDto,
            Mensaje = tipoDocumentoId is null
                ? "No existe una regla que aplique a este producto. Se guardó sin registro sanitario ni checklist."
                : null
        };

        return Created($"/api/v1/productos/{producto.ProductoId}", response);
    }
}
