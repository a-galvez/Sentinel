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
//
// Contenido: el ejemplo del PDF (Docs/sentinel-explicacion-y-ejemplo) — LIC-FARM, LIC-DROG y
// REG-MED con las reglas R1..R3 y la dependencia REG-MED → LIC-DROG. Es contenido ILUSTRATIVO:
// los requisitos legales reales deben verificarse contra la normativa de ARSA (H11 DoD).
public sealed class CatalogoSeeder(IAppDbContext db)
{
    public async Task SembrarAsync(CancellationToken ct)
    {
        await SembrarCategoriasProductoAsync(ct);
        await SembrarTiposEstablecimientoAsync(ct);
        await SembrarTiposTramiteAsync(ct);
        await SembrarTiposDocumentoAsync(ct);
        await SembrarRequisitosAsync(ct);
        await SembrarDocumentosRequisitoAsync(ct);
        await SembrarDependenciasAsync(ct);
        await SembrarReglasAsync(ct);

        await db.SaveChangesAsync(ct);
    }

    private async Task SembrarCategoriasProductoAsync(CancellationToken ct)
    {
        await AgregarSiFaltaAsync(db.CategoriasProducto, x => x.CategoriaProductoId == CatalogoIds.CategoriasProducto.Medicamento, ct,
            () => new CategoriaProducto { CategoriaProductoId = CatalogoIds.CategoriasProducto.Medicamento, Nombre = "Medicamento" });

        await AgregarSiFaltaAsync(db.CategoriasProducto, x => x.CategoriaProductoId == CatalogoIds.CategoriasProducto.DispositivoMedico, ct,
            () => new CategoriaProducto { CategoriaProductoId = CatalogoIds.CategoriasProducto.DispositivoMedico, Nombre = "Dispositivo médico" });

        await AgregarSiFaltaAsync(db.CategoriasProducto, x => x.CategoriaProductoId == CatalogoIds.CategoriasProducto.Cosmetico, ct,
            () => new CategoriaProducto { CategoriaProductoId = CatalogoIds.CategoriasProducto.Cosmetico, Nombre = "Cosmético" });

        await AgregarSiFaltaAsync(db.CategoriasProducto, x => x.CategoriaProductoId == CatalogoIds.CategoriasProducto.Alimento, ct,
            () => new CategoriaProducto { CategoriaProductoId = CatalogoIds.CategoriasProducto.Alimento, Nombre = "Alimento" });
    }

