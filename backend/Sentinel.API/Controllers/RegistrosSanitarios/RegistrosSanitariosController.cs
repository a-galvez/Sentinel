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
    private readonly IMotorRequisitos _motor;
    private readonly ICurrentTenant _tenant;
    private readonly IClock _clock;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<VerificarRegistroRequest> _verificarValidator;

    public RegistrosSanitariosController(
        IProductoRepository productos,
        IRegistroSanitarioRepository registros,
        IDocumentoRepository documentos,
        IChecklistRepository checklists,
        IMotorRequisitos motor,
        ICurrentTenant tenant,
        IClock clock,
        IUnitOfWork unitOfWork,
        IValidator<VerificarRegistroRequest> verificarValidator)
    {
        _productos = productos;
        _registros = registros;
        _documentos = documentos;
        _checklists = checklists;
        _motor = motor;
        _tenant = tenant;
        _clock = clock;
        _unitOfWork = unitOfWork;
        _verificarValidator = verificarValidator;
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

            var tipoDocumento = await _documentos.ObtenerTipoDocumentoAsync(
                registro.TipoDocumentoId, ct);

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
            registro.Estado = EstadoVencimiento.EnTramite;

            // P5 conecta el motor real. Copia los requisitos al checklist.
            await _motor.AplicarAProductoAsync(producto, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var registroDto = await ProductoMapper.RegistroAsync(
            registro,
            _documentos,
            _checklists,
            _clock,
            ct);

        return Ok(registroDto);
    }
}
