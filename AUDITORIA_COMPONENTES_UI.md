# 📊 Auditoría de Componentes y Experiencia de Usuario

## Comparativa: WebAssembly + Firebase vs Blazor Server + SQLite (Local)

**Fecha de Auditoría:** 8 de enero de 2026  
**Versiones Analizadas:**
- **SGRRHH.Web** (WebAssembly + Firebase) - En `src/SGRRHH.Web/`
- **SGRRHH.Local.Server** (Blazor Server + SQLite) - En `SGRRHH.Local/`

---

## 1. 📦 Inventario de Funcionalidades

### 1.1 Resumen de Páginas

| Módulo | WebAssembly (Firebase) | Blazor Server (Local) | Estado |
|--------|------------------------|----------------------|--------|
| Dashboard | ✅ | ✅ | Paridad |
| Empleados | ✅ | ✅ | Paridad |
| Permisos | ✅ | ✅ | Paridad |
| Vacaciones | ✅ | ✅ | Paridad |
| Contratos | ✅ | ✅ | Paridad |
| Control Diario | ✅ | ✅ | Paridad |
| Control Diario Wizard | ✅ | ✅ | Paridad |
| Documentos | ✅ | ✅ | Paridad |
| Reportes | ❌ **En desarrollo** | ✅ **COMPLETO** | **BRECHA CRÍTICA** |
| Usuarios | ✅ | ✅ | Paridad |
| Configuración | ✅ | ✅ | Paridad |
| Catálogos (Cargos, Deptos) | ✅ Páginas separadas | ✅ Página unificada | Diferencia de diseño |
| Actividades | ✅ | ✅ (Tab en Catalogos) | Paridad |
| Proyectos | ✅ | ✅ (Tab en Catalogos) | Paridad |
| Tipos Permiso | ✅ | ✅ (Tab en Catalogos) | Paridad |
| Auditoría | ❌ No existe | ✅ | **FALTANTE EN WEB** |

---

## 2. 🚨 Funcionalidades Faltantes en Versión WebAssembly (Firebase)

### 2.1 CRÍTICO: Módulo de Reportes

**Estado actual en WebAssembly:** Solo placeholder con mensaje "Módulo de reportes en desarrollo..."

**Funcionalidades disponibles en Blazor Server (Local) que FALTAN:**

| Reporte | Descripción | Funcionalidades |
|---------|-------------|-----------------|
| **Listado de Empleados** | PDF/Excel con filtros | ✅ Filtro por estado, departamento, cargo, ordenamiento |
| **Reporte de Permisos** | Por rango de fechas | ✅ Filtro por empleado, tipo, estado, departamento |
| **Reporte de Vacaciones** | Por año/periodo | ✅ Filtro por empleado, estado, departamento |
| **Reporte de Asistencia** | Control diario | ✅ Por rango de fechas, empleado, departamento |
| **Certificado Laboral** | PDF individual | ✅ Incluye propósito, opción de incluir salario |

**Servicios de exportación faltantes en WebAssembly:**
- `IReportService` - Generación de PDFs y Excel
- `IExportService` - Exportación general a Excel

### 2.2 Página de Auditoría

La versión Local tiene una página `/auditoria` que permite:
- Ver logs de actividad del sistema
- Rastrear cambios realizados por usuarios
- Historial de operaciones

**No existe equivalente en la versión WebAssembly.**

### 2.3 Exportación a Excel (Empleados)

| Funcionalidad | WebAssembly | Local |
|--------------|-------------|-------|
| Exportar empleados a Excel | ❌ No implementado | ✅ Botón F10, genera .xlsx |

---

## 3. 🔄 Diferencias en Funcionalidades Existentes

### 3.1 Página de Empleados

| Característica | WebAssembly | Local |
|----------------|-------------|-------|
| Atajos de teclado (F2, F3, F4, F5, F10) | ❌ No | ✅ Sí, con barra visual |
| Exportar a Excel | ❌ No | ✅ Sí (F10) |
| Vista expediente completo | ❌ No | ✅ Sí (botón EXPEDIENTE) |
| Filtro por estado | Checkbox "Mostrar Inactivos" | Select con todos los estados |
| Cálculo de edad en formulario | ❌ No muestra | ✅ Muestra edad calculada |
| Cálculo de antigüedad | ✅ En detalle | ✅ En formulario y detalle |
| Contacto de emergencia | ❌ Solo en edición | ✅ Sección dedicada visible |
| Preview de foto | ✅ Básico | ✅ Con placeholder de iniciales |
| Parámetro URL para abrir empleado | ❌ No | ✅ `/empleados/{id}` abre modal |

