# 🔧 AGENTE 2: REFACTORIZACIÓN - ScannerModal.razor

## 📋 INFORMACIÓN DEL COMPONENTE

**Componente Objetivo:** `SGRRHH.Local\SGRRHH.Local.Server\Components\Shared\ScannerModal.razor`  
**Tamaño Actual:** 1,592 líneas (67 KB)  
**Complejidad:** ⚠️ MUY ALTA  
**Prioridad:** 🟠 ALTA

### Descripción
Modal complejo de escaneo de documentos con múltiples capacidades:
- Interfaz con dispositivos escáner físicos
- Captura de múltiples páginas
- Procesamiento de imágenes (rotar, voltear, recortar)
- OCR (Reconocimiento Óptico de Caracteres)
- Generación de PDFs
- Perfiles de escaneo personalizables
- Vista previa con zoom y navegación
- Herramientas de edición de imagen

### Archivos Exclusivos de Este Agente (NO TOCAR POR OTROS)
```
✅ ARCHIVOS PERMITIDOS PARA MODIFICAR/CREAR:
- SGRRHH.Local\SGRRHH.Local.Server\Components\Shared\ScannerModal.razor
- SGRRHH.Local\SGRRHH.Local.Server\Components\Scanner\ScannerPreview.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Scanner\ScannerToolbar.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Scanner\ScannerThumbnails.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Scanner\ScannerDeviceSelector.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Scanner\ScannerProfileSelector.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Scanner\ImageEditorTools.razor (NUEVO)
- SGRRHH.Local\SGRRHH.Local.Server\Components\Scanner\OcrPanel.razor (NUEVO)

❌ ARCHIVOS PROHIBIDOS (USADOS POR OTROS AGENTES):
- EmpleadoOnboarding.razor (Agente 1)
- EmpleadoExpediente.razor (Agente 3)
- Permisos.razor (Agente 4)
- ControlDiario.razor (Agente 5)
```

---

## 🎯 OBJETIVOS DE REFACTORIZACIÓN

### Metas Principales
1. ✅ Reducir `ScannerModal.razor` de **1,592 líneas → ~250 líneas** (componente orquestador)
2. ✅ Extraer **7 componentes especializados** para scanner
3. ✅ Separar lógica de procesamiento de imagen en servicio dedicado
4. ✅ Consolidar operaciones repetitivas de transformación de imagen
5. ✅ Mejorar performance con renderizado selectivo
6. ✅ Mantener 100% de funcionalidad de scanner
7. ✅ Compilación sin errores

### KPIs de Éxito
- **Reducción de líneas:** Mínimo 80% en archivo principal
- **Componentes creados:** 7 nuevos componentes + 1 servicio
- **Redundancias eliminadas:** Operaciones de imagen duplicadas
- **Tests de compilación:** 0 errores
- **Funcionalidad:** Scanner operativo al 100%

---

## 📊 FASE 1: INVESTIGACIÓN (3-4 horas)

### 1.1 Análisis Estructural

**Tareas:**
```bash
# 1. Mapear secciones funcionales del componente
- Identificar panel de vista previa (líneas ~20-100)
- Identificar barra de herramientas (líneas ~40-80)
- Identificar panel de thumbnails (líneas ~100-200)
- Identificar selector de dispositivo (líneas ~300-400)
- Identificar configuración de perfiles (líneas ~400-500)
- Identificar panel OCR (líneas ~500-600)
- Identificar lógica de procesamiento (líneas ~800-1400)
```

**Deliverable 1.1:** Crear archivo `ANALISIS_SCANNER_MODAL.md` con:
- Mapa detallado de secciones (líneas inicio-fin)
- Lista de métodos por funcionalidad
- Servicios inyectados y su uso
- Estados compartidos vs estados locales

### 1.2 Análisis de Dependencias Externas

**Investigar uso de:**
1. `IScannerService` - Interfaz con hardware
2. `IImageProcessingService` - Procesamiento de imágenes
3. `IOcrService` - OCR
4. `IScanProfileRepository` - Perfiles de escaneo
5. `QuestPDF` - Generación de PDFs

