using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sentinel.API.Controllers.Productos;
using Sentinel.Application.Abstractions;
using Sentinel.Application.Abstractions.Repositories;
using Sentinel.Application.Features.Productos.DTOs;
using Sentinel.Application.Features.Productos.Mappers;
using Sentinel.Domain.Calculos;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;

namespace Sentinel.API.Controllers.RegistrosSanitarios;

[ApiController]
[Authorize]
[Tags("Registros sanitarios")]
[Route("api/v1/productos/{productoId:guid}/registro")]
public class RegistrosSanitariosController : ProductosApiControllerBase
{
    private readonly IProductoRepository _productos;
    private readonly IRegistroSanitarioRepository _registros;
    private readonly IDocumentoRepository _documentos;
    private readonly IChecklistRepository _checklists;
    private readonly ICurrentTenant _tenant;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<VerificarRegistroRequest> _verificarValidator;
    private readonly IValidator<EmisionRequest> _emisionValidator;

    public RegistrosSanitariosController(
        IProductoRepository productos,
        IRegistroSanitarioRepository registros,
        IDocumentoRepository documentos,
        IChecklistRepository checklists,
        ICurrentTenant tenant,
        IClock clock,
        IUnitOfWork unitOfWork,
        IValidator<VerificarRegistroRequest> verificarValidator,
        IValidator<EmisionRequest> emisionValidator)
    {
        _productos = productos;
        _registros = registros;
        _documentos = documentos;
        _checklists = checklists;
        _tenant = tenant;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _verificarValidator = verificarValidator;
        _emisionValidator = emisionValidator;
    }

