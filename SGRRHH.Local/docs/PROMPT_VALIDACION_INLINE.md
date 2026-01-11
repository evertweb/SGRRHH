# PROMPT: Implementar Validación Inline en Formularios SGRRHH

## CONTEXTO DEL PROYECTO

Estás trabajando en una aplicación **Blazor Server (.NET 8)** de gestión de Recursos Humanos (SGRRHH). La aplicación usa un tema visual estilo "hospital" con fuentes monospace, bordes negros y estética retro.

### Arquitectura:
- **Frontend**: Blazor Server con componentes `.razor`
- **Estilos**: CSS personalizado en `wwwroot/css/hospital.css`
- **JavaScript**: Funciones de utilidad en `wwwroot/js/app.js`
- **Componente Modal**: `Components/Shared/FormModal.razor` (modal reutilizable)

---

## OBJETIVO

Implementar **validación inline con enfoque automático** en TODOS los formularios de la aplicación. Cuando el usuario intente guardar sin completar campos requeridos o con datos inválidos:

1. ❌ **NO mostrar** banners de error al fondo del formulario
2. ✅ **Enfocar** automáticamente el campo con error
3. ✅ **Resaltar** el campo con borde rojo y fondo rosado
4. ✅ **Mostrar mensaje** de error debajo del campo
5. ✅ **Animar** con efecto "shake" para llamar la atención
6. ✅ **Limpiar** el error cuando el usuario comience a escribir

---

## INFRAESTRUCTURA YA IMPLEMENTADA

### 1. Funciones JavaScript (wwwroot/js/app.js)

```javascript
/**
 * Enfoca un elemento, lo resalta con error y muestra mensaje inline
 */
window.focusFieldWithError = function(elementId, errorMessage) {
    const element = document.getElementById(elementId);
    if (!element) return;
    
    // Limpiar errores previos
    clearAllFieldErrors();
    
    // Agregar clase de error al campo
    element.classList.add('field-error');
    
    // Buscar el grupo padre del campo
    const formGroup = element.closest('.hospital-form-group');
    if (formGroup) {
        formGroup.classList.add('has-error');
        
        // Crear mensaje de error inline si no existe
        let errorEl = formGroup.querySelector('.field-error-message');
        if (!errorEl) {
            errorEl = document.createElement('div');
            errorEl.className = 'field-error-message';
            formGroup.appendChild(errorEl);
        }
        errorEl.textContent = errorMessage;
        errorEl.style.display = 'block';
    }
    
    // Hacer scroll al elemento y enfocar
    element.scrollIntoView({ behavior: 'smooth', block: 'center' });
    setTimeout(() => element.focus(), 100);
    
    // Limpiar error cuando el usuario escriba
    element.addEventListener('input', function clearError() {
        element.classList.remove('field-error');
        if (formGroup) {
            formGroup.classList.remove('has-error');
            const errorEl = formGroup.querySelector('.field-error-message');
            if (errorEl) errorEl.style.display = 'none';
        }
        element.removeEventListener('input', clearError);
    }, { once: true });
};

/**
 * Limpia todos los errores de campos
 */
window.clearAllFieldErrors = function() {
    document.querySelectorAll('.field-error').forEach(el => el.classList.remove('field-error'));
    document.querySelectorAll('.has-error').forEach(el => el.classList.remove('has-error'));
    document.querySelectorAll('.field-error-message').forEach(el => el.style.display = 'none');
};
```

### 2. Estilos CSS (wwwroot/css/hospital.css)

```css
/* Estilos de error de campo inline */
.hospital-form-group.has-error input,
.hospital-form-group.has-error select,
.hospital-form-group.has-error textarea {
    border-color: #CC0000 !important;
    background-color: #FFF5F5 !important;
    animation: shake 0.3s ease-in-out;
}

.hospital-form-group.has-error input:focus,
.hospital-form-group.has-error select:focus,
.hospital-form-group.has-error textarea:focus {
    box-shadow: 0 0 0 2px rgba(204, 0, 0, 0.3) !important;
    outline: none !important;
}

.hospital-form-group .field-error-message {
    display: none;
    font-size: 11px;
    color: #CC0000;
    font-weight: 600;
    margin-top: 6px;
    padding: 6px 8px;
    background: #FFEBEE;
    border-left: 3px solid #CC0000;
    text-align: left;
}

.field-error {
    border-color: #CC0000 !important;
    background-color: #FFF5F5 !important;
}

@keyframes shake {
    0%, 100% { transform: translateX(0); }
    20%, 60% { transform: translateX(-5px); }
    40%, 80% { transform: translateX(5px); }
}
```

---

## PASOS PARA IMPLEMENTAR EN CADA FORMULARIO

### Paso 1: Agregar inyección de IJSRuntime

Si el componente NO tiene `@inject IJSRuntime JSRuntime`, agregarlo en las directivas:

```razor
@inject IJSRuntime JSRuntime
```