**Identificar:**
- ¿Qué métodos de cada servicio se usan?
- ¿Hay lógica que debería estar en los servicios pero está en el componente?
- ¿Se puede mover más lógica a servicios?

**Deliverable 1.2:** Sección en `ANALISIS_SCANNER_MODAL.md`:
- Tabla de dependencias con métodos usados
- Lógica que debe moverse a servicios
- Propuesta de nuevo servicio `ImageTransformationService`

### 1.3 Búsqueda de Redundancias

**Investigar:**
1. **Operaciones de rotación:** ¿Se repite código de rotación 90°, 180°?
2. **Transformaciones de imagen:** ¿Hay patrones comunes en voltear/recortar?
3. **Validaciones:** ¿Se valida múltiples veces el mismo estado?
4. **Conversión de formatos:** ¿Se convierte base64 ↔ bytes repetidamente?

**Tareas Específicas:**
```bash
# Buscar operaciones de rotación
grep -n "Rotate" ScannerModal.razor

# Buscar conversiones base64
grep -n "base64\|Convert.FromBase64\|Convert.ToBase64" ScannerModal.razor

# Buscar validaciones de páginas
grep -n "scannedPages.Count\|previewIndex" ScannerModal.razor
```

**Deliverable 1.3:** Sección "Redundancias" en `ANALISIS_SCANNER_MODAL.md`:
- Lista de código duplicado (con líneas)
- Operaciones que se pueden consolidar
- Propuesta de métodos helper

### 1.4 Revisión de Skills

**Leer obligatoriamente:**
```bash
.cursor/skills/blazor-component/SKILL.md
.cursor/skills/hospital-ui-style/SKILL.md
.cursor/skills/build-and-verify/SKILL.md
```

**Deliverable 1.4:** Checklist en `ANALISIS_SCANNER_MODAL.md`

---

## 🗺️ FASE 2: PLANEACIÓN (3-4 horas)

### 2.1 Diseño de Arquitectura de Componentes

**Árbol de componentes propuesto:**

```
ScannerModal.razor (Orquestador - ~250 líneas)
│
├─ Header (inline, simple)
│
├─ <ScannerPreview 
│     CurrentPage="@GetCurrentPage()"
│     Zoom="@previewZoom"
│     OnZoomIn="@ZoomIn"
│     OnZoomOut="@ZoomOut" />
│
├─ <ScannerToolbar 
│     HasPages="@(scannedPages.Count > 0)"
│     AllowMultiple="@AllowMultiplePages"
│     OnRotate="@RotatePage"
│     OnFlipHorizontal="@FlipHorizontalPage"
│     OnFlipVertical="@FlipVerticalPage"
│     OnAutoCrop="@AutoCropPage" />
│
├─ <ScannerThumbnails 
│     Pages="@scannedPages"
│     SelectedIndex="@previewIndex"
│     OnSelectPage="@SelectPage"
│     OnDeletePage="@DeletePage"
│     OnReorder="@ReorderPages" />
│
├─ <ScannerDeviceSelector 
│     Devices="@availableDevices"
│     SelectedDevice="@selectedDevice"
│     OnDeviceSelected="@OnDeviceSelected" />
│
├─ <ScannerProfileSelector 
│     Profiles="@profiles"
│     SelectedProfile="@selectedProfile"
│     OnProfileSelected="@OnProfileSelected"
│     OnSaveProfile="@SaveProfile" />
│
├─ <ImageEditorTools 
│     CurrentPage="@GetCurrentPage()"
│     OnBrightnessChange="@AdjustBrightness"
│     OnContrastChange="@AdjustContrast"
│     OnCrop="@CropImage" />
│
└─ <OcrPanel 
      CurrentPage="@GetCurrentPage()"
      OcrService="@ocrService"
      OnTextExtracted="@HandleOcrText" />
```

**Deliverable 2.1:** Archivo `PLAN_ARQUITECTURA_SCANNER.md` con:
- Diagrama completo
- Props/parámetros de cada componente
- Eventos entre componentes
- Estado compartido

### 2.2 Diseño del Servicio ImageTransformationService