    private async Task SembrarTiposEstablecimientoAsync(CancellationToken ct)
    {
        await AgregarSiFaltaAsync(db.TiposEstablecimiento, x => x.TipoEstablecimientoId == CatalogoIds.TiposEstablecimiento.Farmacia, ct,
            () => new TipoEstablecimiento { TipoEstablecimientoId = CatalogoIds.TiposEstablecimiento.Farmacia, Nombre = "Farmacia" });

        await AgregarSiFaltaAsync(db.TiposEstablecimiento, x => x.TipoEstablecimientoId == CatalogoIds.TiposEstablecimiento.Botica, ct,
            () => new TipoEstablecimiento { TipoEstablecimientoId = CatalogoIds.TiposEstablecimiento.Botica, Nombre = "Botica" });

        await AgregarSiFaltaAsync(db.TiposEstablecimiento, x => x.TipoEstablecimientoId == CatalogoIds.TiposEstablecimiento.Drogueria, ct,
            () => new TipoEstablecimiento { TipoEstablecimientoId = CatalogoIds.TiposEstablecimiento.Drogueria, Nombre = "Droguería" });
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

    private async Task SembrarTiposDocumentoAsync(CancellationToken ct)
    {
        // Los tres documentos del ejemplo del PDF. La dependencia que da valor al MVP es
        // REG-MED → LIC-DROG (el registro del producto exige una licencia de droguería vigente).
        await AgregarSiFaltaAsync(db.TiposDocumento, x => x.TipoDocumentoId == CatalogoIds.TiposDocumento.RegistroSanitarioProducto, ct,
            () => new TipoDocumento
            {
                TipoDocumentoId = CatalogoIds.TiposDocumento.RegistroSanitarioProducto,
                Codigo = "REG-MED",
                Nombre = "Registro sanitario de medicamento",
                AplicaA = AplicaA.Producto,
                AutoridadEmisora = "ARSA",
                BaseLegal = "(por verificar)",
                VigenciaMeses = 60,
                Activo = true
            });

        await AgregarSiFaltaAsync(db.TiposDocumento, x => x.TipoDocumentoId == CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento, ct,
            () => new TipoDocumento
            {
                TipoDocumentoId = CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento,
                Codigo = "LIC-FARM",
                Nombre = "Licencia sanitaria de farmacia",
                AplicaA = AplicaA.Establecimiento,
                AutoridadEmisora = "ARSA",
                BaseLegal = "(por verificar)",
                VigenciaMeses = 12,
                Activo = true
            });

        await AgregarSiFaltaAsync(db.TiposDocumento, x => x.TipoDocumentoId == CatalogoIds.TiposDocumento.LicenciaSanitariaProducto, ct,
            () => new TipoDocumento
            {
                TipoDocumentoId = CatalogoIds.TiposDocumento.LicenciaSanitariaProducto,
                Codigo = "LIC-DROG",
                Nombre = "Licencia sanitaria de droguería",
                AplicaA = AplicaA.Establecimiento,
                AutoridadEmisora = "ARSA",
                BaseLegal = "(por verificar)",
                VigenciaMeses = 12,
                Activo = true
            });
    }

    private async Task SembrarRequisitosAsync(CancellationToken ct)
    {
        await AgregarRequisitoAsync(CatalogoIds.Requisitos.SolicitudArsa,
            "Solicitud en el formulario de ARSA", TipoRequisito.Documento, ct);

        await AgregarRequisitoAsync(CatalogoIds.Requisitos.DeclaracionJurada,
            "Declaración jurada del representante legal", TipoRequisito.Documento, ct);

        await AgregarRequisitoAsync(CatalogoIds.Requisitos.PlanosLocal,
            "Planos del local", TipoRequisito.Documento, ct);

        await AgregarRequisitoAsync(CatalogoIds.Requisitos.DatosRegente,
            "Datos del regente farmacéutico", TipoRequisito.Dato, ct);

        await AgregarRequisitoAsync(CatalogoIds.Requisitos.PagoTarifa,
            "Pago de la tarifa", TipoRequisito.Accion, ct);

        await AgregarRequisitoAsync(CatalogoIds.Requisitos.FichaTecnica,
            "Ficha técnica del producto", TipoRequisito.Documento, ct);

        await AgregarRequisitoAsync(CatalogoIds.Requisitos.CertificadoBpm,
            "Certificado de buenas prácticas de manufactura del fabricante", TipoRequisito.Documento, ct);

        await AgregarRequisitoAsync(CatalogoIds.Requisitos.Etiqueta,
            "Etiqueta del producto", TipoRequisito.Documento, ct);
    }

    private async Task AgregarRequisitoAsync(int id, string descripcion, TipoRequisito tipo, CancellationToken ct)
        => await AgregarSiFaltaAsync(db.Requisitos, x => x.RequisitoId == id, ct,
            () => new Requisito { RequisitoId = id, Descripcion = descripcion, TipoRequisito = tipo, Activo = true });

    private async Task SembrarDocumentosRequisitoAsync(CancellationToken ct)
    {
        // Del ejemplo del PDF, sección "DOCUMENTO_REQUISITO", trámite Nueva.
        // REG-MED · Nueva: los 6 requisitos.
        var regMed = (CatalogoIds.TiposDocumento.RegistroSanitarioProducto, CatalogoIds.TiposTramite.AltaProducto);
        await AgregarDocumentoRequisitoAsync(regMed.Item1, regMed.Item2, CatalogoIds.Requisitos.SolicitudArsa, orden: 1, ct);
        await AgregarDocumentoRequisitoAsync(regMed.Item1, regMed.Item2, CatalogoIds.Requisitos.DeclaracionJurada, orden: 2, ct);
        await AgregarDocumentoRequisitoAsync(regMed.Item1, regMed.Item2, CatalogoIds.Requisitos.FichaTecnica, orden: 3, ct);
        await AgregarDocumentoRequisitoAsync(regMed.Item1, regMed.Item2, CatalogoIds.Requisitos.CertificadoBpm, orden: 4, ct);
        await AgregarDocumentoRequisitoAsync(regMed.Item1, regMed.Item2, CatalogoIds.Requisitos.Etiqueta, orden: 5, ct);
        await AgregarDocumentoRequisitoAsync(regMed.Item1, regMed.Item2, CatalogoIds.Requisitos.PagoTarifa, orden: 6, ct);

        // LIC-FARM · Nueva: el PDF omite sus filas; se siembra la misma plantilla de licencia
        // que LIC-DROG para que una farmacia también tenga checklist.
        var licFarm = (CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento, CatalogoIds.TiposTramite.AltaEstablecimiento);
        await AgregarDocumentoRequisitoAsync(licFarm.Item1, licFarm.Item2, CatalogoIds.Requisitos.SolicitudArsa, orden: 1, ct);
        await AgregarDocumentoRequisitoAsync(licFarm.Item1, licFarm.Item2, CatalogoIds.Requisitos.DeclaracionJurada, orden: 2, ct);
        await AgregarDocumentoRequisitoAsync(licFarm.Item1, licFarm.Item2, CatalogoIds.Requisitos.PlanosLocal, orden: 3, ct);
        await AgregarDocumentoRequisitoAsync(licFarm.Item1, licFarm.Item2, CatalogoIds.Requisitos.DatosRegente, orden: 4, ct);
        await AgregarDocumentoRequisitoAsync(licFarm.Item1, licFarm.Item2, CatalogoIds.Requisitos.PagoTarifa, orden: 5, ct);

        // LIC-DROG · Nueva: los 5 requisitos. Es el documento que bloquea/desbloquea REG-MED.
        var licDrog = (CatalogoIds.TiposDocumento.LicenciaSanitariaProducto, CatalogoIds.TiposTramite.AltaEstablecimiento);
        await AgregarDocumentoRequisitoAsync(licDrog.Item1, licDrog.Item2, CatalogoIds.Requisitos.SolicitudArsa, orden: 1, ct);
        await AgregarDocumentoRequisitoAsync(licDrog.Item1, licDrog.Item2, CatalogoIds.Requisitos.DeclaracionJurada, orden: 2, ct);
        await AgregarDocumentoRequisitoAsync(licDrog.Item1, licDrog.Item2, CatalogoIds.Requisitos.PlanosLocal, orden: 3, ct);
        await AgregarDocumentoRequisitoAsync(licDrog.Item1, licDrog.Item2, CatalogoIds.Requisitos.DatosRegente, orden: 4, ct);
        await AgregarDocumentoRequisitoAsync(licDrog.Item1, licDrog.Item2, CatalogoIds.Requisitos.PagoTarifa, orden: 5, ct);
    }

    private async Task SembrarDependenciasAsync(CancellationToken ct)
    {
        // REG-MED requiere una LIC-DROG vigente (ejemplo del PDF: DOCUMENTO_DEPENDENCIA).
        var existe = await db.DocumentosDependencia.AnyAsync(
            x => x.TipoDocumentoId == CatalogoIds.TiposDocumento.RegistroSanitarioProducto
                 && x.RequiereTipoDocumentoId == CatalogoIds.TiposDocumento.LicenciaSanitariaProducto, ct);

        if (!existe)
        {
            await db.DocumentosDependencia.AddAsync(new DocumentoDependencia
            {
                TipoDocumentoId = CatalogoIds.TiposDocumento.RegistroSanitarioProducto,
                RequiereTipoDocumentoId = CatalogoIds.TiposDocumento.LicenciaSanitariaProducto
            }, ct);
        }
    }

    private async Task SembrarReglasAsync(CancellationToken ct)
    {
        // Reglas R1..R3 del ejemplo. Una condición en null significa "cualquiera".
        // R1: SI el establecimiento es Farmacia → LIC-FARM.
        await AgregarReglaAsync(CatalogoIds.Reglas.LicenciaFarmacia, CatalogoIds.TiposDocumento.LicenciaFuncionamientoEstablecimiento,
            tipoEstablecimientoId: CatalogoIds.TiposEstablecimiento.Farmacia, categoriaProductoId: null, ct);

        // R2: SI el establecimiento es Droguería → LIC-DROG.
        await AgregarReglaAsync(CatalogoIds.Reglas.LicenciaDrogueria, CatalogoIds.TiposDocumento.LicenciaSanitariaProducto,
            tipoEstablecimientoId: CatalogoIds.TiposEstablecimiento.Drogueria, categoriaProductoId: null, ct);

        // R3: SI el producto es Medicamento → REG-MED.
        await AgregarReglaAsync(CatalogoIds.Reglas.RegistroMedicamento, CatalogoIds.TiposDocumento.RegistroSanitarioProducto,
            tipoEstablecimientoId: null, categoriaProductoId: CatalogoIds.CategoriasProducto.Medicamento, ct);
    }

    private async Task AgregarReglaAsync(
        int reglaId, int tipoDocumentoId, int? tipoEstablecimientoId, int? categoriaProductoId, CancellationToken ct)
        => await AgregarSiFaltaAsync(db.ReglasAplicabilidad, x => x.ReglaAplicabilidadId == reglaId, ct,
            () => new ReglaAplicabilidad
            {
                ReglaAplicabilidadId = reglaId,
                TipoDocumentoId = tipoDocumentoId,
                TipoEstablecimientoId = tipoEstablecimientoId,
                CategoriaProductoId = categoriaProductoId
            });

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
