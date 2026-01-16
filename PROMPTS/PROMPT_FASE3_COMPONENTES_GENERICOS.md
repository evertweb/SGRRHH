# PROMPT: Fase 3 - Componentes Genéricos y Servicios

## 📋 Contexto

Este prompt continúa la refactorización del componente `EmpleadoExpediente.razor`. 

**Prerrequisitos completados:**
- ✅ Fase 1: Helpers compartidos creados (`DocumentHelper`, `DateHelper`, `FormatHelper`, `ValidationHelper`)
- ✅ Fase 2: Tabs extraídos (`InformacionBancariaTab`, `DotacionEppTab`, `SeguridadSocialTab`, `ContratosTab`)

Ahora implementamos componentes genéricos reutilizables y servicios centralizados.

---

## 🎯 Objetivos

### 1. Crear Componente Genérico de Modal Reutilizable

**Archivo:** `SGRRHH.Local.Server/Components/Shared/Modal.razor`

```razor
@* Modal genérico reutilizable *@
<div class="modal-backdrop" style="display: @(IsVisible ? "flex" : "none")">
    <div class="modal-content" style="max-width: @MaxWidth;">
        <div class="modal-header">
            <h3>@Title</h3>
            <button class="close-btn" @onclick="OnClose">×</button>
        </div>
        <div class="modal-body">
            @ChildContent
        </div>
        @if (Footer != null)
        {
            <div class="modal-footer">
                @Footer
            </div>
        }
    </div>
</div>

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string MaxWidth { get; set; } = "600px";
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? Footer { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
}
```

---

### 2. Crear Componente de Confirmación de Eliminación

**Archivo:** `SGRRHH.Local.Server/Components/Shared/ConfirmDeleteDialog.razor`

```razor
@* Diálogo de confirmación de eliminación reutilizable *@
<Modal IsVisible="@IsVisible" Title="@Title" MaxWidth="400px" OnClose="@OnCancel">
    <p>@Message</p>
    @if (!string.IsNullOrEmpty(WarningMessage))
    {
        <p class="text-danger">@WarningMessage</p>
    }
    <Footer>
        <button class="btn btn-secondary" @onclick="OnCancel">Cancelar</button>
        <button class="btn btn-danger" @onclick="OnConfirm" disabled="@IsProcessing">
            @(IsProcessing ? "Procesando..." : ConfirmButtonText)
        </button>
    </Footer>
</Modal>

@code {
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public string Title { get; set; } = "Confirmar Eliminación";
    [Parameter] public string Message { get; set; } = "¿Está seguro?";
    [Parameter] public string WarningMessage { get; set; } = "Esta acción no se puede deshacer.";
    [Parameter] public string ConfirmButtonText { get; set; } = "Eliminar";
    [Parameter] public bool IsProcessing { get; set; }
    [Parameter] public EventCallback OnConfirm { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
}
```

---

### 3. Crear Servicio de Almacenamiento de Documentos Centralizado

**Archivo:** `SGRRHH.Local.Infrastructure/Services/DocumentoStorageService.cs`

Este servicio centraliza todas las operaciones de almacenamiento de documentos que actualmente se repiten en múltiples lugares.

```csharp
namespace SGRRHH.Local.Infrastructure.Services;

public interface IDocumentoStorageService
{
    /// <summary>
    /// Guarda un documento escaneado o subido y lo registra en la base de datos
    /// </summary>
    Task<DocumentoEmpleado?> GuardarDocumentoAsync(
        int empleadoId,
        byte[] contenido,
        string nombreArchivo,
        TipoDocumentoEmpleado tipo,
        string? descripcion = null,
        DateTime? fechaEmision = null,
        DateTime? fechaVencimiento = null);
    
    /// <summary>
    /// Vincula un documento existente a una cuenta bancaria como certificado
    /// </summary>
    Task<bool> VincularCertificadoBancarioAsync(int documentoId, int cuentaBancariaId);
    
    /// <summary>
    /// Vincula un documento existente a una entrega de dotación como acta
    /// </summary>
    Task<bool> VincularActaEntregaAsync(int documentoId, int entregaDotacionId);
    
    /// <summary>
    /// Elimina un documento (soft delete)
    /// </summary>
    Task<bool> EliminarDocumentoAsync(int documentoId);
}

public class DocumentoStorageService : IDocumentoStorageService
{
    private readonly IDocumentoEmpleadoRepository _documentoRepo;
    private readonly ICuentaBancariaRepository _cuentaRepo;
    private readonly IEntregaDotacionRepository _entregaRepo;
    private readonly IStorageService _storageService;
    private readonly ILogger<DocumentoStorageService> _logger;
    
    // Implementación de métodos...
}
```

