# PROMPT: Refactorización de ScannerModal - Extracción de Lógica a Servicios

## 📋 Contexto

El componente `ScannerModal.razor` tiene **1,394 líneas** y es el único componente de la refactorización original que NO alcanzó la reducción esperada. Ya tiene 7 subcomponentes UI creados, pero toda la lógica de negocio (~1,100 líneas) sigue en el archivo `.razor`.

**Estado actual:**
- ✅ UI componentizada: `ScannerToolbar`, `ScannerPreview`, `ScannerDeviceSelector`, `ScannerProfileSelector`, `ScannerThumbnails`, `ImageEditorTools`, `OcrPanel`, `ImagePreviewPopup`
- ❌ Lógica de negocio: Todo el bloque `@code {}` (líneas 258-1395) sigue siendo monolítico
- Servicios existentes ya inyectados: `IScannerService`, `IImageTransformationService`, `IScanProfileRepository`, `IOcrService`

**Objetivo:** Reducir `ScannerModal.razor` a ~300 líneas extrayendo la lógica a servicios y/o code-behind.

---

## 🎯 Objetivos Específicos

1. **Crear `ScannerModalStateService`** - Servicio para manejar el estado del modal
2. **Crear `ScannerWorkflowService`** - Servicio para orquestar las operaciones de escaneo
3. **Extraer code-behind** - Mover propiedades binding y handlers a `.razor.cs`
4. **El archivo `.razor` debe quedar solo con el template HTML/Razor**

---

## 📁 Archivos a Crear

### 1. `IScannerModalStateService.cs`
**Ubicación:** `SGRRHH.Local.Shared/Interfaces/IScannerModalStateService.cs`

```csharp
namespace SGRRHH.Local.Shared.Interfaces;

public interface IScannerModalStateService
{
    // Estado de páginas escaneadas
    List<ScannedPageDto> ScannedPages { get; }
    int PreviewIndex { get; set; }
    int CurrentPage { get; set; }
    
    // Estado de dispositivos
    List<ScannerDeviceDto> Scanners { get; }
    string? SelectedDeviceId { get; set; }
    
    // Estado de perfiles
    List<ScanProfileDto> Profiles { get; }
    int? SelectedProfileId { get; set; }
    
    // Estado de opciones de escaneo
    ScanOptionsDto ScanOptions { get; }
    string OutputFormat { get; set; }
    string OcrLanguage { get; set; }
    
    // Estado de corrección de imagen
    ImageCorrectionDto ImageCorrection { get; }
    
    // Flags de estado
    bool IsScanning { get; set; }
    bool IsGeneratingPdf { get; set; }
    bool IsLoadingScanners { get; set; }
    
    // Mensajes
    string? ErrorMessage { get; set; }
    string? SuccessMessage { get; set; }
    
    // Eventos
    event EventHandler? StateChanged;
    
    // Métodos
    void AddPage(ScannedPageDto page);
    void RemovePage(int index);
    void MovePage(int fromIndex, int toIndex);
    void ClearPages();
    void ApplyCorrectionToPage(int index, byte[] correctedBytes);
    void Reset();
}
```

### 2. `ScannerModalStateService.cs`
**Ubicación:** `SGRRHH.Local.Infrastructure/Services/ScannerModalStateService.cs`

Implementar la interfaz con estado reactivo.

### 3. `IScannerWorkflowService.cs`
**Ubicación:** `SGRRHH.Local.Shared/Interfaces/IScannerWorkflowService.cs`

```csharp
namespace SGRRHH.Local.Shared.Interfaces;

public interface IScannerWorkflowService
{
    Task<Result<List<ScannerDeviceDto>>> RefreshScannersAsync();
    Task<Result<List<ScanProfileDto>>> RefreshProfilesAsync();
    Task<Result<ScannedPageDto>> ScanSinglePageAsync(string deviceId, ScanOptionsDto options);
    Task<Result<byte[]>> GeneratePdfAsync(List<ScannedPageDto> pages);
    Task<Result<byte[]>> GenerateOcrPdfAsync(List<ScannedPageDto> pages, string language);
    Task<Result<byte[]>> ApplyCorrectionsAsync(byte[] imageBytes, ImageCorrectionDto corrections);
    Task SaveProfileAsync(string name, ScanOptionsDto options, ImageCorrectionDto corrections);
    Task DeleteProfileAsync(int profileId);
}
```

### 4. `ScannerWorkflowService.cs`  
**Ubicación:** `SGRRHH.Local.Infrastructure/Services/ScannerWorkflowService.cs`

Orquestar llamadas a: `IScannerService`, `IImageTransformationService`, `IScanProfileRepository`, `IOcrService`

### 5. `ScannerModal.razor.cs` (Code-behind)
**Ubicación:** `SGRRHH.Local.Server/Components/Shared/ScannerModal.razor.cs`

