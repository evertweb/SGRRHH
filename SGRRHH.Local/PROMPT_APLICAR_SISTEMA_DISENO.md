# PROMPT: Aplicar Sistema de Diseño Hospital/ForestechOil en SGRRHH.Local

## 📋 CONTEXTO

Eres un agente especializado en adaptar aplicaciones Blazor Server al sistema de diseño **Hospital/ForestechOil**. Tu misión es aplicar este diseño de manera consistente en **TODAS** las páginas y componentes de la aplicación SGRRHH.Local.

**La infraestructura base YA ESTÁ LISTA:**
- ✅ `wwwroot/css/hospital.css` - CSS completo con variables y estilos
- ✅ `Components/Layout/MainLayout.razor` - Layout principal adaptado
- ✅ `Components/Layout/EmptyLayout.razor` - Layout de login adaptado
- ✅ `Components/Shared/SatelliteSpinner.razor` - Spinner en estilo hospital
- ✅ `wwwroot/docs/ui-guide.html` - Guía visual de componentes

## 🎯 TU OBJETIVO

Revisar y adaptar **CADA PÁGINA Y COMPONENTE** para que use consistentemente el sistema de diseño hospital. NO cambies lógica C#, SOLO adapta estilos y estructura HTML/Razor.

## 📐 PRINCIPIOS DEL DISEÑO HOSPITAL/FORESTECHOIL

### 1. ESTÉTICA RECTANGULAR Y MINIMALISTA
- **CERO border-radius**: Todo es rectangular
- **CERO animaciones**: `animation: none !important;`
- **CERO transiciones**: `transition: none !important;`
- Fuente monoespaciada: `'Courier New', Courier, monospace`
- Bordes sólidos: `1px solid #808080` o `2px solid #000000`

### 2. PALETA DE COLORES (usar variables CSS)
```css
/* Variables principales */
--color-bg: #FFFFFF;
--color-text: #000000;
--color-border: #808080;
--color-header: #E0E0E0;      /* Fondos de header/th */
--color-nav: #F0F0F0;          /* Fondos de navegación/toolbar */
--color-panel: #FAFAFA;        /* Fondos de paneles/cards */
--color-hover: #F0F0FF;        /* Hover en filas */
--color-selected: #D0D0FF;     /* Selección activa */

/* Botones */
--color-btn-base: #E0E0E0;
--color-btn-hover: #D0D0D0;
--color-btn-primary: #000000;
--color-btn-primary-hover: #333333;

/* Estados */
--color-success: #4CAF50;
--color-success-bg: #CCFFCC;
--color-error: #F44336;
--color-error-bg: #FFF0F0;
--color-warning: #FF9800;
--color-warning-bg: #FFF3CD;
```

### 3. TIPOGRAFÍA
- Fuente base: `14px` / `var(--font-size-base)`
- Labels: `12px` / `var(--font-size-sm)` en **MAYÚSCULAS** y **bold**
- Títulos de sección: `18px` / `var(--font-size-lg)` en **MAYÚSCULAS** y **bold**
- Headers de tabla: `11px` / `var(--font-size-xs)` en **MAYÚSCULAS** y **bold**

### 4. ESPACIADO (usar variables)
```css
--spacing-xs: 4px;
--spacing-sm: 8px;
--spacing-md: 12px;
--spacing-lg: 16px;
--spacing-xl: 24px;
--spacing-xxl: 32px;
```

## 🔧 COMPONENTES A ADAPTAR

### A. BOTONES
**ANTES (evitar):**
```html
<button style="border-radius: 4px; background: #007bff;">Guardar</button>
```

**DESPUÉS (correcto):**
```html
<button class="btn-primary">GUARDAR</button>
```

**Clases disponibles:**
- `.btn` - Botón normal (fondo gris claro)
- `.btn-primary` - Botón primario (fondo negro, texto blanco)
- `.btn-danger` - Botón peligro (fondo rojo)
- `.btn-success` - Botón éxito (fondo verde)

### B. FORMULARIOS