### 3.2 Página de Vacaciones

| Característica | WebAssembly | Local |
|----------------|-------------|-------|
| Atajos de teclado | ❌ No | ✅ F2-F7, ESC |
| Filtro por período (año) | ❌ No | ✅ Sí |
| Historial de vacaciones en modal | ❌ No | ✅ Últimas 10 vacaciones |
| Cálculo de días disponibles | ✅ Via servicio | ✅ Cálculo local con antigüedad |
| Botón "Marcar como Disfrutada" | ✅ Sí | ❌ Solo aprobar/rechazar |
| Botón eliminar vacación | ✅ Solo admin | ❌ No disponible |
| Editar vacaciones aprobadas | ✅ Limitado | ✅ Con más control |

### 3.3 Página de Permisos

| Característica | WebAssembly | Local |
|----------------|-------------|-------|
| Atajos de teclado | ❌ No | ✅ F2-F7, F12, ESC |
| Aprobar todos pendientes (batch) | ✅ Sí | ❌ No |
| Generar Acta PDF | ❌ No | ✅ Sí (F12) |
| Subir documento soporte | ❌ No | ✅ Sí |
| Descargar documento soporte | ❌ No | ✅ Sí |
| Filtro por empleado | ❌ No | ✅ Sí |

### 3.4 Página de Control Diario

| Característica | WebAssembly | Local |
|----------------|-------------|-------|
| Atajos de teclado | ❌ No | ✅ F2, F3, F5, ESC |
| Crear registros masivos | ✅ Wizard en nueva ventana | ✅ Wizard integrado |
| Panel empleados sin registro | ❌ No visible | ✅ Listado expandible |
| Edición inline de horarios | ❌ Solo en modal | ✅ Click directo en tabla |
| Selección múltiple | ❌ No | ✅ Checkboxes con select all |
| Estadísticas del día | ❌ Básicas | ✅ Cards detalladas |
| Navegación por fechas | ✅ Selector | ✅ Botones anterior/siguiente + hoy |
| Exportar reporte del día | ❌ No | ✅ Botón disponible |

### 3.5 Página de Documentos

| Característica | WebAssembly | Local |
|----------------|-------------|-------|
| Atajos de teclado | ❌ No | ✅ F3, F5, ESC |
| Almacenamiento | Firebase Storage | Sistema de archivos local |
| Vista previa de tamaño | ❌ No | ✅ Muestra tamaño formateado |
| Confirmación de eliminación | ✅ Básica | ✅ Modal dedicado |
| Selección de documento | ❌ No | ✅ Fila resaltada |

### 3.6 Dashboard

| Característica | WebAssembly | Local |
|----------------|-------------|-------|
| Atajos de teclado | ❌ No | ✅ F5 para actualizar |
| Alertas detalladas | ✅ Múltiples tipos | ❌ No |
| Lista de pendientes (permisos/vacaciones) | ❌ Solo conteo | ✅ Tabla con acciones |
| Contratos por vencer con días restantes | ❌ Solo conteo | ✅ Tabla con código de colores |
| Acciones rápidas | ✅ Iconos | ✅ Botones |

---

## 4. 🖥️ Reactividad de la UI

### 4.1 Evaluación de Reactividad Blazor Server (Local)

| Aspecto | Estado | Observaciones |
|---------|--------|---------------|
| Carga de datos después de CRUD | ✅ Correcto | `StateHasChanged()` llamado después de operaciones |
| Actualización de contadores | ✅ Correcto | Se recalculan tras cada operación |
| Respuesta a cambios en filtros | ✅ Correcto | `@bind:after` usado consistentemente |
| Edición inline | ✅ Funcional | Horarios editables directamente |
| Mensajes de éxito/error temporales | ✅ Correcto | Auto-limpieza con `Task.Delay` |
| Modales y diálogos | ✅ Correcto | Estados gestionados correctamente |

### 4.2 Problemas Potenciales de Reactividad en Local

1. **SignalR Dependency:** La versión Blazor Server depende de conexión SignalR constante
   - Si la conexión se pierde, la UI no responde
   - No hay mecanismo de reconexión visible implementado

2. **Carga inicial sin paginación:** Algunas páginas cargan todos los registros
   - `EmpleadoRepository.GetAllAsync()` sin límite
   - Puede causar lentitud con muchos registros

