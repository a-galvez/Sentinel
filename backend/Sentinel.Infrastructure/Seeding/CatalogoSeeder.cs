
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Sentinel.Application.Abstractions;
using Sentinel.Domain.Constantes;
using Sentinel.Domain.Entities;
using Sentinel.Domain.Enums;
using Sentinel.Infrastructure.Persistence;

namespace Sentinel.Infrastructure.Seeding;

// Siembra los catálogos fijos del motor de requisitos (M6).
// Idempotente: usa ids fijos y sólo inserta lo que falta; correrlo N veces deja la BD igual.
public sealed class CatalogoSeeder(IAppDbContext db)
{
    public async Task SembrarAsync(CancellationToken ct)
    {
        await SembrarTiposDocumentoAsync(ct);
        await SembrarRequisitosAsync(ct);
        await SembrarTiposTramiteAsync(ct);
        await SembrarDocumentosRequisitoAsync(ct);

        // ReglasAplicabilidad: vacía a propósito (decisión 1A: checklist uniforme por categoría).
        await db.SaveChangesAsync(ct);
    }

    private async Task SembrarTiposDocumentoAsync(CancellationToken ct)
    {
        await AgregarSiFaltaAsync(db.TiposDocumento, x => x.TipoDocumentoId == CatalogoIds.TiposDocumento.RegistroSanitarioProducto, ct,
            () => new TipoDocumento
            {
                TipoDocumentoId = CatalogoIds.TiposDocumento.RegistroSanitarioProducto,
                Codigo = "REG_SANITARIO_PROD",
                Nombre = "Registro sanitario de producto",
                AplicaA = AplicaA.Producto,
                AutoridadEmisora = "ARSA",
                BaseLegal = "Reglamento de registro sanitario",
                VigenciaMeses = 60,
                Activo = true
            });

        await AgregarSiFaltaAsync(db.TiposDocumento, x => x.TipoDocumentoId == CatalogoIds.TiposDocumento.LicenciaSanitariaProducto, ct,
            () => new TipoDocumento
            {
                TipoDocumentoId = CatalogoIds.TiposDocumento.LicenciaSanitariaProducto,
                Codigo = "LIC_SANITARIA_PROD",
                Nombre = "Licencia sanitaria de producto",
                AplicaA = AplicaA.Producto,
                AutoridadEmisora = "ARSA",
                BaseLegal = "Reglamento de licencias sanitarias",
                VigenciaMeses = 60,
                Activo = true
            });

        await AgregarSiFaltaAsync(db.TiposDocumento, x => x.TipoDocumentoId == CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento, ct,
            () => new TipoDocumento
            {
                TipoDocumentoId = CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento,
                Codigo = "LIC_FUNC_ESTAB",
                Nombre = "Licencia de funcionamiento del establecimiento",
                AplicaA = AplicaA.Establecimiento,
                AutoridadEmisora = "ARSA",
                BaseLegal = "Reglamento de establecimientos farmacéuticos",
                VigenciaMeses = 12,
                Activo = true
            });

        await AgregarSiFaltaAsync(db.TiposDocumento, x => x.TipoDocumentoId == CatalogoIds.TiposDocumento.RegistroPropietarioEstablecimiento, ct,
            () => new TipoDocumento
            {
                TipoDocumentoId = CatalogoIds.TiposDocumento.RegistroPropietarioEstablecimiento,
                Codigo = "REG_PROPIETARIO_ESTAB",
                Nombre = "Registro de propietario del establecimiento",
                AplicaA = AplicaA.Establecimiento,
                AutoridadEmisora = "ARSA",
                BaseLegal = "Reglamento de establecimientos farmacéuticos",
                VigenciaMeses = 12,
                Activo = true
            });

        await AgregarSiFaltaAsync(db.TiposDocumento, x => x.TipoDocumentoId == CatalogoIds.TiposDocumento.AvisoFuncionamientoEstablecimiento, ct,
            () => new TipoDocumento
            {
                TipoDocumentoId = CatalogoIds.TiposDocumento.AvisoFuncionamientoEstablecimiento,
                Codigo = "AVISO_FUNC_ESTAB",
                Nombre = "Aviso de funcionamiento del establecimiento",
                AplicaA = AplicaA.Establecimiento,
                AutoridadEmisora = "ARSA",
                BaseLegal = "Reglamento de establecimientos farmacéuticos",
                VigenciaMeses = 12,
                Activo = true
            });
    }

