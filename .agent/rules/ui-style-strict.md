---
trigger: always_on
description: Apply when creating or modifying Blazor components. Strictly prohibits inline styles, arbitrary colors, and requires using hospital.css classes.
---

# Estilos UI Estrictos - SGRRHH

## Archivo Principal de Estilos

**Ubicación:** `SGRRHH.Local.Server/wwwroot/css/hospital.css`

> [!CAUTION]
> **REGLAS ESTRICTAMENTE PROHIBIDAS - VIOLACIÓN = ERROR CRÍTICO:**
> 
> 1. ❌ **NUNCA** crear bloques `<style>` inline en componentes `.razor`
> 2. ❌ **NUNCA** usar atributo `style="..."` en elementos HTML
> 3. ❌ **NUNCA** usar colores arbitrarios (#00ff00, rgb(), etc.)
> 4. ❌ **NUNCA** crear archivos CSS específicos por componente
> 5. ✅ **SIEMPRE** usar las clases CSS existentes en `hospital.css`

## Ejemplos de Violaciones

### ❌ INCORRECTO - Estilo Inline en Razor

```razor
@* ❌ PROHIBIDO *@
<style>
    .mi-boton {
        background: #00ff00;
        color: white;
    }
</style>

<button class="mi-boton">Click</button>
```

### ❌ INCORRECTO - Atributo style

```razor
@* ❌ PROHIBIDO *@
<div style="color: red; font-size: 14px;">
    Error
</div>
```

### ❌ INCORRECTO - Colores arbitrarios en CSS nuevo

```css
/* ❌ PROHIBIDO */
.nuevo-componente {
    background: #FF5733;  /* Color arbitrario */
    border: 1px solid green;
}
```

### ✅ CORRECTO - Usar clases existentes

```razor
@* ✅ CORRECTO *@
<div class="error-block">
    Error
</div>

<button class="btn-primary">GUARDAR</button>

<div class="modal-overlay">
    <div class="modal-box">
        <div class="modal-header">TÍTULO</div>
    </div>
</div>
```

## Variables CSS Obligatorias

Si necesitas crear estilos nuevos, **DEBES** usar estas variables:

### Colores Base
```css
var(--color-bg)           /* #FFFFFF - Fondo principal */
var(--color-text)         /* #000000 - Texto principal */
var(--color-border)       /* #666666 - Bordes oscuros */
var(--color-border-light) /* #CCCCCC - Bordes claros */
var(--color-disabled-bg)  /* #F0F0F0 - Fondo deshabilitado */
var(--color-disabled-text)/* #999999 - Texto deshabilitado */
```

### Colores de Estado
```css
var(--color-error)    /* #CC0000 - Errores, eliminación */
var(--color-warning)  /* #FF9900 - Advertencias */
var(--color-success)  /* #006600 - Éxito */
```

### Tipografía
```css
var(--font-family)      /* 'Courier New', monospace */
var(--font-size-base)   /* 13px */
var(--font-size-header) /* 14px */
```

## Clases CSS Disponibles

### Botones
| Clase | Uso |
|-------|-----|
| `.btn-action` | Botón de acción general |
| `.btn-primary` | Botón principal (azul) |
| `.btn-danger` | Acción destructiva (rojo) |
| `.btn-success` | Confirmación (verde) |
| `.btn-aprobar` | Aprobar (borde verde) |
| `.btn-rechazar` | Rechazar (borde rojo) |

### Modales
| Clase | Uso |
|-------|-----|
| `.modal-overlay` | Fondo oscuro semi-transparente |
| `.modal-box` | Contenedor del modal |
| `.modal-header` | Cabecera con título |
| `.modal-footer` | Pie con botones de acción |

### Formularios
| Clase | Uso |
|-------|-----|
| `.campo-form` | Contenedor de campo |
| `.campo-label` | Etiqueta de campo |
| `.campo-input` | Input de campo |
| `.campo-requerido` | Marca obligatorio (*) |
| `.campo-textarea` | Textarea |

### Mensajes
| Clase | Uso |
|-------|-----|
| `.error-block` | Bloque de error |
| `.success-block` | Bloque de éxito |
| `.loading-block` | Indicador de carga |

### Tablas
| Clase | Uso |
|-------|-----|
| `.data-table` | Tabla de datos principal |
| `.row-inactive` | Fila de registro inactivo |
| `.cell-truncate` | Celda con texto truncado |

### Estados
| Clase | Estado |
|-------|--------|
| `.estado-pendiente` | Pendiente (naranja) |
| `.estado-aprobada` | Aprobado (verde) |
| `.estado-rechazada` | Rechazado (rojo) |

## Si Necesitas Estilos Nuevos

**Proceso OBLIGATORIO:**

1. ✅ Verificar que NO existe una clase que puedas reutilizar
2. ✅ Agregar el nuevo estilo a `hospital.css` (NUNCA inline)
3. ✅ Usar SOLO las variables CSS existentes para colores
4. ✅ Documentar con comentario el propósito
5. ✅ Seguir naming convention: `nombre-componente` (kebab-case)

### Ejemplo de Estilo Nuevo Correcto

```css
/* ===== WIZARD CONTROL DIARIO ===== */
.wizard-step {
    font-family: var(--font-family);
    color: var(--color-text);
    border: 1px solid var(--color-border);
    background: var(--color-bg);
    padding: 10px;
}

.wizard-step-active {
    border-color: var(--color-success);
    font-weight: bold;
}

.wizard-step-error {
    border-color: var(--color-error);
    color: var(--color-error);
}
```

## Verificación Automática

Antes de aprobar cualquier PR o cambio UI, verificar:

```bash
# Buscar violaciones de estilos inline
grep -r "<style>" SGRRHH.Local.Server/Components/
grep -r 'style="' SGRRHH.Local.Server/Components/

# No debería retornar resultados
```

## Excepciones

**NO HAY EXCEPCIONES.** Esta regla se aplica a:
- ✅ Páginas (`@page`)
- ✅ Componentes reutilizables
- ✅ Tabs
- ✅ Modales
- ✅ Formularios
- ✅ Todo elemento UI

## Consecuencias de Violación

Si se detecta una violación:
1. El código NO se acepta
2. Se debe corregir inmediatamente
3. Se debe mover el estilo a `hospital.css` usando variables

---

**Última actualización:** 2026-01-28