### Paso 2: Agregar IDs únicos a los campos del formulario

Cada `<input>`, `<select>` o `<textarea>` que requiera validación debe tener un `id` único:

**ANTES:**
```razor
<div class="hospital-form-group required">
    <label>NOMBRE</label>
    <input type="text" @bind="editItem.Nombre" maxlength="100" />
</div>
```

**DESPUÉS:**
```razor
<div class="hospital-form-group required">
    <label>NOMBRE</label>
    <input type="text" id="nombreInput" @bind="editItem.Nombre" maxlength="100" />
</div>
```

#### Convención de nombres para IDs:
- Usar camelCase
- Sufijo `Input` para inputs de texto
- Sufijo `Select` para selects
- Sufijo `Date` para campos de fecha
- Ejemplos: `codigoInput`, `nombreInput`, `departamentoSelect`, `fechaInicioDate`

### Paso 3: Modificar el método de validación/guardado

**ANTES (usando SetError con banner):**
```csharp
private async Task Guardar()
{
    if (editItem == null) return;
    
    if (string.IsNullOrWhiteSpace(editItem.Nombre))
    {
        await SetError("El nombre es requerido");
        return;
    }
    
    if (string.IsNullOrWhiteSpace(editItem.Codigo))
    {
        await SetError("El código es requerido");
        return;
    }
    
    // ... resto del guardado
}
```

**DESPUÉS (usando focusFieldWithError):**
```csharp
private async Task Guardar()
{
    if (editItem == null) return;
    
    // Limpiar errores previos
    await JSRuntime.InvokeVoidAsync("clearAllFieldErrors");
    
    if (string.IsNullOrWhiteSpace(editItem.Nombre))
    {
        await JSRuntime.InvokeVoidAsync("focusFieldWithError", "nombreInput", "EL NOMBRE ES REQUERIDO");
        return;
    }
    
    if (string.IsNullOrWhiteSpace(editItem.Codigo))
    {
        await JSRuntime.InvokeVoidAsync("focusFieldWithError", "codigoInput", "EL CÓDIGO ES REQUERIDO");
        return;
    }
    
    // ... resto del guardado
}
```

### Paso 4: Mensajes de error en MAYÚSCULAS

Los mensajes deben estar en **MAYÚSCULAS** para mantener consistencia con el estilo visual de la aplicación:

```csharp
// ✅ Correcto
await JSRuntime.InvokeVoidAsync("focusFieldWithError", "nombreInput", "EL NOMBRE ES REQUERIDO");

// ❌ Incorrecto
await JSRuntime.InvokeVoidAsync("focusFieldWithError", "nombreInput", "El nombre es requerido");
```

---

## FORMULARIOS A ACTUALIZAR

Buscar todos los componentes `.razor` en `Components/Pages/` que tengan formularios con validación. Identificar por:

1. Uso de `<FormModal>` o modales de edición
2. Métodos `Guardar()`, `Save()`, `HandleSubmit()` o similares
3. Llamadas a `SetError()` para validación

### Lista de archivos a revisar:

```
Components/Pages/
├── DepartamentosTab.razor      ✅ YA IMPLEMENTADO (usar como referencia)
├── CargosTab.razor
├── TiposContratoTab.razor
├── TiposDocumentoTab.razor
├── NivelesEducativosTab.razor
├── EstadosCivilesTab.razor
├── TiposPermisoTab.razor
├── FondosPensionTab.razor
├── EPSTab.razor
├── CajasCompensacionTab.razor
├── ARLTab.razor
├── BancosTab.razor
├── Empleados.razor
├── EmpleadoForm.razor
├── Contratos.razor
├── Nomina.razor
├── Vacaciones.razor
├── Permisos.razor
├── Prestaciones.razor
├── Incapacidades.razor
├── Usuarios.razor
├── Configuracion.razor
└── ... (otros formularios)
```

---

## EJEMPLO COMPLETO: DepartamentosTab.razor

### Directivas (inicio del archivo):
```razor
@using SGRRHH.Local.Domain.Entities
@using SGRRHH.Local.Shared.Interfaces
@inject IDepartamentoRepository DepartamentoRepository
@inject IEmpleadoRepository EmpleadoRepository
@inject ILogger<DepartamentosTab> Logger
@inject IJSRuntime JSRuntime
```

### Formulario con IDs:
```razor
<div class="hospital-form-group required">
    <label>CÓDIGO</label>
    <div class="hospital-input-prefix">
        <span class="prefix">DP-</span>
        <input type="text" 
               id="codigoInput"
               @bind="codigoNumero" 
               @bind:event="oninput"
               maxlength="10" 
               disabled="@isSaving"
               placeholder="1, 2, 3..." />
    </div>
    <small>INGRESE EL NÚMERO DEL CÓDIGO (EJ: 1, 2, 3...)</small>
</div>

<div class="hospital-form-group required">
    <label>NOMBRE DEL ÁREA/DEPARTAMENTO</label>
    <input type="text" id="nombreInput" @bind="editItem.Nombre" maxlength="100" 
           disabled="@isSaving" placeholder="EJ: RECURSOS HUMANOS, CONTABILIDAD, SISTEMAS..." />
</div>
```