**Propuesta:**
```csharp
// SGRRHH.Local/SGRRHH.Local.Infrastructure/Services/ImageTransformationService.cs
namespace SGRRHH.Local.Infrastructure.Services;

public interface IImageTransformationService
{
    Task<byte[]> RotateAsync(byte[] imageData, int degrees);
    Task<byte[]> FlipHorizontalAsync(byte[] imageData);
    Task<byte[]> FlipVerticalAsync(byte[] imageData);
    Task<byte[]> AutoCropAsync(byte[] imageData);
    Task<byte[]> AdjustBrightnessAsync(byte[] imageData, float brightness);
    Task<byte[]> AdjustContrastAsync(byte[] imageData, float contrast);
    Task<byte[]> CropAsync(byte[] imageData, Rectangle cropArea);
    Task<string> ToBase64Async(byte[] imageData);
    Task<byte[]> FromBase64Async(string base64Data);
}

public class ImageTransformationService : IImageTransformationService
{
    // Consolidar toda la lógica de transformación aquí
    // Reutilizar código de IImageProcessingService existente
}
```

**Deliverable 2.2:** Sección en `PLAN_ARQUITECTURA_SCANNER.md`:
- Interfaz completa del servicio
- Métodos a migrar desde componente
- Métodos a reutilizar de servicios existentes

### 2.3 Plan de Migración de Código

| Componente | Líneas Origen | Responsabilidad | Dependencias |
|------------|---------------|-----------------|--------------|
| ScannerPreview | 22-100 | Vista previa grande con zoom | IJSRuntime |
| ScannerToolbar | 24-80 | Botones de herramientas | Ninguna |
| ScannerThumbnails | 100-250 | Miniaturas de páginas | IJSRuntime (drag) |
| ScannerDeviceSelector | 300-400 | Selector de escáner | IScannerService |
| ScannerProfileSelector | 400-550 | Perfiles de escaneo | IScanProfileRepository |
| ImageEditorTools | 600-750 | Herramientas de edición | IImageTransformationService |
| OcrPanel | 550-650 | Panel OCR | IOcrService |

**Deliverable 2.3:** Tabla completa en `PLAN_ARQUITECTURA_SCANNER.md`

### 2.4 Plan de Consolidación

**Redundancias a eliminar:**

1. **Rotación de imágenes:**
   - ❌ ANTES: Código duplicado para 90°, 180°, 270°
   - ✅ DESPUÉS: Método único `RotateAsync(degrees)` en servicio

2. **Conversión base64:**
   - ❌ ANTES: `Convert.FromBase64/ToBase64` en múltiples lugares
   - ✅ DESPUÉS: Métodos en `ImageTransformationService`

3. **Validación de índice:**
   - ❌ ANTES: `if (previewIndex >= 0 && previewIndex < scannedPages.Count)` repetido
   - ✅ DESPUÉS: Método `IsValidPageIndex(int index)`

4. **Actualización de preview:**
   - ❌ ANTES: `StateHasChanged()` llamado muchas veces
   - ✅ DESPUÉS: Centralizar en método `RefreshPreview()`

**Deliverable 2.4:** Sección "Consolidaciones" en `PLAN_ARQUITECTURA_SCANNER.md`

### 2.5 Plan de Pruebas

**Checklist de pruebas:**
```markdown
- [ ] Compilación: 0 errores
- [ ] Scanner: Detecta dispositivos correctamente
- [ ] Scanner: Escanea una página
- [ ] Scanner: Escanea múltiples páginas
- [ ] Herramientas: Rotar 90° funciona
- [ ] Herramientas: Rotar 180° funciona
- [ ] Herramientas: Voltear horizontal funciona
- [ ] Herramientas: Voltear vertical funciona
- [ ] Herramientas: Auto-recortar funciona
- [ ] Editor: Ajuste de brillo funciona
- [ ] Editor: Ajuste de contraste funciona
- [ ] OCR: Extracción de texto funciona
- [ ] Thumbnails: Navegación entre páginas funciona
- [ ] Thumbnails: Eliminar página funciona
- [ ] Thumbnails: Reordenar páginas funciona
- [ ] Perfiles: Guardar perfil funciona
- [ ] Perfiles: Cargar perfil funciona
- [ ] Exportar: Generar PDF funciona
- [ ] UI: Zoom in/out funciona
- [ ] UI: Modal se cierra correctamente
```