**ANTES (evitar):**
```html
<div style="margin-bottom: 10px;">
    <label style="font-size: 14px;">Nombre</label>
    <input type="text" style="border-radius: 3px;" />
</div>
```

**DESPUÉS (correcto):**
```html
<div class="form-group">
    <label class="required">NOMBRE COMPLETO</label>
    <input type="text" placeholder="Ingrese el nombre completo" />
</div>
```

**Para 2 columnas:**
```html
<div class="form-row">
    <div class="form-group">
        <label>CÓDIGO</label>
        <input type="text" />
    </div>
    <div class="form-group">
        <label>CÉDULA</label>
        <input type="text" />
    </div>
</div>
```

**Validación de errores:**
```html
<input type="email" class="input-error" />
<div class="validation-message">El formato del email no es válido</div>
```

### C. TABLAS

**Estructura correcta:**
```html
<table>
    <thead>
        <tr>
            <th>CÓDIGO</th>
            <th>NOMBRE</th>
            <th>ESTADO</th>
            <th>ACCIONES</th>
        </tr>
    </thead>
    <tbody>
        @if (isLoading)
        {
            <tr><td colspan="4" class="loading">Cargando...</td></tr>
        }
        else if (!items.Any())
        {
            <tr><td colspan="4" class="empty-state">No hay registros para mostrar</td></tr>
        }
        else
        {
            @foreach (var item in items)
            {
                <tr class="@(selectedItem?.Id == item.Id ? "selected" : "")"
                    @onclick="() => SelectItem(item)">
                    <td>@item.Codigo</td>
                    <td><strong>@item.Nombre</strong></td>
                    <td>
                        <span class="badge badge-@item.Estado.ToString().ToLower()">
                            @item.Estado
                        </span>
                    </td>
                    <td class="table-actions">
                        <button @onclick="() => Edit(item)" @onclick:stopPropagation="true">
                            EDITAR
                        </button>
                        <button @onclick="() => Delete(item)" @onclick:stopPropagation="true">
                            ELIMINAR
                        </button>
                    </td>
                </tr>
            }
        }
    </tbody>
</table>
```

**Características:**
- Headers con fondo `#E0E0E0` (ya en CSS global)
- Texto de headers en MAYÚSCULAS
- Fila `.selected` con fondo `#D0D0FF`
- Hover con fondo `#F0F0FF`
- Loading/empty states centralizados

### D. MODALES (usando FormModal)

**Estructura recomendada:**
```html
<FormModal @ref="formModal" 
           IsVisible="showModal" 
           Title="CREAR NUEVO EMPLEADO"
           Width="700px"
           OnSaveClicked="GuardarEmpleado"
           OnCancelClicked="CerrarModal"
           IsSaving="isSaving">
    
    <!-- Validation Summary arriba -->
    @if (validationErrors.Any())
    {
        <div class="validation-summary">
            <h4>⚠ CORRIJA LOS SIGUIENTES ERRORES:</h4>
            <ul>
                @foreach (var error in validationErrors)
                {
                    <li>@error</li>
                }
            </ul>
        </div>
    }
    
    <!-- Form Grid 2 columnas -->
    <div class="form-row">
        <div class="form-group">
            <label class="required">CÓDIGO</label>
            <input type="text" @bind="model.Codigo" />
        </div>
        <div class="form-group">
            <label class="required">CÉDULA</label>
            <input type="text" @bind="model.Cedula" />
        </div>
    </div>
    
    <div class="form-group">
        <label class="required">NOMBRE COMPLETO</label>
        <input type="text" @bind="model.NombreCompleto" />
    </div>
    
    <!-- Más campos... -->
    
</FormModal>
```

### E. ALERTAS Y MENSAJES

**Success:**
```html
<div class="alert alert-success">
    ✓ La operación se completó exitosamente
</div>

<!-- O usar success-block para más énfasis -->
<div class="success-block">
    <div class="title">✓ OPERACIÓN EXITOSA</div>
    <div class="message">El empleado se guardó correctamente</div>
</div>
```