Mover a este archivo:
- Propiedades `[Parameter]`
- Propiedades con getters/setters complejos (gammaSliderValue, sharpnessMode, selectedDpi, etc.)
- Event handlers (HandleKeyPress, OnBackdropClick, etc.)
- Métodos de navegación (SelectPage, DeletePage, MovePage, PreviousPage, NextPage, etc.)
- Métodos de zoom y fullscreen
- Métodos de selección de área

---

## 📝 Pasos de Implementación

### Fase 1: Crear Interfaces (10 min)
1. Crear `IScannerModalStateService.cs`
2. Crear `IScannerWorkflowService.cs`
3. Agregar los DTOs necesarios si faltan

### Fase 2: Implementar Servicios (25 min)
1. Implementar `ScannerModalStateService` con eventos de cambio de estado
2. Implementar `ScannerWorkflowService` delegando a servicios existentes
3. Registrar en DI (`Program.cs`)

### Fase 3: Extraer Code-behind (20 min)
1. Crear `ScannerModal.razor.cs` como partial class
2. Mover todas las propiedades y métodos
3. Inyectar los nuevos servicios
4. Mantener solo el template en `.razor`

### Fase 4: Refactorizar el Template (15 min)
1. Reemplazar referencias a propiedades locales por llamadas al servicio de estado
2. Reemplazar lógica inline por llamadas al workflow service
3. Verificar bindings de subcomponentes

### Fase 5: Compilar y Probar (10 min)
1. `dotnet build -v:m /bl:build.binlog 2>&1 | Tee-Object build.log`
2. Corregir errores de compilación
3. Probar funcionalidad básica de escaneo

---

## ⚠️ Consideraciones Importantes

1. **No romper los subcomponentes existentes** - Los 7 subcomponentes reciben parámetros desde ScannerModal; mantener las mismas firmas
2. **Preservar eventos** - `ScannerService.ScanProgress` debe seguir funcionando
3. **Preservar funcionalidad de perfiles** - Cargar/guardar/aplicar perfiles
4. **QuestPDF** - La generación de PDF usa QuestPDF, conservar esa lógica en el workflow
5. **Estado de selección de área** - La lógica de `StartAreaSelectionAsync`, `UpdateAreaSelectionAsync`, `EndAreaSelectionAsync` es compleja, mantener coordinación de estado

---

## 📊 Archivos de Referencia

- **Componente actual:** `SGRRHH.Local.Server/Components/Shared/ScannerModal.razor`
- **Subcomponentes:** `SGRRHH.Local.Server/Components/Scanner/*.razor`
- **Servicios existentes:**
  - `SGRRHH.Local.Shared/Interfaces/IScannerService.cs`
  - `SGRRHH.Local.Shared/Interfaces/IImageTransformationService.cs`
  - `SGRRHH.Local.Domain/Repositories/IScanProfileRepository.cs`
  - `SGRRHH.Local.Infrastructure/Services/OcrService.cs`

---

## ✅ Criterios de Aceptación

1. [ ] `ScannerModal.razor` reducido a ≤300 líneas (solo template)
2. [ ] `ScannerModal.razor.cs` contiene toda la lógica de UI
3. [ ] `ScannerModalStateService` maneja estado reactivo
4. [ ] `ScannerWorkflowService` orquestra operaciones
5. [ ] Compilación: 0 errores
6. [ ] Funcionalidad de escaneo preservada
7. [ ] Perfiles siguen funcionando
8. [ ] Generación de PDF (normal y OCR) funciona

---

## 📁 Estructura Final Esperada

```
SGRRHH.Local.Shared/Interfaces/
├── IScannerModalStateService.cs   [NUEVO]
├── IScannerWorkflowService.cs     [NUEVO]
└── IScannerService.cs             [existente]

SGRRHH.Local.Infrastructure/Services/
├── ScannerModalStateService.cs    [NUEVO]
├── ScannerWorkflowService.cs      [NUEVO]
└── ScannerService.cs              [existente]

SGRRHH.Local.Server/Components/Shared/
├── ScannerModal.razor             [MODIFICADO - solo template, ≤300 líneas]
├── ScannerModal.razor.cs          [NUEVO - code-behind]
└── ... otros componentes

SGRRHH.Local.Server/Components/Scanner/
├── ScannerToolbar.razor           [sin cambios]
├── ScannerPreview.razor           [sin cambios]
├── ScannerDeviceSelector.razor    [sin cambios]
├── ScannerProfileSelector.razor   [sin cambios]
├── ScannerThumbnails.razor        [sin cambios]
├── ImageEditorTools.razor         [sin cambios]
├── OcrPanel.razor                 [sin cambios]
└── ImagePreviewPopup.razor        [sin cambios]
```

---

*Prompt generado: 2026-01-16*
