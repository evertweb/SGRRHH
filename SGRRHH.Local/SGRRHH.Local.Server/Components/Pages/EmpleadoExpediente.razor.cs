using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

using SGRRHH.Local.Domain.DTOs;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Domain.Enums;
using SGRRHH.Local.Domain.Services;
using SGRRHH.Local.Infrastructure.Services;
using SGRRHH.Local.Shared.Helpers;
using SGRRHH.Local.Shared.Interfaces;
using SGRRHH.Local.Server.Components.Expediente;
using SGRRHH.Local.Server.Components.Tabs;

namespace SGRRHH.Local.Server.Components.Pages;

public partial class EmpleadoExpediente
{
    [Parameter] public int EmpleadoId { get; set; }

    [SupplyParameterFromQuery(Name = "tab")]
    public string? TabQuery { get; set; }

    private MessageToast? messageToast;
    private Empleado? empleado;
    private List<DocumentoEmpleado> documentos = new();
    private List<Contrato> contratos = new();
    private List<CuentaBancaria> cuentasBancarias = new();
    private List<Cargo> cargos = new();
    private List<Departamento> departamentos = new();

    private bool isLoading = true;
    private string activeTab = "datos";
    private static readonly HashSet<string> _tabsValidos = new() { "datos", "documentos", "contratos", "seguridad", "bancaria", "dotacion" };

    // Previsualización
    private bool showPreviewModal = false;
    private DocumentoEmpleado? previewDocumento;
    private byte[]? previewBytes;

    // Confirmación de eliminación
    private bool showConfirmDelete = false;
    private bool showCambiarFotoModal = false;

    // Estado del Scanner
    private bool mostrarScannerModal = false;
    private TipoDocumentoEmpleado tipoDocumentoParaEscanear = TipoDocumentoEmpleado.Otro;

    // Referencia al componente de Información Bancaria
    private InformacionBancariaTab? informacionBancariaTab;

    // Referencia al componente de Dotación
    private DotacionEppTab? dotacionEppTab;
    private int? entregaIdParaActa = null;

    // Estado del Printer
    private bool mostrarPrinterModal = false;
    private byte[]? bytesParaImprimir = null;
    private string? nombreDocumentoImprimir = null;

    // Modal de subida de documento
    private bool mostrarUploadModal = false;
    private TipoDocumentoEmpleado tipoDocumentoParaSubir = TipoDocumentoEmpleado.Otro;
    private string uploadNombre = "";
    private string uploadDescripcion = "";
    private DateTime? uploadFechaEmision;
    private DateTime? uploadFechaVencimiento;
    private byte[]? uploadFileBytes = null;
    private string uploadFileName = "";
    private string uploadFileMime = "";
    private bool isUploading = false;

    // Confirmación de eliminación de documento
    private bool showConfirmDeleteDoc = false;
    private DocumentoEmpleado? documentoAEliminar = null;

    // Cambio de estado
    private List<EstadoEmpleado> transicionesPermitidas = new();
    private string nuevoEstadoSeleccionado = "";
    private bool isSavingEstado = false;