    private async Task SembrarRequisitosAsync(CancellationToken ct)
    {
        await AgregarSiFaltaAsync(db.Requisitos, x => x.RequisitoId == CatalogoIds.Requisitos.PresentarRegistroSanitario, ct,
            () => new Requisito
            {
                RequisitoId = CatalogoIds.Requisitos.PresentarRegistroSanitario,
                Descripcion = "Presentar el registro sanitario vigente del producto",
                TipoRequisito = TipoRequisito.Documento,
                Activo = true
            });

        await AgregarSiFaltaAsync(db.Requisitos, x => x.RequisitoId == CatalogoIds.Requisitos.PresentarLicenciaSanitaria, ct,
            () => new Requisito
            {
                RequisitoId = CatalogoIds.Requisitos.PresentarLicenciaSanitaria,
                Descripcion = "Presentar la licencia sanitaria vigente del producto",
                TipoRequisito = TipoRequisito.Documento,
                Activo = true
            });

        await AgregarSiFaltaAsync(db.Requisitos, x => x.RequisitoId == CatalogoIds.Requisitos.PresentarLicenciaFuncionamiento, ct,
            () => new Requisito
            {
                RequisitoId = CatalogoIds.Requisitos.PresentarLicenciaFuncionamiento,
                Descripcion = "Presentar la licencia de funcionamiento del establecimiento",
                TipoRequisito = TipoRequisito.Documento,
                Activo = true
            });

        await AgregarSiFaltaAsync(db.Requisitos, x => x.RequisitoId == CatalogoIds.Requisitos.PresentarRegistroPropietario, ct,
            () => new Requisito
            {
                RequisitoId = CatalogoIds.Requisitos.PresentarRegistroPropietario,
                Descripcion = "Presentar el registro del propietario del establecimiento",
                TipoRequisito = TipoRequisito.Documento,
                Activo = true
            });

        await AgregarSiFaltaAsync(db.Requisitos, x => x.RequisitoId == CatalogoIds.Requisitos.PresentarAvisoFuncionamiento, ct,
            () => new Requisito
            {
                RequisitoId = CatalogoIds.Requisitos.PresentarAvisoFuncionamiento,
                Descripcion = "Presentar el aviso de funcionamiento del establecimiento",
                TipoRequisito = TipoRequisito.Documento,
                Activo = true
            });
    }

    private async Task SembrarTiposTramiteAsync(CancellationToken ct)
    {
        await AgregarSiFaltaAsync(db.TiposTramite, x => x.TipoTramiteId == CatalogoIds.TiposTramite.AltaProducto, ct,
            () => new TipoTramite { TipoTramiteId = CatalogoIds.TiposTramite.AltaProducto, Nombre = "Alta de producto" });

        await AgregarSiFaltaAsync(db.TiposTramite, x => x.TipoTramiteId == CatalogoIds.TiposTramite.AltaEstablecimiento, ct,
            () => new TipoTramite { TipoTramiteId = CatalogoIds.TiposTramite.AltaEstablecimiento, Nombre = "Alta de establecimiento" });

        await AgregarSiFaltaAsync(db.TiposTramite, x => x.TipoTramiteId == CatalogoIds.TiposTramite.RenovacionLicencia, ct,
            () => new TipoTramite { TipoTramiteId = CatalogoIds.TiposTramite.RenovacionLicencia, Nombre = "Renovación de licencia" });
    }

    private async Task SembrarDocumentosRequisitoAsync(CancellationToken ct)
    {
        // Plantilla del alta de producto: 2 requisitos.
        await AgregarDocumentoRequisitoAsync(
            CatalogoIds.TiposDocumento.RegistroSanitarioProducto, CatalogoIds.TiposTramite.AltaProducto,
            CatalogoIds.Requisitos.PresentarRegistroSanitario, orden: 1, ct);

        await AgregarDocumentoRequisitoAsync(
            CatalogoIds.TiposDocumento.LicenciaSanitariaProducto, CatalogoIds.TiposTramite.AltaProducto,
            CatalogoIds.Requisitos.PresentarLicenciaSanitaria, orden: 2, ct);

        // Plantilla del alta de establecimiento: 3 requisitos.
        await AgregarDocumentoRequisitoAsync(
            CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento, CatalogoIds.TiposTramite.AltaEstablecimiento,
            CatalogoIds.Requisitos.PresentarLicenciaFuncionamiento, orden: 1, ct);

        await AgregarDocumentoRequisitoAsync(
            CatalogoIds.TiposDocumento.RegistroPropietarioEstablecimiento, CatalogoIds.TiposTramite.AltaEstablecimiento,
            CatalogoIds.Requisitos.PresentarRegistroPropietario, orden: 2, ct);

        await AgregarDocumentoRequisitoAsync(
            CatalogoIds.TiposDocumento.AvisoFuncionamientoEstablecimiento, CatalogoIds.TiposTramite.AltaEstablecimiento,
            CatalogoIds.Requisitos.PresentarAvisoFuncionamiento, orden: 3, ct);
    }

    private async Task AgregarDocumentoRequisitoAsync(
        int tipoDocumentoId, int tipoTramiteId, int requisitoId, int orden, CancellationToken ct)
    {
        var existe = await db.DocumentosRequisito.AnyAsync(
            x => x.TipoDocumentoId == tipoDocumentoId
                 && x.TipoTramiteId == tipoTramiteId
                 && x.RequisitoId == requisitoId, ct);

        if (!existe)
        {
            await db.DocumentosRequisito.AddAsync(new DocumentoRequisito
            {
                TipoDocumentoId = tipoDocumentoId,
                TipoTramiteId = tipoTramiteId,
                RequisitoId = requisitoId,
                Obligatorio = true,
                Orden = orden
            }, ct);
        }
    }

    private static async Task AgregarSiFaltaAsync<T>(
        DbSet<T> set, Expression<Func<T, bool>> filtro, CancellationToken ct, Func<T> crear)
        where T : class
    {
        if (!await set.AnyAsync(filtro, ct))
            await set.AddAsync(crear(), ct);
    }
}
