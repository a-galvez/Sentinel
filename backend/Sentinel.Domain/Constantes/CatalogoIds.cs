
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
        public const int RegistroSanitarioProducto = 1;  // Producto
        public const int PermisoImportacionProducto = 2;  // Producto (reservado, no se siembra en 1A)
        public const int LicenciaSanitariaProducto = 3;  // Producto
        public const int LicenciaFuncionamientoEstablecimiento = 4;  // LicenciaSanitaria
        public const int RegistroPropietarioEstablecimiento = 5;  // Establecimiento
        public const int AvisoFuncionamientoEstablecimiento = 6;  // Establecimiento
    }

    public static class Requisitos
    {
        public const int PresentarRegistroSanitario = 1;  // Documento
        public const int PresentarPermisoImportacion = 2;  // Documento (reservado, no se siembra en 1A)
        public const int PresentarLicenciaSanitaria = 3;  // Documento
        public const int PresentarLicenciaFuncionamiento = 4;  // Documento
        public const int PresentarRegistroPropietario = 5;  // Documento
        public const int PresentarAvisoFuncionamiento = 6;  // Documento
    }
}