**Deliverable 2.5:** Archivo `TEST_PLAN_SCANNER.md`

---

## ⚙️ FASE 3: EJECUCIÓN CONTROLADA (10-14 horas)

### 3.1 Preparación

```bash
# 1. Crear carpetas
mkdir -p SGRRHH.Local/SGRRHH.Local.Server/Components/Scanner

# 2. Backup
cp ScannerModal.razor ScannerModal.razor.BACKUP

# 3. Compilar ANTES
dotnet build SGRRHH.Local/SGRRHH.Local.Server/SGRRHH.Local.Server.csproj
```

### 3.2 Iteración 1: Crear Servicio de Transformación

**Paso 1: IImageTransformationService.cs**
```csharp
// SGRRHH.Local/SGRRHH.Local.Shared/Interfaces/IImageTransformationService.cs
namespace SGRRHH.Local.Shared.Interfaces;

public interface IImageTransformationService
{
    Task<byte[]> RotateAsync(byte[] imageData, int degrees);
    Task<byte[]> FlipHorizontalAsync(byte[] imageData);
    Task<byte[]> FlipVerticalAsync(byte[] imageData);
    Task<byte[]> AutoCropAsync(byte[] imageData, int threshold = 240);
    Task<byte[]> AdjustBrightnessAsync(byte[] imageData, float brightness);
    Task<byte[]> AdjustContrastAsync(byte[] imageData, float contrast);
    Task<byte[]> CropAsync(byte[] imageData, int x, int y, int width, int height);
    string ToBase64String(byte[] imageData, string mimeType = "image/png");
    byte[] FromBase64String(string base64Data);
}
```

**Paso 2: ImageTransformationService.cs**
```csharp
// SGRRHH.Local/SGRRHH.Local.Infrastructure/Services/ImageTransformationService.cs
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace SGRRHH.Local.Infrastructure.Services;

public class ImageTransformationService : IImageTransformationService
{
    public async Task<byte[]> RotateAsync(byte[] imageData, int degrees)
    {
        using var image = Image.Load<Rgba32>(imageData);
        
        // Normalizar grados a 0-360
        degrees = ((degrees % 360) + 360) % 360;
        
        image.Mutate(x =>
        {
            if (degrees == 90)
                x.Rotate(RotateMode.Rotate90);
            else if (degrees == 180)
                x.Rotate(RotateMode.Rotate180);
            else if (degrees == 270)
                x.Rotate(RotateMode.Rotate270);
            else if (degrees != 0)
                x.Rotate(degrees);
        });
        
        using var ms = new MemoryStream();
        await image.SaveAsPngAsync(ms);
        return ms.ToArray();
    }
    
    public async Task<byte[]> FlipHorizontalAsync(byte[] imageData)
    {
        using var image = Image.Load<Rgba32>(imageData);
        image.Mutate(x => x.Flip(FlipMode.Horizontal));
        
        using var ms = new MemoryStream();
        await image.SaveAsPngAsync(ms);
        return ms.ToArray();
    }
    
    public async Task<byte[]> FlipVerticalAsync(byte[] imageData)
    {
        using var image = Image.Load<Rgba32>(imageData);
        image.Mutate(x => x.Flip(FlipMode.Vertical));
        
        using var ms = new MemoryStream();
        await image.SaveAsPngAsync(ms);
        return ms.ToArray();
    }
    
    // ... implementar otros métodos
    
    public string ToBase64String(byte[] imageData, string mimeType = "image/png")
    {
        return $"data:{mimeType};base64,{Convert.ToBase64String(imageData)}";
    }
    
    public byte[] FromBase64String(string base64Data)
    {
        // Remover prefijo data:image/...;base64, si existe
        if (base64Data.Contains(","))
        {
            base64Data = base64Data.Split(',')[1];
        }
        return Convert.FromBase64String(base64Data);
    }
}
```

**Paso 3: Registrar servicio**
```csharp
// En Program.cs o Startup.cs
builder.Services.AddScoped<IImageTransformationService, ImageTransformationService>();
```

**✅ CHECKPOINT 1:** Compilar

### 3.3 Iteración 2: Componentes de UI

