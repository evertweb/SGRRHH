Inventario de componentes (SGRRHH.Local.Server)
Ruteo y layout

App.razor
Routes.razor
MainLayout.razor
EmptyLayout.razor
NavMenu.razor
Pages

ImageViewer.razor
Páginas (Components/Pages)

Home.razor
Login.razor
LoggedOut.razor
Error.razor
Empleados.razor
EmpleadoEditar.razor
EmpleadoOnboarding.razor
EmpleadoExpediente.razor
Documentos.razor
Contratos.razor
Permisos.razor
SeguimientoPermisos.razor
ModalSeguimientoPermiso.razor
Vacaciones.razor
Incapacidades.razor
Nomina.razor
Prestaciones.razor
ControlDiario.razor
ControlDiarioWizard.razor
ProyectosTab.razor
ProyectoDetalle.razor
ReporteProyecto.razor
Reportes.razor
DashboardProductividad.razor
Catalogos.razor
DepartamentosTab.razor
CargosTab.razor
ActividadesTab.razor
TiposPermisoTab.razor
Configuracion.razor
ConfiguracionLegal.razor
Usuarios.razor
Auditoria.razor
Aspirantes.razor
Vacantes.razor
Festivos.razor
TablaPendientesDocumento.razor
TablaVencidos.razor
TablaParaDescuento.razor
TablaEnCompensacion.razor
Counter.razor
Weather.razor
Expediente

EmpleadoHeader.razor
EmpleadoInfoCard.razor
TabsNavigation.razor
DatosGeneralesTab.razor
DocumentosTab.razor
FotoChangeModal.razor
Tabs

ContratosTab.razor
SeguridadSocialTab.razor
InformacionBancariaTab.razor
DotacionEppTab.razor
Forms

DatosPersonalesForm.razor
DatosLaboralesForm.razor
ContactoEmpleadoForm.razor
DatosBancariosForm.razor
SeguridadSocialForm.razor
Permisos

PermisosHeader.razor
PermisosFilters.razor
PermisosTable.razor
PermisoFormModal.razor
PermisoAprobacionModal.razor
PermisoCalculadora.razor
PermisoSeguimientoPanel.razor
Control Diario

ControlDiarioHeader.razor
DateNavigator.razor
ActividadSelector.razor
FiltrosDiarios.razor
EmpleadoRow.razor
ResumenDiarioCard.razor
AccionesMasivasPanel.razor
RegistroAsistenciaModal.razor
Proyecto

ProyectoEncabezado.razor
ProyectoResumen.razor
ProyectoMetricas.razor
ProyectoActividades.razor
ProyectoCuadrilla.razor
ProyectoHistorial.razor
ProyectoMapa.razor
Scanner

ScannerToolbar.razor
ScannerDeviceSelector.razor
ScannerProfileSelector.razor
ScannerPreview.razor
ScannerThumbnails.razor
ImageEditorTools.razor
OcrPanel.razor
Reportes

ReportListadoEmpleados.razor
ReportPermisos.razor
ReportVacaciones.razor
ReportAsistencia.razor
ReportCertificadoLaboral.razor
Vacaciones (subcomponentes)

VacacionesSummary.razor
VacacionesList.razor
VacacionesForm.razor
VacacionesFilter.razor
Shared

Modal.razor
FormModal.razor
ConfirmDialog.razor
ConfirmDeleteDialog.razor
MessageToast.razor
NotificationBell.razor
KeyboardHandler.razor
UnsavedChangesGuard.razor
AuthPersistence.razor
AuthorizeViewLocal.razor
RedirectToLogin.razor
DataTable.razor
EstadoBadge.razor
EmpleadoCard.razor
EmpleadoSelector.razor
InputCedula.razor
InputUpperCase.razor
InputMoneda.razor
CalendarioMini.razor
ResumenVacacionesPanel.razor
SelectorVacante.razor
ModalContratacion.razor
ScannerModal.razor
PrinterModal.razor
ImagePreviewPopup.razor
WizardNavigation.razor
WizardProgress.razor
SatelliteSpinner.razor
Relaciones principales entre entidades (Dominio)
Entidad central

Empleado: Empleado.cs
Relaciones directas de Empleado

Empleado 1..* DocumentoEmpleado (documentos de expediente)
DocumentoEmpleado.cs
Empleado 1..* Contrato
Contrato.cs
Empleado 1..* CuentaBancaria
CuentaBancaria.cs
Empleado 1..* Vacacion
Vacacion.cs
Empleado 1..* Permiso
Permiso.cs
Empleado 1..* Incapacidad
Incapacidad.cs
Empleado 1..* Nomina
Nomina.cs
Empleado 1..* Prestacion
Prestacion.cs
Empleado 1..* RegistroDiario
RegistroDiario.cs
Empleado 1..* EntregaDotacion (actas de entrega)
EntregaDotacion.cs
Empleado 1..1? TallasEmpleado
TallasEmpleado.cs
Relaciones con estructura organizacional

Departamento 1..* Empleado
Departamento.cs
Cargo 1..* Empleado
Cargo.cs
Empleado ↔ Empleado (SupervisorId)
Empleado.cs
Proyectos

Proyecto 1..* ProyectoEmpleado y ProyectoEmpleado *..1 Empleado
Proyecto.cs
ProyectoEmpleado.cs
Permisos ↔ Incapacidades

Permiso puede generar Incapacidad (Permiso.IncapacidadId) y Incapacidad puede referenciar PermisoOrigenId
Permiso.cs
Incapacidad.cs
Usuarios

Usuario puede vincularse a Empleado (Usuario.EmpleadoId)
Usuario.cs
Relación entre Empleado y “Expediente”
El expediente es un agregado UI: EmpleadoExpediente.razor.
Se compone de tabs y secciones que consumen datos de:
Empleado (datos personales y seguridad social)
DocumentoEmpleado (documentos del expediente)
Contrato (historial contractual)
CuentaBancaria (información bancaria)
EntregaDotacion y TallasEmpleado (dotación/EPP)
Tabs usados en expediente:
DatosGeneralesTab.razor,
DocumentosTab.razor,
ContratosTab.razor,
SeguridadSocialTab.razor,
InformacionBancariaTab.razor,
DotacionEppTab.razor.
