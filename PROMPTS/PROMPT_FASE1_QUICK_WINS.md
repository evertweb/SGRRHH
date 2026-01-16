# PROMPT: Fase 1 - Quick Wins (Helpers y Centralización)

## 📋 Contexto

Este prompt continúa la refactorización del componente `EmpleadoExpediente.razor` según el análisis en `analisis_redundancias_expediente.md`. La Fase 2 (extracción de tabs) está ~80% completada. Ahora abordamos la Fase 1 que quedaba pendiente.

**Estado actual:**
- ✅ Tabs extraídos: `InformacionBancariaTab`, `DotacionEppTab`, `SeguridadSocialTab`, `ContratosTab`
- ⬜ Pendiente: `DatosPersonalesTab`, `DocumentosTab`
- ⬜ Helpers compartidos no existen todavía

---

## 🎯 Objetivos

### 1. Crear Helpers Compartidos

Crear clases estáticas en `SGRRHH.Local.Shared/Helpers/` para centralizar métodos utilitarios duplicados:

#### A) `DocumentHelper.cs`
```csharp
namespace SGRRHH.Local.Shared.Helpers;

public static class DocumentHelper
{
    /// <summary>
    /// Obtiene el nombre legible de un tipo de documento
    /// Extraído de EmpleadoExpediente.razor líneas 910-938
    /// </summary>
    public static string GetTipoDocumentoNombre(TipoDocumentoEmpleado tipo) { ... }
    
    /// <summary>
    /// Obtiene el estado de un documento (vigente, próximo a vencer, vencido)
    /// Extraído de EmpleadoExpediente.razor líneas 940-947
    /// </summary>
    public static string GetDocumentoStatus(DocumentoEmpleado doc) { ... }
    
    /// <summary>
    /// Verifica si un MIME type corresponde a imagen
    /// Extraído de EmpleadoExpediente.razor líneas 756-759
    /// </summary>
    public static bool IsImageMime(string? mimeType) { ... }
}
```

#### B) `DateHelper.cs`
```csharp
namespace SGRRHH.Local.Shared.Helpers;

public static class DateHelper
{
    /// <summary>
    /// Calcula la edad a partir de fecha de nacimiento
    /// Extraído de EmpleadoExpediente.razor líneas 890-896
    /// </summary>
    public static int CalcularEdad(DateTime fechaNacimiento) { ... }
    
    /// <summary>
    /// Calcula la antigüedad laboral formateada (X años, Y meses)
    /// Extraído de EmpleadoExpediente.razor líneas 898-908
    /// </summary>
    public static string CalcularAntiguedad(DateTime fechaIngreso) { ... }
}
```

#### C) `FormatHelper.cs`
```csharp
namespace SGRRHH.Local.Shared.Helpers;

public static class FormatHelper
{
    /// <summary>
    /// Formatea tamaño de archivo en bytes a formato legible (KB, MB)
    /// Extraído de EmpleadoExpediente.razor líneas 949-960
    /// </summary>
    public static string FormatFileSize(long bytes) { ... }
    
    /// <summary>
    /// Obtiene nombre legible de tipo de contrato
    /// Extraído de EmpleadoExpediente.razor líneas 962-975
    /// </summary>
    public static string GetTipoContratoDisplay(TipoContrato tipo) { ... }
}
```

---

### 2. Centralizar Consultas DB Repetidas

En `InformacionBancariaTab.razor.cs` y otros componentes, hay consultas idénticas que se repiten.

#### Crear método en cada Tab component:
```csharp
// En InformacionBancariaTab.razor.cs
private async Task RecargarCuentasAsync()
{
    cuentasBancarias = (await CuentaBancariaRepo.GetByEmpleadoIdAsync(EmpleadoId)).ToList();
    StateHasChanged();
}
```

**Buscar y reemplazar** todas las instancias duplicadas de:
```csharp
cuentasBancarias = (await CuentaBancariaRepo.GetByEmpleadoIdAsync(EmpleadoId))
    .OrderByDescending(c => c.EsCuentaNomina)
    .ThenByDescending(c => c.FechaCreacion)
    .ToList();
```

Por llamadas al nuevo método centralizado.

---

### 3. Extraer Validaciones Comunes

Crear clase `ValidationHelper.cs` en `SGRRHH.Local.Shared/Helpers/`:

```csharp
namespace SGRRHH.Local.Shared.Helpers;

public static class ValidationHelper
{
    /// <summary>
    /// Valida campo requerido y muestra error si está vacío
    /// </summary>
    public static bool ValidarCampoRequerido(string? valor, string nombreCampo, dynamic? messageToast)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            messageToast?.ShowError($"{nombreCampo} es obligatorio");
            return false;
        }
        return true;
    }
    
    /// <summary>
    /// Valida fecha requerida
    /// </summary>
    public static bool ValidarFechaRequerida(DateTime? fecha, string nombreCampo, dynamic? messageToast)
    {
        if (!fecha.HasValue)
        {
            messageToast?.ShowError($"{nombreCampo} es obligatoria");
            return false;
        }
        return true;
    }
}
```

---

## 📁 Archivos a Crear

| Archivo | Ubicación |
|---------|-----------|
| `DocumentHelper.cs` | `SGRRHH.Local.Shared/Helpers/DocumentHelper.cs` |
| `DateHelper.cs` | `SGRRHH.Local.Shared/Helpers/DateHelper.cs` |
| `FormatHelper.cs` | `SGRRHH.Local.Shared/Helpers/FormatHelper.cs` |
| `ValidationHelper.cs` | `SGRRHH.Local.Shared/Helpers/ValidationHelper.cs` |

---

## 📁 Archivos a Modificar

| Archivo | Cambios |
|---------|---------|
| `EmpleadoExpediente.razor` | Reemplazar métodos locales por llamadas a Helpers |
| `InformacionBancariaTab.razor.cs` | Usar ValidationHelper, crear RecargarCuentasAsync |
| `DotacionEppTab.razor.cs` | Usar helpers donde aplique |
| `ContratosTab.razor.cs` | Usar FormatHelper.GetTipoContratoDisplay |

---

## ✅ Verificación

1. **Build exitoso**: `dotnet build -v:m /bl:build.binlog 2>&1 | Tee-Object build.log`
2. **Funcionalidad preservada**: Los tabs deben funcionar igual que antes
3. **Reducción de código**: Verificar que se eliminó código duplicado

---

## 📝 Orden de Implementación

1. Crear carpeta `Helpers/` en `SGRRHH.Local.Shared/`
2. Crear `DocumentHelper.cs` con métodos extraídos
3. Crear `DateHelper.cs` con métodos extraídos
4. Crear `FormatHelper.cs` con métodos extraídos
5. Crear `ValidationHelper.cs` con validaciones comunes
6. Modificar `EmpleadoExpediente.razor` para usar helpers
7. Modificar tabs para usar helpers
8. Build y verificar
9. Eliminar métodos duplicados originales

---

## ⚠️ Notas Importantes

- **NO modificar lógica de negocio**, solo mover código
- **Preservar exactamente** el comportamiento actual
- **Agregar `using`** donde sea necesario para los nuevos helpers
- Los helpers deben ser **métodos estáticos puros** (sin estado)