#### Paso 4: ScannerToolbar.razor
```razor
@* Barra de herramientas de scanner *@
<div class="scanner-preview-toolbar">
    <div class="scanner-toolbar-left">
        @if (AllowMultiple && TotalPages > 0)
        {
            <button class="scanner-tool-btn" @onclick="OnPreviousPage" disabled="@(CurrentPage <= 0)" title="Página anterior">◀</button>
            <span class="scanner-page-indicator">@(CurrentPage + 1) / @TotalPages</span>
            <button class="scanner-tool-btn" @onclick="OnNextPage" disabled="@(CurrentPage >= TotalPages - 1)" title="Página siguiente">▶</button>
            <div class="scanner-tool-separator"></div>
        }
        @if (HasPages)
        {
            <button class="scanner-tool-btn" @onclick="@(() => OnRotate.InvokeAsync(-90))" title="Rotar izquierda 90°">↺</button>
            <button class="scanner-tool-btn" @onclick="@(() => OnRotate.InvokeAsync(90))" title="Rotar derecha 90°">↻</button>
            <button class="scanner-tool-btn" @onclick="@(() => OnRotate.InvokeAsync(180))" title="Rotar 180°">⟲</button>
            <div class="scanner-tool-separator"></div>
            <button class="scanner-tool-btn" @onclick="OnFlipHorizontal" title="Voltear horizontal">⇆</button>
            <button class="scanner-tool-btn" @onclick="OnFlipVertical" title="Voltear vertical">⇅</button>
            <div class="scanner-tool-separator"></div>
            <button class="scanner-tool-btn" @onclick="OnAutoCrop" title="Auto-recortar bordes">⬚</button>
        }
    </div>
    <div class="scanner-toolbar-right">
        <button class="scanner-tool-btn" @onclick="OnZoomOut" title="Alejar" disabled="@(Zoom <= 25)">−</button>
        <span class="scanner-zoom-indicator">@(Zoom == 0 ? "Auto" : $"{Zoom}%")</span>
        <button class="scanner-tool-btn" @onclick="OnZoomIn" title="Acercar" disabled="@(Zoom >= 200)">+</button>
    </div>
</div>

@code {
    [Parameter] public bool HasPages { get; set; }
    [Parameter] public bool AllowMultiple { get; set; }
    [Parameter] public int CurrentPage { get; set; }
    [Parameter] public int TotalPages { get; set; }
    [Parameter] public int Zoom { get; set; }
    
    [Parameter] public EventCallback<int> OnRotate { get; set; }
    [Parameter] public EventCallback OnFlipHorizontal { get; set; }
    [Parameter] public EventCallback OnFlipVertical { get; set; }
    [Parameter] public EventCallback OnAutoCrop { get; set; }
    [Parameter] public EventCallback OnZoomIn { get; set; }
    [Parameter] public EventCallback OnZoomOut { get; set; }
    [Parameter] public EventCallback OnPreviousPage { get; set; }
    [Parameter] public EventCallback OnNextPage { get; set; }
}
```

**✅ CHECKPOINT 2:** Compilar

#### Paso 5: ScannerPreview.razor
```razor
@inject IJSRuntime JS

<div class="scanner-preview-container">
    @if (CurrentPage != null && !string.IsNullOrEmpty(CurrentPage.ImageDataUrl))
    {
        <div class="scanner-preview-image-wrapper" style="transform: scale(@GetZoomScale());">
            <img src="@CurrentPage.ImageDataUrl" alt="Vista previa" class="scanner-preview-image" />
        </div>
    }
    else
    {
        <div class="scanner-preview-placeholder">
            <div class="scanner-placeholder-icon">📄</div>
            <p>NO HAY PÁGINAS ESCANEADAS</p>
            <p class="scanner-placeholder-hint">Use el botón ESCANEAR para capturar documentos</p>
        </div>
    }
</div>

@code {
    [Parameter] public ScannedPageData? CurrentPage { get; set; }
    [Parameter] public int Zoom { get; set; }
    
    private double GetZoomScale()
    {
        return Zoom == 0 ? 1.0 : Zoom / 100.0;
    }
}
```

**✅ CHECKPOINT 3:** Compilar