**Error:**
```html
<div class="alert alert-error">
    ✗ Ocurrió un error al procesar la solicitud
</div>

<!-- O usar error-block -->
<div class="error-block">
    <div class="title">✗ ERROR DE VALIDACIÓN</div>
    <div class="message">Corrija los errores en el formulario</div>
</div>
```

**Warning:**
```html
<div class="alert alert-warning">
    ⚠ Esta acción no se puede deshacer
</div>
```

### F. BADGES (Estados)

```html
<!-- Estados de empleado -->
<span class="badge badge-activo">ACTIVO</span>
<span class="badge badge-inactivo">INACTIVO</span>
<span class="badge badge-pendiente">PENDIENTE</span>
<span class="badge badge-retirado">RETIRADO</span>
<span class="badge badge-suspendido">SUSPENDIDO</span>

<!-- Estados de permiso/vacación -->
<span class="badge badge-aprobado">APROBADO</span>
<span class="badge badge-rechazado">RECHAZADO</span>
<span class="badge badge-cancelado">CANCELADO</span>
```

### G. TOOLBARS (Barras de herramientas)

```html
<div class="toolbar">
    <div class="toolbar-group">
        <button class="btn-primary">NUEVO (F3)</button>
        <button>EDITAR (F4)</button>
        <button class="btn-danger">ELIMINAR</button>
    </div>
    
    <div class="toolbar-separator"></div>
    
    <div class="toolbar-group">
        <button>EXPORTAR</button>
        <button>ACTUALIZAR (F5)</button>
    </div>
    
    <div class="search-box">
        <input type="text" placeholder="Buscar... (F2)" @bind="searchTerm" />
        <button @onclick="Buscar">BUSCAR</button>
        @if (!string.IsNullOrEmpty(searchTerm))
        {
            <button @onclick="LimpiarBusqueda">LIMPIAR</button>
        }
    </div>
</div>
```

### H. STATS CARDS (Dashboard)

```html
<div class="stats-grid">
    <div class="stats-card">
        <h3>TOTAL EMPLEADOS</h3>
        <div class="value">125</div>
        <div class="label">Activos en el sistema</div>
    </div>
    
    <div class="stats-card">
        <h3>NUEVOS ESTE MES</h3>
        <div class="value">8</div>
        <div class="label">Ingresos recientes</div>
    </div>
    
    <div class="stats-card">
        <h3>PERMISOS PENDIENTES</h3>
        <div class="value">3</div>
        <div class="label">Requieren aprobación</div>
    </div>
</div>
```

### I. EMPTY STATES

```html
<div class="empty-state">
    <div class="icon">📋</div>
    <div class="message">No hay registros para mostrar</div>
    <div class="hint">Utilice el botón "NUEVO" para crear el primer registro</div>
</div>
```

### J. PANELS

```html
<div class="panel">
    <div class="panel-header">INFORMACIÓN PERSONAL</div>
    <div class="panel-body">
        <div class="form-group">
            <label>NOMBRE</label>
            <input type="text" value="@empleado.NombreCompleto" readonly />
        </div>
        <!-- Más campos... -->
    </div>
</div>
```

### K. LOADING SPINNER

```html
<!-- En el código del componente -->
<SatelliteSpinner IsVisible="isLoading" Message="CARGANDO DATOS" />

<!-- O para overlay más simple -->
@if (isLoading)
{
    <div class="loading-overlay">
        <div class="loading-content">
            <div class="loading-spinner">⏳</div>
            <div class="loading-message">PROCESANDO...</div>
            <div class="loading-dots">...</div>
        </div>
    </div>
}
```

## 📝 LISTA DE ARCHIVOS A REVISAR Y ADAPTAR

### PRIORIDAD ALTA ⭐⭐⭐
1. **Components/Pages/Login.razor** - Página de login
2. **Components/Pages/Empleados.razor** - Gestión de empleados
3. **Components/Pages/Documentos.razor** - Gestión de documentos
4. **Components/Pages/Permisos.razor** - Gestión de permisos
5. **Components/Pages/Vacaciones.razor** - Gestión de vacaciones
6. **Components/Pages/Contratos.razor** - Gestión de contratos
7. **Components/Pages/ControlDiario.razor** - Control diario