### Método Guardar con validación inline:
```csharp
private async Task Guardar()
{
    if (editItem == null) return;
    
    // Limpiar errores previos
    await JSRuntime.InvokeVoidAsync("clearAllFieldErrors");
    
    // Validar código
    if (string.IsNullOrWhiteSpace(codigoNumero))
    {
        await JSRuntime.InvokeVoidAsync("focusFieldWithError", "codigoInput", "EL NÚMERO DEL CÓDIGO ES REQUERIDO");
        return;
    }
    
    // Validar que sea un número válido
    codigoNumero = codigoNumero.Trim();
    if (!int.TryParse(codigoNumero, out var numCodigo) || numCodigo <= 0)
    {
        await JSRuntime.InvokeVoidAsync("focusFieldWithError", "codigoInput", "EL CÓDIGO DEBE SER UN NÚMERO POSITIVO (EJ: 1, 2, 3...)");
        return;
    }
    
    // Construir código completo
    editItem.Codigo = $"DP-{numCodigo}";
    
    // Validar nombre
    if (string.IsNullOrWhiteSpace(editItem.Nombre))
    {
        await JSRuntime.InvokeVoidAsync("focusFieldWithError", "nombreInput", "EL NOMBRE DEL ÁREA/DEPARTAMENTO ES REQUERIDO");
        return;
    }
    
    // Validar código duplicado
    if (await DepartamentoRepository.ExistsCodigoAsync(editItem.Codigo, isEditing ? editItem.Id : null))
    {
        await JSRuntime.InvokeVoidAsync("focusFieldWithError", "codigoInput", $"YA EXISTE UN ÁREA/DEPARTAMENTO CON EL CÓDIGO '{editItem.Codigo}'");
        return;
    }
    
    // Validar nombre duplicado
    if (await DepartamentoRepository.ExistsByNameAsync(editItem.Nombre, isEditing ? editItem.Id : null))
    {
        await JSRuntime.InvokeVoidAsync("focusFieldWithError", "nombreInput", $"YA EXISTE UN ÁREA/DEPARTAMENTO CON EL NOMBRE '{editItem.Nombre}'");
        return;
    }
    
    // Si pasa todas las validaciones, continuar con el guardado...
    isSaving = true;
    StateHasChanged();
    
    try
    {
        // ... lógica de guardado
    }
    catch (Exception ex)
    {
        // Para errores de sistema, sí usar SetError (no es error de campo específico)
        await SetError($"Error al guardar: {ex.Message}");
    }
}
```

---

## NOTAS IMPORTANTES

1. **SetError() sigue siendo válido** para errores de sistema (excepciones, errores de BD, etc.) que no corresponden a un campo específico.

2. **No modificar** los métodos `SetError()` y `SetSuccess()` existentes, solo cambiar las llamadas de validación de campos.

3. **Compilar después de cada archivo** modificado para verificar que no hay errores:
   ```powershell
   dotnet build --no-restore
   ```

4. **Probar la funcionalidad** abriendo el formulario, dejando campos vacíos y haciendo clic en GUARDAR.

5. **Para campos dentro de input-prefix** (como el código con prefijo "DP-"), el ID va en el `<input>`, no en el div contenedor.

6. **Para selects**, agregar listener de `change` en lugar de `input` si es necesario limpiar el error al cambiar selección.

---

## ORDEN DE IMPLEMENTACIÓN SUGERIDO

1. **Catálogos simples** (tienen estructura similar):
   - CargosTab.razor
   - TiposContratoTab.razor
   - TiposDocumentoTab.razor
   - NivelesEducativosTab.razor
   - EstadosCivilesTab.razor
   - TiposPermisoTab.razor
   - FondosPensionTab.razor
   - EPSTab.razor
   - CajasCompensacionTab.razor
   - ARLTab.razor
   - BancosTab.razor

2. **Formularios complejos**:
   - EmpleadoForm.razor (muchos campos)
   - Contratos.razor
   - Nomina.razor
   - Vacaciones.razor
   - Permisos.razor
   - Prestaciones.razor
   - Incapacidades.razor

3. **Administración**:
   - Usuarios.razor
   - Configuracion.razor

---

## VERIFICACIÓN FINAL

Después de implementar en todos los formularios:

1. ✅ Verificar que compila sin errores
2. ✅ Probar cada formulario dejando campos vacíos
3. ✅ Verificar que el campo se enfoca y resalta
4. ✅ Verificar que el mensaje aparece debajo del campo
5. ✅ Verificar que el error se limpia al escribir
6. ✅ Verificar que los errores de sistema (excepciones) siguen usando el banner

---

*Prompt generado para propagación de validación inline - SGRRHH Local*