    [HttpPost("verificacion")]
    public async Task<IActionResult> Verificar(
        Guid productoId,
        [FromBody] VerificarRegistroRequest request,
        CancellationToken ct)
    {
        var validacion = await _verificarValidator.ValidateAsync(request, ct);
        if (!validacion.IsValid)
            return ValidacionInvalida(validacion);

        var producto = await _productos.ObtenerAsync(productoId, ct);
        if (producto is null)
            return Problema(404, "No encontrado", "Producto no encontrado.");

        var registro = await _registros.ObtenerPorProductoAsync(productoId, ct);
        if (registro is null)
            return Problema(
                404,
                "No encontrado",
                "El producto no tiene registro sanitario.");

        if (registro.Estado != EstadoVencimiento.NoVerificado)
            return Problema(409, "Conflicto", "El registro ya fue verificado.");

        if (request.TieneDocumento is null)
            return Problema(
                400,
                "Datos inválidos",
                "Debe indicar si tiene el documento.");

        if (request.TieneDocumento.Value)
        {
            if (string.IsNullOrWhiteSpace(request.Numero)
                || request.FechaEmision is null)
            {
                return Problema(
                    400,
                    "Datos inválidos",
                    "El número y la fecha de emisión van juntos.");
            }

            var numero = request.Numero.Trim();

            var duplicado = await _registros.ExisteNumeroAsync(
                _tenant.EmpresaId,
                numero,
                registro.RegistroSanitarioId,
                ct);

            if (duplicado)
            {
                return Problema(
                    409,
                    "Conflicto",
                    "El número de registro ya existe en tu empresa.",
                    new Dictionary<string, string[]>
                    {
                        ["numero"] = new[]
                        {
                            "El número de registro ya existe en tu empresa."
                        }
                    });
            }

            if (registro.TipoDocumentoId is null)
            {
                return Problema(
                    400,
                    "Datos inválidos",
                    "El registro no tiene documento asignado.");
            }

            var tipoDocumento = await _documentos.ObtenerTipoDocumentoAsync(
                registro.TipoDocumentoId.Value, ct);

            if (tipoDocumento is null)
            {
                return Problema(
                    400,
                    "Datos inválidos",
                    "El documento asignado al registro no existe.");
            }

            var fechaEmision = request.FechaEmision.Value;

            var fechaVencimiento = fechaEmision.AddMonths(
                tipoDocumento.VigenciaMeses);

            registro.NumeroRegistro = numero;
            registro.FechaEmision = fechaEmision;
            registro.FechaVencimiento = fechaVencimiento;
            registro.Estado = EstadoVencimientoCalculator.Calcular(
                fechaVencimiento,
                _clock.HoyHonduras);
        }
        else
        {
            if (registro.TipoDocumentoId is null)
            {
                return Problema(
                    400,
                    "Datos inválidos",
                    "El registro no tiene documento asignado.");
            }

            registro.Estado = EstadoVencimiento.EnTramite;

            // El motor ya no crea checklist: el caso de uso lo copia desde el documento.
            var requisitos = await _documentos.RequisitosDeDocumentoAsync(
                registro.TipoDocumentoId.Value,
                CatalogoIds.TiposTramite.AltaProducto,
                ct);

            foreach (var requisitoId in requisitos)
            {
                _checklists.Agregar(new ChecklistRequisito
                {
                    ChecklistRequisitoId = Guid.NewGuid(),
                    EmpresaId = _tenant.EmpresaId,
                    RequisitoId = requisitoId,
                    TipoTramiteId = CatalogoIds.TiposTramite.AltaProducto,
                    ProductoId = productoId,
                    Estado = EstadoRequisito.Pendiente
                });
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var tieneChecklist = await _checklists.ExisteParaProductoAsync(productoId, ct);

        var registroDto = await ProductoMapper.RegistroAsync(
            registro,
            tieneChecklist,
            _documentos,
            _clock,
            ct);

        return Ok(registroDto);
    }

    [HttpPatch]
    public async Task<IActionResult> RegistrarEmision(
        Guid productoId,
        [FromBody] EmisionRequest request,
        CancellationToken ct)
    {
        var validacion = await _emisionValidator.ValidateAsync(request, ct);
        if (!validacion.IsValid)
            return ValidacionInvalida(validacion);

        if (request.FechaEmision is null)
        {
            return Problema(
                400,
                "Datos inválidos",
                "La fecha de emisión es obligatoria.");
        }

        var producto = await _productos.ObtenerAsync(productoId, ct);

        if (producto is null)
            return Problema(404, "No encontrado", "Producto no encontrado.");

        var registro = await _registros.ObtenerPorProductoAsync(productoId, ct);
        if (registro is null)
        {
            return Problema(
                404,
                "No encontrado",
                "El producto no existe o no tiene registro sanitario.");
        }

        if (registro.TipoDocumentoId is null)
        {
            return Problema(
                400,
                "Datos inválidos",
                "El registro no tiene documento asignado.");
        }

        var tipoDocumento = await _documentos.ObtenerTipoDocumentoAsync(
            registro.TipoDocumentoId.Value, ct);

        if (tipoDocumento is null)
        {
            return Problema(
                400,
                "Datos inválidos",
                "El documento asignado al registro no existe.");
        }

        var numero = request.Numero.Trim();

        var duplicado = await _registros.ExisteNumeroAsync(
            _tenant.EmpresaId,
            numero,
            registro.RegistroSanitarioId,
            ct);

        if (duplicado)
        {
            return Problema(
                409,
                "Conflicto",
                "El número de registro ya existe en tu empresa.",
                new Dictionary<string, string[]>
                {
                    ["numero"] = new[]
                    {
                        "El número de registro ya existe en tu empresa."
                    }
                });
        }

        var fechaEmision = request.FechaEmision.Value;

        var fechaVencimiento = fechaEmision.AddMonths(
            tipoDocumento.VigenciaMeses);

        registro.NumeroRegistro = numero;
        registro.FechaEmision = fechaEmision;
        registro.FechaVencimiento = fechaVencimiento;
        registro.Estado = EstadoVencimientoCalculator.Calcular(
            fechaVencimiento,
            _clock.HoyHonduras);

        await _unitOfWork.SaveChangesAsync(ct);

        var tieneChecklist = await _checklists.ExisteParaProductoAsync(productoId, ct);

        var registroDto = await ProductoMapper.RegistroAsync(
            registro,
            tieneChecklist,
            _documentos,
            _clock,
            ct);

        return Ok(registroDto);
    }

    [HttpDelete]
    public async Task<IActionResult> DarDeBaja(
        Guid productoId,
        CancellationToken ct)
    {
        var producto = await _productos.ObtenerAsync(productoId, ct);
        if (producto is null)
            return Problema(404, "No encontrado", "Producto no encontrado.");

        var registro = await _registros.ObtenerPorProductoAsync(productoId, ct);
        if (registro is null)
        {
            return Problema(
                404,
                "No encontrado",
                "El producto no existe o no tiene registro sanitario.");
        }

        registro.DadoDeBaja = true;

        await _unitOfWork.SaveChangesAsync(ct);

        return NoContent();
    }

}