#### Paso 6: ScannerThumbnails.razor
```razor
<div class="scanner-thumbnails-panel">
    <div class="scanner-thumbnails-header">
        <span>PÁGINAS (@Pages.Count)</span>
    </div>
    <div class="scanner-thumbnails-grid">
        @foreach (var (page, index) in Pages.Select((p, i) => (p, i)))
        {
            <div class="scanner-thumbnail @(index == SelectedIndex ? "selected" : "")" 
                 @onclick="() => OnSelectPage.InvokeAsync(index)">
                <div class="scanner-thumbnail-image">
                    <img src="@page.ImageDataUrl" alt="Página @(index + 1)" />
                </div>
                <div class="scanner-thumbnail-footer">
                    <span>@(index + 1)</span>
                    <button class="scanner-thumbnail-delete" 
                            @onclick="() => OnDeletePage.InvokeAsync(index)"
                            @onclick:stopPropagation="true">
                        🗑
                    </button>
                </div>
            </div>
        }
    </div>
</div>

@code {
    [Parameter] public List<ScannedPageData> Pages { get; set; } = new();
    [Parameter] public int SelectedIndex { get; set; }
    [Parameter] public EventCallback<int> OnSelectPage { get; set; }
    [Parameter] public EventCallback<int> OnDeletePage { get; set; }
}
```

**✅ CHECKPOINT 4:** Compilar

#### Pasos 7-9: Crear componentes restantes
- ScannerDeviceSelector.razor
- ScannerProfileSelector.razor
- ImageEditorTools.razor
- OcrPanel.razor

**✅ CHECKPOINTS 5-8:** Compilar después de cada uno

### 3.4 Iteración 3: Refactorizar ScannerModal.razor

**Reducir a orquestador (~250 líneas):**