### PRIORIDAD MEDIA ⭐⭐
8. **Components/Pages/Catalogos.razor** - Catálogos (tabs)
9. **Components/Pages/Usuarios.razor** - Gestión de usuarios
10. **Components/Pages/Reportes.razor** - Reportes
11. **Components/Pages/Auditoria.razor** - Auditoría
12. **Components/Pages/Configuracion.razor** - Configuración
13. **Components/Pages/EmpleadoOnboarding.razor** - Wizard de ingreso

### PRIORIDAD BAJA ⭐
14. **Components/Shared/EmpleadoCard.razor** - Tarjeta de empleado
15. **Components/Shared/EmpleadoSelector.razor** - Selector de empleado
16. **Components/Shared/EstadoBadge.razor** - Badge de estado
17. **Components/Shared/NotificationBell.razor** - Campana de notificaciones
18. **Components/Shared/MessageToast.razor** - Notificaciones toast

## 🔍 PROCESO DE ADAPTACIÓN POR PÁGINA

### PASO 1: ANÁLISIS
1. Abrir el archivo .razor
2. Identificar todos los estilos inline (`style="..."`)
3. Identificar elementos HTML que necesitan clases del sistema
4. Buscar colores hardcodeados, border-radius, animaciones

### PASO 2: ADAPTACIÓN DE ESTRUCTURA
1. **Header/Título de página:**
   ```html
   <h1 class="page-title">GESTIÓN DE [MÓDULO]</h1>
   ```

2. **Toolbar/Acciones:**
   - Envolver botones en `<div class="toolbar">`
   - Usar clases de botón apropiadas
   - Agregar atajos de teclado en labels: "(F3)", "(F5)", etc.

3. **Búsqueda/Filtros:**
   - Usar `<div class="toolbar">` con `<div class="search-box">`
   - Inputs con placeholder descriptivo

4. **Tabla de datos:**
   - Asegurar que `<th>` tengan texto en MAYÚSCULAS
   - Agregar clases `.selected` para fila seleccionada
   - Loading/empty states consistentes

5. **Modales:**
   - Usar `<FormModal>` componente
   - Agregar `keyboard-hint-bar` con atajos
   - Validation summary arriba del form
   - Botones en footer con `.modal-actions`

### PASO 3: ADAPTACIÓN DE ESTILOS
1. **Eliminar todos los estilos inline** excepto:
   - Estilos dinámicos (ej: `display: @(show ? "block" : "none")`)
   - Estilos calculados en código C#
   - Estilos de layout específicos que no tienen clase

2. **Reemplazar con clases:**
   ```html
   <!-- ANTES -->
   <div style="display: flex; gap: 8px; margin-bottom: 16px;">
   
   <!-- DESPUÉS -->
   <div class="toolbar">
   <!-- O -->
   <div class="d-flex gap-2 mb-3">
   ```

3. **Usar variables CSS en estilos necesarios:**
   ```html
   <style>
       .custom-element {
           background-color: var(--color-panel);
           border: 1px solid var(--color-border);
           padding: var(--spacing-lg);
       }
   </style>
   ```

### PASO 4: REFINAMIENTO
1. Verificar consistencia con otras páginas
2. Asegurar que los textos importantes estén en MAYÚSCULAS
3. Verificar que todos los estados (loading, error, empty) estén manejados
4. Revisar responsive (si aplica)

### PASO 5: VALIDACIÓN
1. No debe haber `border-radius` en ningún lado
2. No debe haber `box-shadow` excepto en focus states
3. Todos los botones deben usar clases `.btn-*`
4. Todos los formularios deben usar `.form-group` o `.form-row`
5. Todas las tablas deben tener headers en MAYÚSCULAS

## 🚫 RESTRICCIONES IMPORTANTES

### NO HACER:
- ❌ NO cambiar lógica C# o métodos existentes
- ❌ NO modificar `@code { }` blocks (solo si es absolutamente necesario para UI)
- ❌ NO eliminar funcionalidad existente
- ❌ NO agregar dependencias nuevas
- ❌ NO usar Bootstrap classes (ya no se usa Bootstrap aquí)
- ❌ NO crear archivos CSS separados por página (usar hospital.css global)