---

### 4. Refactorizar para Usar Nuevos Componentes

#### En `InformacionBancariaTab.razor`:

**Antes:**
```razor
@if (showConfirmDeleteCuenta && cuentaAEliminar != null)
{
    <div class="modal-backdrop">
        <div class="modal-content" style="max-width: 400px;">
            <div class="modal-header">
                <h3>Eliminar Cuenta Bancaria</h3>
                <button class="close-btn" @onclick="() => showConfirmDeleteCuenta = false">×</button>
            </div>
            <div class="modal-body">
                <p>¿Está seguro de que desea eliminar la cuenta...</p>
                <p class="text-danger">Esta acción no se puede deshacer.</p>
            </div>
            <div class="modal-footer">
                <button class="btn btn-secondary" @onclick="() => showConfirmDeleteCuenta = false">Cancelar</button>
                <button class="btn btn-danger" @onclick="EliminarCuenta">Confirmo Eliminar Cuenta</button>
            </div>
        </div>
    </div>
}
```

**Después:**
```razor
<ConfirmDeleteDialog 
    IsVisible="@showConfirmDeleteCuenta"
    Title="Eliminar Cuenta Bancaria"
    Message="@($"¿Está seguro de eliminar la cuenta {cuentaAEliminar?.Banco} - {cuentaAEliminar?.NumeroCuenta}?")"
    OnConfirm="@EliminarCuenta"
    OnCancel="@(() => showConfirmDeleteCuenta = false)" />
```

---

## 📁 Archivos a Crear

| Archivo | Ubicación |
|---------|-----------|
| `Modal.razor` | `SGRRHH.Local.Server/Components/Shared/Modal.razor` |
| `ConfirmDeleteDialog.razor` | `SGRRHH.Local.Server/Components/Shared/ConfirmDeleteDialog.razor` |
| `DocumentoStorageService.cs` | `SGRRHH.Local.Infrastructure/Services/DocumentoStorageService.cs` |

---

## 📁 Archivos a Modificar

| Archivo | Cambios |
|---------|---------|
| `InformacionBancariaTab.razor` | Usar `ConfirmDeleteDialog` |
| `DotacionEppTab.razor` | Usar `ConfirmDeleteDialog` y `Modal` |
| `EmpleadoExpediente.razor` | Inyectar `IDocumentoStorageService`, usar componentes compartidos |
| `Program.cs` | Registrar `DocumentoStorageService` en DI |

---

## 📝 Registro en Dependency Injection

**En `Program.cs`:**
```csharp
// Servicios de dominio
builder.Services.AddScoped<IDocumentoStorageService, DocumentoStorageService>();
```

---

## ✅ Verificación

1. **Build exitoso**: `dotnet build -v:m /bl:build.binlog 2>&1 | Tee-Object build.log`
2. **Modales funcionando**: Probar eliminar cuenta bancaria, eliminar entrega
3. **Documentos guardándose**: Probar escanear y subir documentos
4. **Reducción de código**: Los modales deben ser más concisos

---

## 📝 Orden de Implementación

1. Crear carpeta `Shared/` en `Components/` si no existe
2. Crear `Modal.razor` base
3. Crear `ConfirmDeleteDialog.razor` que usa Modal
4. Crear interfaz `IDocumentoStorageService`
5. Implementar `DocumentoStorageService`
6. Registrar en `Program.cs`
7. Refactorizar `InformacionBancariaTab` para usar nuevos componentes
8. Refactorizar `DotacionEppTab` 
9. Refactorizar `EmpleadoExpediente.razor`
10. Build y verificar funcionalidad
11. Eliminar código duplicado antiguo

---

## ⚠️ Notas Importantes

- Los nuevos componentes deben ser **compatibles hacia atrás** (no romper funcionalidad existente)
- El `DocumentoStorageService` centraliza lógica dispersa en `OnScanComplete`, `OnPdfGenerated`, `SubirDocumento`
- Preservar exactamente el comportamiento actual, solo reorganizar código
- Agregar logs informativos en el nuevo servicio