```razor
@using SGRRHH.Local.Domain.DTOs
@using SGRRHH.Local.Shared.Interfaces
@inject IScannerService ScannerService
@inject IImageTransformationService ImageTransform
@inject IScanProfileRepository ProfileRepository
@inject IOcrService OcrService
@inject IJSRuntime JS

@if (IsVisible)
{
    <div class="scanner-backdrop" @onclick="OnBackdropClick" @onkeydown="HandleKeyPress" tabindex="-1">
        <div class="scanner-modal scanner-modal-fullscreen" @onclick:stopPropagation="true">
            <div class="scanner-header">
                <span>@(Titulo ?? "ESCÁNER DE DOCUMENTOS")</span>
                <button class="scanner-close" @onclick="Cerrar" disabled="@isScanning">✕</button>
            </div>
            
            <div class="scanner-body-horizontal">
                @* Panel izquierdo: Vista previa *@
                <div class="scanner-preview-panel">
                    <ScannerToolbar 
                        HasPages="@(scannedPages.Count > 0)"
                        AllowMultiple="@AllowMultiplePages"
                        CurrentPage="@previewIndex"
                        TotalPages="@scannedPages.Count"
                        Zoom="@previewZoom"
                        OnRotate="@RotatePage"
                        OnFlipHorizontal="@FlipHorizontalPage"
                        OnFlipVertical="@FlipVerticalPage"
                        OnAutoCrop="@AutoCropPage"
                        OnZoomIn="@ZoomIn"
                        OnZoomOut="@ZoomOut"
                        OnPreviousPage="@PreviousPage"
                        OnNextPage="@NextPage" />
                    
                    <ScannerPreview 
                        CurrentPage="@GetCurrentPage()"
                        Zoom="@previewZoom" />
                </div>
                
                @* Panel derecho: Controles *@
                <div class="scanner-controls-panel">
                    <ScannerDeviceSelector 
                        Devices="@availableDevices"
                        SelectedDevice="@selectedDevice"
                        OnDeviceSelected="@SelectDevice"
                        OnRefresh="@RefreshDevices" />
                    
                    <ScannerProfileSelector 
                        Profiles="@profiles"
                        SelectedProfile="@currentProfile"
                        OnProfileSelected="@LoadProfile"
                        OnSaveProfile="@SaveCurrentProfile" />
                    
                    @* Botones principales *@
                    <div class="scanner-main-actions">
                        <button class="scanner-btn scanner-btn-primary" 
                                @onclick="ScanPage" 
                                disabled="@(selectedDevice == null || isScanning)">
                            @(isScanning ? "ESCANEANDO..." : "ESCANEAR PÁGINA")
                        </button>
                        
                        @if (AllowMultiplePages && scannedPages.Count > 0)
                        {
                            <button class="scanner-btn scanner-btn-success" 
                                    @onclick="FinalizarEscaneoMultiple" 
                                    disabled="@isScanning">
                                FINALIZAR (@scannedPages.Count páginas)
                            </button>
                        }
                    </div>
                    
                    @* Herramientas avanzadas *@
                    @if (scannedPages.Count > 0)
                    {
                        <ImageEditorTools 
                            CurrentPage="@GetCurrentPage()"
                            OnBrightnessChange="@AdjustBrightness"
                            OnContrastChange="@AdjustContrast"
                            OnCrop="@CropImage" />
                        
                        <OcrPanel 
                            CurrentPage="@GetCurrentPage()"
                            OnTextExtracted="@HandleOcrText" />
                    }
                    
                    @* Thumbnails *@
                    <ScannerThumbnails 
                        Pages="@scannedPages"
                        SelectedIndex="@previewIndex"
                        OnSelectPage="@SelectPage"
                        OnDeletePage="@DeletePage" />
                </div>
            </div>
        </div>
    </div>
}

@code {
    // PARÁMETROS
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public string? Titulo { get; set; }
    [Parameter] public bool AllowMultiplePages { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public EventCallback<List<ScannedPageData>> OnComplete { get; set; }
    
    // ESTADO LOCAL
    private List<ScannedPageData> scannedPages = new();
    private List<ScannerDevice> availableDevices = new();
    private ScannerDevice? selectedDevice;
    private int previewIndex = 0;
    private int previewZoom = 0;
    private bool isScanning = false;
    private ScanProfile? currentProfile;
    private List<ScanProfile> profiles = new();
    
    // INICIALIZACIÓN
    protected override async Task OnInitializedAsync()
    {
        await RefreshDevices();
        await LoadProfiles();
    }
    
    // MÉTODOS PRINCIPALES (delegan a servicio ImageTransform)
    private async Task RotatePage(int degrees)
    {
        var page = GetCurrentPage();
        if (page == null) return;
        
        try
        {
            var imageBytes = ImageTransform.FromBase64String(page.ImageDataUrl);
            var rotatedBytes = await ImageTransform.RotateAsync(imageBytes, degrees);
            page.ImageDataUrl = ImageTransform.ToBase64String(rotatedBytes);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("alert", $"Error rotando imagen: {ex.Message}");
        }
    }
    
    private async Task FlipHorizontalPage()
    {
        var page = GetCurrentPage();
        if (page == null) return;
        
        try
        {
            var imageBytes = ImageTransform.FromBase64String(page.ImageDataUrl);
            var flippedBytes = await ImageTransform.FlipHorizontalAsync(imageBytes);
            page.ImageDataUrl = ImageTransform.ToBase64String(flippedBytes);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("alert", $"Error volteando imagen: {ex.Message}");
        }
    }
    
    private async Task FlipVerticalPage()
    {
        var page = GetCurrentPage();
        if (page == null) return;
        
        try
        {
            var imageBytes = ImageTransform.FromBase64String(page.ImageDataUrl);
            var flippedBytes = await ImageTransform.FlipVerticalAsync(imageBytes);
            page.ImageDataUrl = ImageTransform.ToBase64String(flippedBytes);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("alert", $"Error volteando imagen: {ex.Message}");
        }
    }
    
    private async Task AutoCropPage()
    {
        var page = GetCurrentPage();
        if (page == null) return;
        
        try
        {
            var imageBytes = ImageTransform.FromBase64String(page.ImageDataUrl);
            var croppedBytes = await ImageTransform.AutoCropAsync(imageBytes);
            page.ImageDataUrl = ImageTransform.ToBase64String(croppedBytes);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            await JS.InvokeVoidAsync("alert", $"Error recortando imagen: {ex.Message}");
        }
    }
    
    // HELPERS
    private ScannedPageData? GetCurrentPage()
    {
        return previewIndex >= 0 && previewIndex < scannedPages.Count 
            ? scannedPages[previewIndex] 
            : null;
    }
    
    private void ZoomIn()
    {
        if (previewZoom == 0) previewZoom = 100;
        previewZoom = Math.Min(previewZoom + 25, 200);
    }
    
    private void ZoomOut()
    {
        previewZoom = Math.Max(previewZoom - 25, 25);
        if (previewZoom == 25) previewZoom = 0;
    }
    
    // ... otros métodos simplificados
}
```

