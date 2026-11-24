
namespace Sentinel.Domain.Constantes;

public static class CatalogoIds
{
    public static class CategoriasProducto
    {
        public const int Medicamento = 1;
        public const int DispositivoMedico = 2;
        public const int Cosmetico = 3;
        public const int Alimento = 4;
    }

    public static class TiposEstablecimiento
    {
        public const int Farmacia = 1;
        public const int Botica = 2;
        public const int Drogueria = 3;
    }

    public static class TiposTramite
    {
        public const int AltaProducto = 1;
        public const int AltaEstablecimiento = 2;
        public const int RenovacionLicencia = 3;
    }

    public static class TiposDocumento
    {
        // El sufijo indica a qué entidad se asocia el documento.
        // El Codigo (string, único en BD) es el identificador estable de negocio;
        // estos int son los ids que el seed fija para que el código no dependa de la secuencia.
        // El nombre sigue el flujo, no el nombre legal del documento: el valor es el mismo
        // tipo de documento aunque el Codigo (REG-MED, LIC-FARM, LIC-DROG) diga otra cosa.
        public const int RegistroSanitarioProducto = 1;             // Producto · REG-MED
        public const int PermisoImportacionProducto = 2;            // Producto (reservado)
        public const int LicenciaSanitariaProducto = 3;             // Establecimiento · LIC-DROG
        public const int LicenciaFuncionamientoEstablecimiento = 4; // Establecimiento · LIC-FARM
        public const int RegistroPropietarioEstablecimiento = 5;    // Establecimiento (reservado)
        public const int AvisoFuncionamientoEstablecimiento = 6;    // Establecimiento (reservado)
    }

    public static class Requisitos
    {
        // Ids del ejemplo del PDF (Docs/sentinel-explicacion-y-ejemplo), trámite Nueva.
        public const int SolicitudArsa = 1;          // Documento
        public const int DeclaracionJurada = 2;      // Documento
        public const int PlanosLocal = 3;            // Documento
        public const int DatosRegente = 4;           // Dato
        public const int PagoTarifa = 5;             // Acción
        public const int FichaTecnica = 6;           // Documento
        public const int CertificadoBpm = 7;         // Documento
        public const int Etiqueta = 8;               // Documento
    }

    public static class Reglas
    {
        // R1..R3 del ejemplo del PDF.
        public const int LicenciaFarmacia = 1;       // Farmacia → LIC-FARM
        public const int LicenciaDrogueria = 2;      // Droguería → LIC-DROG
        public const int RegistroMedicamento = 3;    // Medicamento → REG-MED
    }

    public static class AmbitosDocumento
    {
        public const int Producto = 1;
        public const int Establecimiento = 2;
    }

    public static class TiposRequisito {
        public const int Documento = 1;
        public const int Dato = 2;
        public const int Accion = 3;
    }
}