### SÍ HACER:
- ✅ Mantener toda la funcionalidad actual
- ✅ Usar clases del sistema de hospital.css
- ✅ Asegurar consistencia visual entre páginas
- ✅ Mejorar UX manteniendo la funcionalidad
- ✅ Documentar cambios significativos en comentarios

## 📋 CHECKLIST POR ARCHIVO

Cuando adaptes un archivo, verifica:

```
[ ] Se eliminaron estilos inline innecesarios
[ ] Se usan clases de hospital.css
[ ] Se usan variables CSS donde sea apropiado
[ ] Títulos y labels en MAYÚSCULAS
[ ] Headers de tabla en MAYÚSCULAS
[ ] Botones usan clases .btn-*
[ ] Formularios usan .form-group / .form-row
[ ] Tablas tienen estados loading/empty
[ ] No hay border-radius
[ ] No hay animaciones/transiciones
[ ] Fuente es Courier New (herencia de hospital.css)
[ ] Colores siguen paleta del sistema
[ ] Atajos de teclado visibles donde aplique
[ ] Funcionalidad existente se mantiene 100%
```

## 🎨 EJEMPLO COMPLETO: ANTES Y DESPUÉS

### ANTES (sin sistema de diseño):
```html
@page "/empleados"

<h2>Empleados</h2>

<div style="margin-bottom: 20px;">
    <button @onclick="Nuevo" style="background: blue; color: white; padding: 10px; border-radius: 5px;">
        Nuevo
    </button>
</div>

<table style="width: 100%; border-collapse: collapse;">
    <thead style="background: #f5f5f5;">
        <tr>
            <th style="padding: 10px;">Código</th>
            <th>Nombre</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var emp in empleados)
        {
            <tr style="@(selected?.Id == emp.Id ? "background: lightblue;" : "")">
                <td style="padding: 8px;">@emp.Codigo</td>
                <td>@emp.Nombre</td>
            </tr>
        }
    </tbody>
</table>
```

### DESPUÉS (con sistema de diseño):
```html
@page "/empleados"

<h1 class="page-title">GESTIÓN DE EMPLEADOS</h1>

<div class="toolbar">
    <div class="toolbar-group">
        <button class="btn-primary" @onclick="Nuevo">
            NUEVO EMPLEADO (F3)
        </button>
        <button @onclick="Refrescar">ACTUALIZAR (F5)</button>
    </div>
    
    <div class="search-box">
        <input type="text" placeholder="Buscar... (F2)" @bind="searchTerm" />
        <button @onclick="Buscar">BUSCAR</button>
    </div>
</div>

<table>
    <thead>
        <tr>
            <th>CÓDIGO</th>
            <th>NOMBRE COMPLETO</th>
            <th>ESTADO</th>
            <th>ACCIONES</th>
        </tr>
    </thead>
    <tbody>
        @if (isLoading)
        {
            <tr><td colspan="4" class="loading">Cargando empleados...</td></tr>
        }
        else if (!empleados.Any())
        {
            <tr>
                <td colspan="4" class="empty-state">
                    No hay empleados para mostrar
                </td>
            </tr>
        }
        else
        {
            @foreach (var emp in empleados)
            {
                <tr class="@(selected?.Id == emp.Id ? "selected" : "")"
                    @onclick="() => Seleccionar(emp)">
                    <td><strong>@emp.Codigo</strong></td>
                    <td>@emp.NombreCompleto</td>
                    <td>
                        <span class="badge badge-@emp.Estado.ToString().ToLower()">
                            @emp.Estado
                        </span>
                    </td>
                    <td class="table-actions">
                        <button @onclick="() => Editar(emp)" @onclick:stopPropagation="true">
                            EDITAR
                        </button>
                    </td>
                </tr>
            }
        }
    </tbody>
</table>
```

## 📚 RECURSOS DISPONIBLES

1. **hospital.css** - `wwwroot/css/hospital.css`
   - Todas las clases y variables CSS