**✅ CHECKPOINT FINAL:**
```bash
dotnet build SGRRHH.Local/SGRRHH.Local.Server/SGRRHH.Local.Server.csproj
wc -l ScannerModal.razor  # Debe ser ~250 líneas
```

### 3.5 Pruebas de Funcionalidad

Ejecutar TODAS las pruebas del `TEST_PLAN_SCANNER.md`

**Documentar en:** `RESULTADO_PRUEBAS_SCANNER.md`

---

## 📝 FASE 4: DOCUMENTACIÓN Y ENTREGA (1 hora)

### 4.1 Archivos Entregables
1. **ANALISIS_SCANNER_MODAL.md**
2. **PLAN_ARQUITECTURA_SCANNER.md**
3. **TEST_PLAN_SCANNER.md**
4. **RESULTADO_PRUEBAS_SCANNER.md**
5. **REFACTOR_SUMMARY_SCANNER.md**

### 4.2 Contenido de REFACTOR_SUMMARY_SCANNER.md
```markdown
# Resumen de Refactorización: ScannerModal.razor

## Métricas Finales
- **Líneas ANTES:** 1,592
- **Líneas DESPUÉS:** ~250
- **Reducción:** 84%
- **Componentes creados:** 7
- **Servicios creados:** 1 (ImageTransformationService)

## Componentes Creados
1. ScannerToolbar.razor
2. ScannerPreview.razor
3. ScannerThumbnails.razor
4. ScannerDeviceSelector.razor
5. ScannerProfileSelector.razor
6. ImageEditorTools.razor
7. OcrPanel.razor

## Servicios Creados
1. ImageTransformationService (consolida operaciones de imagen)

## Redundancias Eliminadas
1. Código de rotación duplicado → método único RotateAsync
2. Conversiones base64 repetidas → métodos ToBase64/FromBase64
3. Validaciones de índice duplicadas → método IsValidPageIndex
4. Transformaciones de imagen dispersas → servicio centralizado

## Pruebas Realizadas
- ✅ Compilación: 0 errores
- ✅ Funcionalidad scanner: 100% operativo
- ✅ Transformaciones de imagen: Todas funcionan
- ✅ OCR: Operativo
- ✅ Perfiles: Guardado/carga funciona
```

---

## ⚠️ REGLAS CRÍTICAS

### ❌ NO HACER:
1. NO modificar archivos de otros agentes
2. NO cambiar interfaces de IScannerService
3. NO eliminar funcionalidad de scanner existente
4. NO hacer commit sin compilación exitosa

### ✅ HACER SIEMPRE:
1. Compilar después de cada componente
2. Mantener estilos CSS existentes
3. Probar con escáner físico si está disponible
4. Documentar todos los cambios

---

## ✅ CHECKLIST FINAL
```markdown
- [ ] Fase 1: Investigación completada
- [ ] Fase 2: Planeación completada
- [ ] ImageTransformationService creado y registrado ✅
- [ ] ScannerToolbar.razor creado ✅
- [ ] ScannerPreview.razor creado ✅
- [ ] ScannerThumbnails.razor creado ✅
- [ ] ScannerDeviceSelector.razor creado ✅
- [ ] ScannerProfileSelector.razor creado ✅
- [ ] ImageEditorTools.razor creado ✅
- [ ] OcrPanel.razor creado ✅
- [ ] ScannerModal.razor refactorizado ✅
- [ ] Todas las pruebas pasadas ✅
- [ ] Documentación completada ✅
- [ ] Build final: 0 errores ✅
```

---

**INICIO DE EJECUCIÓN:** [FECHA]  
**FIN ESPERADO:** [FECHA + 3-4 días]  
**AGENTE ASIGNADO:** [NOMBRE/ID]