3. **No hay indicadores de carga granulares:** 
   - Solo `isLoading` global, no por sección

---

## 5. 🧭 Consistencia de Navegación

### 5.1 Rutas con Parámetros

| Página | WebAssembly | Local |
|--------|-------------|-------|
| Empleados con ID | ❌ No soporta | ✅ `/empleados/{id}` |
| Vacaciones con ID | ❌ No soporta | ✅ `/vacaciones/{id}` |
| Permisos con ID | ❌ No soporta | ✅ `/permisos/{id}` |
| Documentos con empleado | ✅ `/documentos/{empleadoId}` | ✅ `/documentos/{empleadoId}` |
| Control Diario con fecha | ❌ No soporta | ✅ `/control-diario/{fecha}` |

### 5.2 Permisos de Visualización

| Rol | WebAssembly | Local |
|-----|-------------|-------|
| `IsAdmin` | ✅ `AppState.IsAdmin` | ✅ `AuthService.IsAdmin` |
| `CanApprove` | ✅ `AppState.CanApprove` | ✅ `AuthService.IsAprobador` |
| `CanEdit` | ✅ `AppState.CanEdit*` | ❌ No granular |

**Inconsistencia:** La versión WebAssembly usa `AppStateService` mientras que Local usa `IAuthService` directamente.

### 5.3 Redirección a Login

| Versión | Implementación |
|---------|----------------|
| WebAssembly | Componente de autorización de Blazor |
| Local | Verificación manual `if (!AuthService.IsAuthenticated)` en `OnInitializedAsync` |

---

## 6. 📋 Resumen de Brechas

### Funcionalidades Faltantes en WebAssembly (Firebase)

1. **🔴 CRÍTICO: Módulo completo de Reportes**
   - Sin capacidad de generar PDFs
   - Sin exportación a Excel de reportes
   - Sin certificados laborales

2. **🔴 CRÍTICO: Exportación de Empleados a Excel**
   - No hay botón de exportación

3. **🟡 IMPORTANTE: Página de Auditoría**
   - No existe tracking de actividad

4. **🟡 IMPORTANTE: Documentos de Soporte en Permisos**
   - No se pueden adjuntar documentos

5. **🟢 MENOR: Atajos de Teclado**
   - No implementados en ninguna página

6. **🟢 MENOR: Navegación por parámetros URL**
   - No soporta abrir registros específicos via URL

### Errores de Comportamiento en Versión Local

1. **Potencial pérdida de conexión SignalR** sin manejo visible
2. **Carga completa de datos** sin paginación en algunas páginas
3. **Inconsistencia** en nombres de propiedades de permisos entre versiones

---

## 7. 🛠️ Recomendaciones

### Prioridad Alta

1. **Implementar módulo de Reportes en WebAssembly**
   - Considerar generación de PDFs del lado del cliente (jsPDF)
   - O implementar endpoint en Firebase Functions

2. **Agregar exportación a Excel en WebAssembly**
   - Usar librería como SheetJS para generación client-side

3. **Implementar atajos de teclado globales**
   - Crear componente `KeyboardHandler` similar al de Local

### Prioridad Media

4. **Unificar servicio de autenticación**
   - Estandarizar nombres de propiedades (`IsAdmin`, `CanApprove`)

5. **Agregar soporte de rutas con parámetros**
   - Permitir deep-linking a registros específicos

6. **Agregar página de Auditoría** en WebAssembly

### Prioridad Baja

7. **Mejorar indicadores de carga**
   - Agregar skeleton loaders por sección

8. **Implementar paginación**
   - Especialmente en listados grandes

---

## 8. 📁 Referencias de WPF a Limpiar

Se encontraron referencias al proyecto WPF eliminado en los siguientes archivos:

| Archivo | Línea | Descripción |
|---------|-------|-------------|
| `scripts/Publish-All.ps1` | 53 | Variable `$wpfProject` apunta a SGRRHH.WPF |
| `tools/MigrateToFirestore/Program.cs` | 115-167 | Rutas a SGRRHH.WPF |
| `tools/MigrateFilesToStorage/Program.cs` | 628-629 | Rutas a SGRRHH.WPF |
| `tools/CreateFirestoreUsers/Program.cs` | 7 | Ruta a credentials de WPF |
| `tools/apply_*.py` | Varios | Scripts que modificaban WPF |

**Recomendación:** Actualizar o eliminar estos scripts obsoletos.

---

*Auditoría generada automáticamente - SGRRHH*
