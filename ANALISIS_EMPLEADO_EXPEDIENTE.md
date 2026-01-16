# Análisis - EmpleadoExpediente

## Mapa de secciones (UI)
- Header del expediente: foto, nombre, estado y acciones.
- Tabs de navegación: datos, documentos, contratos, seguridad social, bancaria, dotación.
- Contenido de tabs:
  - Datos personales (lectura).
  - Documentos (requeridos y otros, acciones).
  - Contratos (lista y navegación).
  - Seguridad social (lectura).
  - Información bancaria (gestión de cuentas).
  - Dotación y EPP (tallas, entregas, actas).
- Modales:
  - Previsualización de documento.
  - Cambio de foto (subir/eliminar/escaneo).
  - Confirmación eliminación empleado.
  - Scanner modal.
  - Printer modal.
  - Subida de documento.
  - Confirmación eliminación documento.

## Tabs y contenido
- datos: información personal, contacto y laboral (solo lectura).
- documentos: checklist de documentos requeridos, otros documentos, acciones.
- contratos: lista y acciones de contrato.
- seguridad: afiliaciones (EPS/AFP/ARL/Caja).
- bancaria: cuentas bancarias y certificado bancario.
- dotacion: tallas, entregas y acta.

## Dependencias (servicios y helpers)
- `IAuthService`
- `IEmpleadoRepository`
- `IDocumentoEmpleadoRepository`
- `IContratoRepository`
- `ICuentaBancariaRepository`
- `ICatalogCacheService`
- `ILocalStorageService`
- `IDocumentoStorageService`
- `IJSRuntime`
- `ILogger<EmpleadoExpediente>`
- Helpers: `DocumentHelper`, `FormatHelper`, `NumberFormatHelper`, `DateHelper`, `EstadoEmpleadoService`

## Estado principal (componente orquestador)
- Empleado y listas: `empleado`, `documentos`, `contratos`, `cuentasBancarias`, `cargos`, `departamentos`.
- Tabs: `activeTab`, `_tabsValidos`.
- Estado de carga y mensajes: `isLoading`, `messageToast`.
- Modales: `showPreviewModal`, `showCambiarFotoModal`, `showConfirmDelete`, `mostrarUploadModal`, `showConfirmDeleteDoc`.
- Scanner/printer: `mostrarScannerModal`, `tipoDocumentoParaEscanear`, `mostrarPrinterModal`, `bytesParaImprimir`, `nombreDocumentoImprimir`.
- Upload: `uploadNombre`, `uploadDescripcion`, `uploadFechaEmision`, `uploadFechaVencimiento`, `uploadFileBytes`, `uploadFileName`, `isUploading`.
- Estado del empleado: `transicionesPermitidas`, `nuevoEstadoSeleccionado`, `isSavingEstado`.