    private List<TabsNavigation.TabDefinition> TabsDisponibles => new()
    {
        new() { Id = "datos", Label = "DATOS PERSONALES" },
        new() { Id = "documentos", Label = "DOCUMENTOS", Contador = documentos.Count, MostrarConteo = true },
        new() { Id = "contratos", Label = "CONTRATOS", Contador = contratos.Count, MostrarConteo = true },
        new() { Id = "seguridad", Label = "SEGURIDAD SOCIAL" },
        new() { Id = "bancaria", Label = "INFORMACIÓN BANCARIA", Contador = cuentasBancarias.Count, MostrarConteo = true },
        new() { Id = "dotacion", Label = "DOTACIÓN Y EPP" }
    };

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await CargarDatos();
    }

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrEmpty(TabQuery) && _tabsValidos.Contains(TabQuery))
        {
            activeTab = TabQuery;
        }
    }

    private void SetActiveTab(string tab)
    {
        if (!_tabsValidos.Contains(tab)) return;

        activeTab = tab;
        var uri = Navigation.GetUriWithQueryParameter("tab", tab);
        Navigation.NavigateTo(uri, replace: true);
    }

    private async Task CargarDatos()
    {
        isLoading = true;
        StateHasChanged();

        try
        {
            cargos = await CatalogCache.GetCargosAsync();
            departamentos = await CatalogCache.GetDepartamentosAsync();

            empleado = await EmpleadoRepo.GetByIdWithRelationsAsync(EmpleadoId);

            if (empleado != null)
            {
                documentos = (await DocumentoRepo.GetByEmpleadoIdAsync(EmpleadoId))
                    .OrderByDescending(d => d.FechaCreacion)
                    .ToList();

                contratos = (await ContratoRepo.GetByEmpleadoIdAsync(EmpleadoId))
                    .OrderByDescending(c => c.FechaInicio)
                    .ToList();

                cuentasBancarias = (await CuentaBancariaRepo.GetByEmpleadoIdAsync(EmpleadoId))
                    .OrderByDescending(c => c.EsCuentaNomina)
                    .ThenByDescending(c => c.FechaCreacion)
                    .ToList();

                foreach (var contrato in contratos)
                {
                    contrato.Cargo = cargos.FirstOrDefault(c => c.Id == contrato.CargoId);
                }

                var rolUsuario = AuthService.CurrentUser?.Rol ?? RolUsuario.Operador;
                transicionesPermitidas = EstadoEmpleadoService.ObtenerTransicionesPermitidas(empleado.Estado, rolUsuario).ToList();
                nuevoEstadoSeleccionado = "";

                Logger.LogInformation("Expediente cargado: {Codigo} con {Docs} documentos y {Contratos} contratos",
                    empleado.Codigo, documentos.Count, contratos.Count);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error cargando expediente del empleado {Id}", EmpleadoId);
            messageToast?.ShowError("Error al cargar expediente: " + ex.Message);
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    private void Volver()
    {
        Navigation.NavigateTo("/empleados");
    }

    private void EditarEmpleado()
    {
        Navigation.NavigateTo($"/empleados/{EmpleadoId}/editar");
    }

    private async Task CambiarEstado()
    {
        if (empleado == null || string.IsNullOrEmpty(nuevoEstadoSeleccionado))
            return;

        try
        {
            isSavingEstado = true;
            StateHasChanged();

            var nuevoEstado = (EstadoEmpleado)int.Parse(nuevoEstadoSeleccionado);
            var rolUsuario = AuthService.CurrentUser?.Rol ?? RolUsuario.Operador;

            var empleadoActual = await EmpleadoRepo.GetByIdWithRelationsAsync(empleado.Id);
            if (empleadoActual == null)
            {
                messageToast?.ShowError("El empleado no existe");
                Navigation.NavigateTo("/empleados");
                return;
            }

            var estadoAnterior = empleadoActual.Estado;

            if (estadoAnterior != empleado.Estado)
            {
                messageToast?.ShowError($"El estado del empleado cambió. Estado actual: {EstadoEmpleadoService.ObtenerDescripcion(estadoAnterior)}. Recargando...");
                empleado = empleadoActual;
                transicionesPermitidas = EstadoEmpleadoService.ObtenerTransicionesPermitidas(estadoAnterior, rolUsuario).ToList();
                nuevoEstadoSeleccionado = "";
                return;
            }

            if (!EstadoEmpleadoService.TienePermisoParaTransicion(rolUsuario, estadoAnterior, nuevoEstado))
            {
                messageToast?.ShowError("No tiene permisos para realizar este cambio de estado");
                return;
            }

            if (!EstadoEmpleadoService.EsTransicionValida(estadoAnterior, nuevoEstado))
            {
                messageToast?.ShowError($"No se puede cambiar de {estadoAnterior} a {nuevoEstado}");
                return;
            }

            empleadoActual.Estado = nuevoEstado;

            if (estadoAnterior == EstadoEmpleado.PendienteAprobacion && nuevoEstado == EstadoEmpleado.Activo)
            {
                empleadoActual.AprobadoPorId = AuthService.CurrentUser?.Id;
                empleadoActual.FechaAprobacion = DateTime.Now;
            }

            await EmpleadoRepo.UpdateAsync(empleadoActual);

            Logger.LogInformation("Estado cambiado: {Codigo} de {Anterior} a {Nuevo} por {Usuario}",
                empleadoActual.Codigo, estadoAnterior, nuevoEstado, AuthService.CurrentUser?.Username);

            messageToast?.ShowSuccess($"Estado cambiado a {EstadoEmpleadoService.ObtenerDescripcion(nuevoEstado)}");

            empleado = empleadoActual;
            transicionesPermitidas = EstadoEmpleadoService.ObtenerTransicionesPermitidas(nuevoEstado, rolUsuario).ToList();
            nuevoEstadoSeleccionado = "";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error cambiando estado del empleado {Id}", empleado?.Id);
            messageToast?.ShowError("Error al cambiar estado: " + ex.Message);
        }
        finally
        {
            isSavingEstado = false;
            StateHasChanged();
        }
    }

    private void ConfirmarEliminar()
    {
        showConfirmDelete = true;
    }

    private async Task EliminarEmpleado()
    {
        if (empleado == null) return;

        try
        {
            foreach (var doc in documentos)
            {
                if (!string.IsNullOrEmpty(doc.ArchivoPath))
                {
                    await StorageService.DeleteDocumentoEmpleadoAsync(doc.Id);
                }
                await DocumentoRepo.DeleteAsync(doc.Id);
            }

            if (!string.IsNullOrEmpty(empleado.FotoPath))
            {
                await StorageService.DeleteEmpleadoFotoAsync(empleado.Id);
            }

            await EmpleadoRepo.DeleteAsync(empleado.Id);

            Logger.LogInformation("Empleado eliminado: {Codigo} por usuario {User}",
                empleado.Codigo, AuthService.CurrentUser?.Username);

            messageToast?.ShowSuccess($"Empleado {empleado.Codigo} eliminado correctamente");
            await Task.Delay(1500);
            Navigation.NavigateTo("/empleados");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error eliminando empleado {Id}", EmpleadoId);
            messageToast?.ShowError("Error al eliminar: " + ex.Message);
            showConfirmDelete = false;
        }
    }

    private async Task PrevisualizarDocumento(DocumentoEmpleado documento)
    {
        try
        {
            previewDocumento = documento;
            previewBytes = null;
            showPreviewModal = true;
            StateHasChanged();

            var result = await StorageService.GetFileAsync(documento.ArchivoPath);
            if (result != null && result.Length > 0)
            {
                previewBytes = result;
            }
            else
            {
                messageToast?.ShowError("No se pudo cargar el archivo");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error previsualizando documento {Id}", documento.Id);
            messageToast?.ShowError("Error al cargar documento: " + ex.Message);
        }
        finally
        {
            StateHasChanged();
        }
    }

    private void CerrarPreview()
    {
        showPreviewModal = false;
        previewDocumento = null;
        previewBytes = null;
    }

    private async Task DescargarDocumento(DocumentoEmpleado documento)
    {
        try
        {
            var bytes = await StorageService.GetFileAsync(documento.ArchivoPath);
            if (bytes != null && bytes.Length > 0)
            {
                await JS.InvokeVoidAsync("downloadFile", documento.NombreArchivoOriginal, documento.TipoMime, bytes);
                Logger.LogInformation("Documento descargado: {Nombre}", documento.Nombre);
            }
            else
            {
                messageToast?.ShowError("No se pudo descargar el archivo");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error descargando documento {Id}", documento.Id);
            messageToast?.ShowError("Error al descargar: " + ex.Message);
        }
    }

    private void AbrirCambiarFoto()
    {
        showCambiarFotoModal = true;
    }

    private void AbrirScannerParaFoto()
    {
        tipoDocumentoParaEscanear = TipoDocumentoEmpleado.Foto;
        mostrarScannerModal = true;
    }

    private async Task OnFotoActualizada()
    {
        if (empleado == null) return;

        try
        {
            var empleadoActualizado = await EmpleadoRepo.GetByIdWithRelationsAsync(EmpleadoId);
            if (empleadoActualizado != null)
            {
                empleado = empleadoActualizado;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error refrescando foto del empleado {Id}", EmpleadoId);
        }
        finally
        {
            StateHasChanged();
        }
    }

    private int? cuentaIdParaCertificado = null;

    private void AbrirScannerParaCertificadoBancario(int cuentaId)
    {
        cuentaIdParaCertificado = cuentaId;
        tipoDocumentoParaEscanear = TipoDocumentoEmpleado.CertificadoBancario;
        mostrarScannerModal = true;
    }

    private void AbrirScannerParaTipo(TipoDocumentoEmpleado tipo)
    {
        tipoDocumentoParaEscanear = tipo;
        mostrarScannerModal = true;
    }

    private async Task OnScanComplete(List<ScannedPageDto> pages)
    {
        if (empleado == null || pages.Count == 0)
        {
            Logger.LogWarning("OnScanComplete: No hay empleado o páginas escaneadas");
            return;
        }

        try
        {
            if (tipoDocumentoParaEscanear == TipoDocumentoEmpleado.Foto && pages.Count > 0)
            {
                var page = pages[0];
                var extension = page.MimeType.Contains("png") ? ".png" : ".jpg";
                var fileName = $"foto_{DateTime.Now:yyyyMMdd_HHmmss}{extension}";

                if (!string.IsNullOrEmpty(empleado.FotoPath))
                {
                    await StorageService.DeleteEmpleadoFotoAsync(empleado.Id);
                }

                var storageResult = await StorageService.SaveEmpleadoFotoAsync(
                    empleado.Id,
                    page.ImageBytes,
                    fileName);

                if (storageResult.IsSuccess)
                {
                    empleado.FotoPath = storageResult.Value;
                    await EmpleadoRepo.UpdateAsync(empleado);
                    messageToast?.ShowSuccess("✓ Foto actualizada exitosamente");
                    Logger.LogInformation("Foto escaneada y guardada para empleado {Codigo}: {Path}", empleado.Codigo, storageResult.Value);
                }
                else
                {
                    messageToast?.ShowError("Error al guardar la foto");
                    Logger.LogError("Error guardando foto: {Error}", storageResult.Error);
                }

                mostrarScannerModal = false;
                showCambiarFotoModal = false;
                tipoDocumentoParaEscanear = TipoDocumentoEmpleado.Otro;
                StateHasChanged();
                return;
            }

            var tipoNombre = DocumentHelper.GetTipoDocumentoNombre(tipoDocumentoParaEscanear);
            Logger.LogInformation("Guardando {Paginas} página(s) escaneada(s) tipo {Tipo} para empleado {Codigo}",
                pages.Count, tipoNombre, empleado.Codigo);

            foreach (var page in pages)
            {
                var extension = page.MimeType.Contains("png") ? ".png" : ".jpg";
                var fileName = $"{tipoNombre}_{DateTime.Now:yyyyMMdd_HHmmss}_{page.PageNumber}{extension}";

                var documento = await DocumentoStorageService.GuardarDocumentoAsync(
                    empleado.Id,
                    page.ImageBytes,
                    fileName,
                    tipoDocumentoParaEscanear,
                    descripcion: pages.Count > 1 ? $"{tipoNombre} (Pág. {page.PageNumber})" : null);

                if (documento != null)
                {
                    documentos.Insert(0, documento);

                    Logger.LogInformation("Imagen escaneada guardada: {Path}", documento.ArchivoPath);

                    if (tipoDocumentoParaEscanear == TipoDocumentoEmpleado.CertificadoBancario && cuentaIdParaCertificado.HasValue && page.PageNumber == 1)
                    {
                        await DocumentoStorageService.VincularCertificadoBancarioAsync(documento.Id, cuentaIdParaCertificado.Value);
                        if (informacionBancariaTab != null)
                        {
                            await informacionBancariaTab.AsociarCertificadoACuenta(cuentaIdParaCertificado.Value, documento);
                        }
                    }

                    if (tipoDocumentoParaEscanear == TipoDocumentoEmpleado.ActaEntregaDotacion && entregaIdParaActa.HasValue && page.PageNumber == 1)
                    {
                        await DocumentoStorageService.VincularActaEntregaAsync(documento.Id, entregaIdParaActa.Value);
                        if (dotacionEppTab != null)
                        {
                            await dotacionEppTab.AsociarActaAEntrega(entregaIdParaActa.Value, documento);
                        }
                    }
                }
                else
                {
                    Logger.LogError("Error guardando imagen escaneada");
                }
            }

            messageToast?.ShowSuccess($"✓ {tipoNombre} escaneado y guardado exitosamente");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al guardar imágenes escaneadas");
            messageToast?.ShowError("Error al guardar imágenes escaneadas");
        }
        finally
        {
            mostrarScannerModal = false;
            cuentaIdParaCertificado = null;
            tipoDocumentoParaEscanear = TipoDocumentoEmpleado.Otro;
            StateHasChanged();
        }
    }

    private async Task OnPdfGenerated(byte[] pdfBytes)
    {
        if (empleado == null) return;

        try
        {
            var tipoNombre = DocumentHelper.GetTipoDocumentoNombre(tipoDocumentoParaEscanear);
            var fileName = $"{tipoNombre}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            var documento = await DocumentoStorageService.GuardarDocumentoAsync(
                empleado.Id,
                pdfBytes,
                fileName,
                tipoDocumentoParaEscanear);

            if (documento != null)
            {
                documentos.Insert(0, documento);

                if (tipoDocumentoParaEscanear == TipoDocumentoEmpleado.CertificadoBancario && cuentaIdParaCertificado.HasValue)
                {
                    await DocumentoStorageService.VincularCertificadoBancarioAsync(documento.Id, cuentaIdParaCertificado.Value);
                    if (informacionBancariaTab != null)
                    {
                        await informacionBancariaTab.AsociarCertificadoACuenta(cuentaIdParaCertificado.Value, documento);
                    }
                }

                if (tipoDocumentoParaEscanear == TipoDocumentoEmpleado.ActaEntregaDotacion && entregaIdParaActa.HasValue)
                {
                    await DocumentoStorageService.VincularActaEntregaAsync(documento.Id, entregaIdParaActa.Value);
                    if (dotacionEppTab != null)
                    {
                        await dotacionEppTab.AsociarActaAEntrega(entregaIdParaActa.Value, documento);
                    }
                }

                messageToast?.ShowSuccess($"✓ {tipoNombre} escaneado y guardado exitosamente");
                Logger.LogInformation("Documento escaneado guardado: {Path}", documento.ArchivoPath);
            }
            else
            {
                messageToast?.ShowError("Error al guardar documento escaneado");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al guardar documento escaneado");
            messageToast?.ShowError("Error al guardar documento escaneado");
        }

        mostrarScannerModal = false;
        cuentaIdParaCertificado = null;
        StateHasChanged();
    }

    private async Task ImprimirDocumento(DocumentoEmpleado documento)
    {
        try
        {
            var bytes = await StorageService.GetFileAsync(documento.ArchivoPath);
            if (bytes != null && bytes.Length > 0)
            {
                bytesParaImprimir = bytes;
                nombreDocumentoImprimir = documento.NombreArchivoOriginal;
                mostrarPrinterModal = true;
            }
            else
            {
                messageToast?.ShowError("No se pudo cargar el archivo para imprimir");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al cargar documento para imprimir");
            messageToast?.ShowError("Error al cargar documento");
        }
    }

    private void OnPrintComplete(PrintJobResultDto result)
    {
        if (result.Success)
        {
            messageToast?.ShowSuccess($"Documento enviado a imprimir: {result.PrinterName}");
        }
        else
        {
            messageToast?.ShowError($"Error al imprimir: {result.ErrorMessage}");
        }

        mostrarPrinterModal = false;
        bytesParaImprimir = null;
        nombreDocumentoImprimir = null;
    }

    private void CerrarPrinterModal()
    {
        mostrarPrinterModal = false;
        bytesParaImprimir = null;
        nombreDocumentoImprimir = null;
    }

    private void AbrirUploadModalParaTipo(TipoDocumentoEmpleado tipo)
    {
        tipoDocumentoParaSubir = tipo;
        uploadNombre = DocumentHelper.GetTipoDocumentoNombre(tipo);
        uploadDescripcion = "";
        uploadFechaEmision = DateTime.Today;
        uploadFechaVencimiento = null;
        uploadFileBytes = null;
        uploadFileName = "";
        uploadFileMime = "";
        mostrarUploadModal = true;
    }

    private void CerrarUploadModal()
    {
        mostrarUploadModal = false;
        uploadFileBytes = null;
        uploadFileName = "";
    }

    private async Task OnUploadFileSelected(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file != null)
        {
            try
            {
                if (file.Size > 10 * 1024 * 1024)
                {
                    messageToast?.ShowError("El archivo excede el tamaño máximo de 10MB");
                    return;
                }

                uploadFileName = file.Name;
                uploadFileMime = file.ContentType;

                using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                uploadFileBytes = ms.ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error seleccionando archivo");
                messageToast?.ShowError("Error al seleccionar archivo: " + ex.Message);
                uploadFileName = "";
                uploadFileBytes = null;
            }
        }
    }

    private async Task SubirDocumento()
    {
        if (empleado == null || uploadFileBytes == null) return;

        if (string.IsNullOrWhiteSpace(uploadNombre))
        {
            messageToast?.ShowError("El nombre del documento es obligatorio");
            return;
        }

        isUploading = true;
        StateHasChanged();

        try
        {
            var tipoNombre = DocumentHelper.GetTipoDocumentoNombre(tipoDocumentoParaSubir);

            var documento = await DocumentoStorageService.GuardarDocumentoAsync(
                empleado.Id,
                uploadFileBytes,
                uploadFileName,
                tipoDocumentoParaSubir,
                descripcion: uploadDescripcion,
                fechaEmision: uploadFechaEmision,
                fechaVencimiento: uploadFechaVencimiento);

            if (documento != null)
            {
                documentos.Insert(0, documento);
                messageToast?.ShowSuccess($"✓ {tipoNombre} guardado exitosamente");
                Logger.LogInformation("Documento subido: {Nombre} para empleado {EmpleadoId}", uploadNombre, empleado.Id);
                CerrarUploadModal();
            }
            else
            {
                messageToast?.ShowError("Error guardando archivo");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error subiendo documento");
            messageToast?.ShowError("Error al subir documento: " + ex.Message);
        }
        finally
        {
            isUploading = false;
            StateHasChanged();
        }
    }

    private void ConfirmarEliminarDocumento(DocumentoEmpleado documento)
    {
        documentoAEliminar = documento;
        showConfirmDeleteDoc = true;
    }

    private void CancelarEliminarDocumento()
    {
        documentoAEliminar = null;
        showConfirmDeleteDoc = false;
    }

    private async Task EliminarDocumento()
    {
        if (documentoAEliminar == null) return;

        try
        {
            var eliminado = await DocumentoStorageService.EliminarDocumentoAsync(documentoAEliminar.Id);

            if (eliminado)
            {
                documentos.RemoveAll(d => d.Id == documentoAEliminar.Id);

                Logger.LogInformation("Documento eliminado: {Nombre} (ID: {Id})", documentoAEliminar.Nombre, documentoAEliminar.Id);
                messageToast?.ShowSuccess($"Documento {DocumentHelper.GetTipoDocumentoNombre(documentoAEliminar.TipoDocumento)} eliminado");
            }
            else
            {
                messageToast?.ShowError("Error al eliminar documento");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error eliminando documento {Id}", documentoAEliminar.Id);
            messageToast?.ShowError("Error al eliminar documento: " + ex.Message);
        }
        finally
        {
            showConfirmDeleteDoc = false;
            documentoAEliminar = null;
            StateHasChanged();
        }
    }

    private void AbrirScannerParaActaEntrega(int entregaId)
    {
        tipoDocumentoParaEscanear = TipoDocumentoEmpleado.ActaEntregaDotacion;
        entregaIdParaActa = entregaId;
        mostrarScannerModal = true;
    }
}