2. **ui-guide.html** - `wwwroot/docs/ui-guide.html`
   - Guía visual con ejemplos de todos los componentes
   - Abre en navegador para referencia visual

3. **Componentes existentes:**
   - `FormModal.razor` - Modal reutilizable
   - `DataTable.razor` - Tabla con paginación
   - `SatelliteSpinner.razor` - Spinner de carga
   - `KeyboardHandler.razor` - Manejo de atajos
   - `MessageToast.razor` - Notificaciones

4. **Layouts adaptados:**
   - `MainLayout.razor` - Ya sigue el diseño hospital
   - `EmptyLayout.razor` - Ya sigue el diseño hospital

## 🚀 ORDEN DE EJECUCIÓN RECOMENDADO

1. **Fase 1 - Páginas Principales (Día 1-2):**
   - Login.razor
   - Empleados.razor
   - Permisos.razor
   - Vacaciones.razor

2. **Fase 2 - Módulos Secundarios (Día 3-4):**
   - Documentos.razor
   - Contratos.razor
   - ControlDiario.razor
   - Catalogos.razor

3. **Fase 3 - Administración (Día 5):**
   - Usuarios.razor
   - Configuracion.razor
   - Auditoria.razor
   - Reportes.razor

4. **Fase 4 - Componentes Compartidos (Día 6):**
   - EmpleadoCard.razor
   - EmpleadoSelector.razor
   - MessageToast.razor (si necesita ajustes)
   - Otros componentes compartidos

5. **Fase 5 - Refinamiento Final (Día 7):**
   - Revisión de consistencia
   - Ajustes finales
   - Testing visual de todas las páginas

## 💡 TIPS PARA EFICIENCIA

1. **Patrones comunes:** Usa buscar/reemplazar para patrones repetitivos
2. **Reutiliza componentes:** Si ves algo repetido, considera extraer un componente
3. **Documenta casos especiales:** Si algo no puede seguir el patrón, documenta por qué
4. **Prueba incremental:** Prueba cada página después de adaptarla
5. **Consistencia sobre perfección:** Es mejor ser consistente que perfecto

## ✅ CRITERIOS DE ÉXITO

Una página está **correctamente adaptada** cuando:
1. ✅ Visualmente se ve igual a las páginas ya adaptadas
2. ✅ Usa exclusivamente clases de hospital.css (no estilos inline)
3. ✅ No tiene border-radius, sombras innecesarias, o animaciones
4. ✅ Textos importantes en MAYÚSCULAS (headers, labels, botones)
5. ✅ Funcionalidad original se mantiene 100%
6. ✅ Estados de loading, error, y empty están manejados
7. ✅ Es consistente con otras páginas del sistema

## 📞 PREGUNTAS FRECUENTES

**P: ¿Puedo agregar nuevas clases CSS?**
R: Sí, pero agrégalas a hospital.css, no crees archivos separados.

**P: ¿Qué hago si un componente necesita algo muy específico?**
R: Usa estilos inline solo si es absolutamente necesario y documenta por qué.

**P: ¿Debo cambiar la lógica C# para mejorar la UI?**
R: No, mantén la lógica intacta. Solo cambia HTML/CSS/Razor markup.

**P: ¿Qué hago con componentes de terceros?**
R: Envuélvelos en divs con clases del sistema para controlar su apariencia.

---

## 🎯 INICIO RÁPIDO

```bash
# 1. Revisa los recursos disponibles
- Abre wwwroot/docs/ui-guide.html en navegador
- Revisa hospital.css para ver clases disponibles

# 2. Empieza con una página simple
- Ejemplo: Login.razor o Catalogos.razor

# 3. Para cada página:
   a. Identifica estructura actual
   b. Mapea a componentes del sistema de diseño
   c. Reemplaza estilos inline con clases
   d. Verifica funcionalidad
   e. Prueba visualmente

# 4. Marca como completada en tu checklist
```

---

**¡Adelante! Transforma esta aplicación en una obra maestra del diseño Hospital/ForestechOil! 🏥✨**
