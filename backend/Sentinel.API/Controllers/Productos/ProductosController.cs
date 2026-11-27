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
    private readonly IValidator<ProductoListadoQuery> _listadoValidator;
    private readonly IValidator<EditarProductoRequest> _editarValidator;

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
        IValidator<CrearProductoRequest> crearValidator,
        IValidator<ProductoListadoQuery> listadoValidator,
        IValidator<EditarProductoRequest> editarValidator)
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
        _listadoValidator = listadoValidator;
        _editarValidator = editarValidator;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] ProductoListadoQuery query,
        CancellationToken ct)
    {
        var validacion = await _listadoValidator.ValidateAsync(query, ct);
        if (!validacion.IsValid)
            return ValidacionInvalida(validacion);

        EstadoVencimiento? estado = null;
        if (!string.IsNullOrWhiteSpace(query.Estado))
            estado = Enum.Parse<EstadoVencimiento>(query.Estado, true);

        var skip = (query.Pagina - 1) * query.TamanoPagina;

        var (productos, total) = await _productos.ListarAsync(
            query.Q,
            estado,
            _clock.HoyHonduras,
            skip,
            query.TamanoPagina,
            ct);

        var ids = productos.Select(p => p.ProductoId).ToList();

        var registros = await _registros.ObtenerPorProductosAsync(ids, ct);
        var registrosPorProducto = registros.ToDictionary(r => r.ProductoId);

        var conChecklist = await _productos.ProductosConChecklistAsync(ids, ct);

        var items = new List<ProductoDto>();

        foreach (var producto in productos)
        {
            registrosPorProducto.TryGetValue(producto.ProductoId, out var registro);

            items.Add(await ProductoMapper.ProductoAsync(
                producto,
                registro,
                conChecklist.Contains(producto.ProductoId),
                _catalogos,
                _documentos,
                _clock,
                ct));
        }

        var response = new ProductoPaginaDto
        {
            Pagina = query.Pagina,
            TamanoPagina = query.TamanoPagina,
            Total = total,
            Items = items
        };

        return Ok(response);
    }

    [HttpGet("{productoId:guid}")]
    public async Task<IActionResult> Obtener(
        Guid productoId,
        CancellationToken ct)
    {
        var producto = await _productos.ObtenerAsync(productoId, ct);
        if (producto is null)
            return Problema(404, "No encontrado", "Producto no encontrado.");

        var registro = await _registros.ObtenerPorProductoAsync(productoId, ct);

        var tieneChecklist = registro is not null
            && await _checklists.ExisteParaProductoAsync(productoId, ct);

        var dto = await ProductoMapper.ProductoAsync(
            producto,
            registro,
            tieneChecklist,
            _catalogos,
            _documentos,
            _clock,
            ct);

        return Ok(dto);
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
            false,
            _catalogos,
            _documentos,
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

    [HttpPatch("{productoId:guid}")]
    public async Task<IActionResult> Editar(
        Guid productoId,
        [FromBody] EditarProductoRequest request,
        CancellationToken ct)
    {
        var validacion = await _editarValidator.ValidateAsync(request, ct);
        if (!validacion.IsValid)
            return ValidacionInvalida(validacion);

        if (request.Nombre is null && request.CategoriaProductoId is null)
        {
            return Problema(
                400,
                "Datos inválidos",
                "Debe enviar al menos un campo para editar.");
        }

        var producto = await _productos.ObtenerAsync(productoId, ct);
        if (producto is null)
            return Problema(404, "No encontrado", "Producto no encontrado.");

        if (request.CategoriaProductoId is not null
            && request.CategoriaProductoId.Value != producto.CategoriaProductoId)
        {
            return Problema(
                400,
                "Datos inválidos",
                "La categoría de un producto no se puede cambiar.",
                new Dictionary<string, string[]>
                {
                    ["categoriaProductoId"] = new[]
                    {
                        "La categoría de un producto no se puede cambiar."
                    }
                });
        }

        if (request.Nombre is not null)
            producto.Nombre = request.Nombre.Trim();

        await _unitOfWork.SaveChangesAsync(ct);

        var registro = await _registros.ObtenerPorProductoAsync(productoId, ct);

        var tieneChecklist = registro is not null
            && await _checklists.ExisteParaProductoAsync(productoId, ct);

        var dto = await ProductoMapper.ProductoAsync(
            producto,
            registro,
            tieneChecklist,
            _catalogos,
            _documentos,
            _clock,
            ct);

        return Ok(dto);
    }

    [HttpDelete("{productoId:guid}")]
    public async Task<IActionResult> DarDeBaja(
        Guid productoId,
        CancellationToken ct)
    {
        var producto = await _productos.ObtenerAsync(productoId, ct);
        if (producto is null)
            return Problema(404, "No encontrado", "Producto no encontrado.");

        producto.DadoDeBaja = true;

        await _unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }
}
