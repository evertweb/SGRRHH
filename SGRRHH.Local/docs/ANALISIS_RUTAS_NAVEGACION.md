# Análisis de Rutas y Navegación - SGRRHH.Local

> **Fecha de análisis:** Enero 2026  
> **Fase:** 1 - Investigación completada

---

## 1. Inventario de Rutas Actuales

### 1.1 Rutas Existentes (`@page`)

| Archivo | Rutas | Tipo |
|---------|-------|------|
| `Login.razor` | `/`, `/login` | Autenticación |
| `LoggedOut.razor` | `/logged-out` | Autenticación |
| `Home.razor` | `/home` | Dashboard |
| `Error.razor` | `/Error` | Sistema |
| **EMPLEADOS** | | |
| `Empleados.razor` | `/empleados`, `/empleados/{EmpleadoIdParam:int?}` | Lista + Detalle en modal |
| `EmpleadoOnboarding.razor` | `/empleados/onboarding` | Crear nuevo |
| `EmpleadoExpediente.razor` | `/empleados/{EmpleadoId:int}/expediente` | Detalle con **tabs internos** |
| **CONTRATOS** | | |
| `Contratos.razor` | `/contratos`, `/contratos/{EmpleadoIdParam:int?}` | Lista + modal |
| **DOCUMENTOS** | | |
| `Documentos.razor` | `/documentos`, `/documentos/{EmpleadoIdParam:int}` | Lista por empleado |
| **PERMISOS/AUSENCIAS** | | |
| `Permisos.razor` | `/permisos`, `/permisos/{PermisoIdParam:int?}` | Lista + modal |
| `SeguimientoPermisos.razor` | `/seguimiento-permisos` | Panel con **tabs internos** |
| `Incapacidades.razor` | `/incapacidades` | Lista con **tabs internos** |
| `Vacaciones.razor` | `/vacaciones`, `/vacaciones/{VacacionIdParam:int?}` | Lista + modal |
| **CONTROL DIARIO** | | |
| `ControlDiario.razor` | `/control-diario`, `/control-diario/{FechaParam}` | Registro por fecha |
| `ControlDiarioWizard.razor` | `/control-diario-wizard` | Wizard |
| **PROYECTOS** | | |
| `ProyectoDetalle.razor` | `/proyecto/{ProyectoId:int}` | Usa `?tab=` query param |
| `ReporteProyecto.razor` | `/reporte-proyecto/{ProyectoId:int}` | Usa `?inicio=&fin=` |
| `DashboardProductividad.razor` | `/dashboard-productividad` | Dashboard |
| **NÓMINA/PRESTACIONES** | | |
| `Nomina.razor` | `/nomina` | Lista |
| `Prestaciones.razor` | `/prestaciones` | Lista |
| **REPORTES** | | |
| `Reportes.razor` | `/reportes` | Lista con **tabs internos** |
| **ADMINISTRACIÓN** | | |
| `Usuarios.razor` | `/usuarios`, `/usuarios/{UsuarioIdParam:int?}` | Lista + modal |
| `Catalogos.razor` | `/catalogos` | **Tabs internos** (no persiste en URL) |
| `Configuracion.razor` | `/configuracion` | **Tabs internos** (no persiste en URL) |
| `ConfiguracionLegal.razor` | `/configuracion-legal` | Página dedicada |
| `Festivos.razor` | `/festivos` | Lista |
| `Auditoria.razor` | `/auditoria` | Lista |
| **UTILIDADES** | | |
| `ImageViewer.razor` | `/image-viewer` | Visor |
| `Weather.razor` | `/weather` | Demo |
| `Counter.razor` | `/counter` | Demo |

**Total:** 29 archivos con rutas, 38 directivas `@page`

---

## 2. Páginas con Tabs Internos (NO persisten en URL)

### 2.1 Lista de páginas problemáticas

| Página | Tabs | Variable | ¿Lee de URL? |
|--------|------|----------|--------------|
| `EmpleadoExpediente.razor` | datos, documentos, contratos, seguridad | `activeTab` | ❌ NO |
| `Catalogos.razor` | departamentos, cargos, tiposPermiso, proyectos, actividades | `activeTab` | ❌ NO |
| `Configuracion.razor` | sistema, notificaciones, respaldos, seguridad, mantenimiento | `seccionActiva` | ❌ NO |
| `Incapacidades.razor` | activas, transcripcion, cobro, historial | `tabActivo` | ❌ NO |
| `SeguimientoPermisos.razor` | documentos, compensacion, descuentos, vencidos | `tabActivo` | ❌ NO |
| `ReporteProyecto.razor` | categoria, actividad, empleado, diario | `tabActivo` | ❌ NO |
| `ProyectoDetalle.razor` | (múltiples) | usa `?tab=` | ✅ SÍ |

**Impacto:** Al refrescar (F5), el usuario vuelve al tab por defecto. No se puede compartir URL específica.

---

## 3. Patrones de Navegación Actuales

### 3.1 Patrón: Lista con parámetro opcional para edición
```
/modulo                          → Lista
/modulo/{id}                     → Mismo componente, abre modal de edición
```
**Usado en:** Empleados, Contratos, Permisos, Vacaciones, Usuarios, Documentos

### 3.2 Patrón: Ruta jerárquica completa
```
/empleados/{id}/expediente       → Página dedicada de detalle
```
**Usado en:** EmpleadoExpediente

### 3.3 Patrón: Query string para tabs (parcial)
```
/proyecto/{id}?tab=cuadrilla
/catalogos?tab=proyectos
```
**Usado en:** ProyectoDetalle (lee), DashboardProductividad (escribe pero Catalogos no lee)

### 3.4 Patrón: Query string para fechas
```
/reporte-proyecto/{id}?inicio=2025-01-01&fin=2025-01-31
```
**Usado en:** ReporteProyecto

---

## 4. Componentes que Afectan la Navegación

### 4.1 Componentes compartidos
- `NavMenu.razor` - Menú lateral con enlaces planos
- `RedirectToLogin.razor` - Redirección a login
- `NotificationBell.razor` - Navegación desde notificaciones
- `KeyboardHandler.razor` - Atajos de teclado (Ej: F2 → Empleados)

### 4.2 Servicios
- `NavigationManager` - Usado en 78 lugares
- `IAuthService` - Valida permisos antes de navegar

---

## 5. Navegaciones Programáticas Importantes

### 5.1 Navegaciones entre módulos (principales)
```csharp
// Desde Home
Navigation.NavigateTo("/empleados")
Navigation.NavigateTo("/permisos")
Navigation.NavigateTo("/contratos")

// Desde Empleados
Navigation.NavigateTo("/empleados/onboarding")              // Nuevo empleado
Navigation.NavigateTo($"/empleados/{id}/expediente")        // Ver expediente

// Desde EmpleadoExpediente
Navigation.NavigateTo($"/documentos/{EmpleadoId}")          // Ir a documentos
Navigation.NavigateTo($"/contratos/{EmpleadoId}")           // Ir a contratos
Navigation.NavigateTo("/empleados")                          // Volver

// Desde EmpleadoOnboarding
Navigation.NavigateTo($"/documentos/{empleado.Id}")         // Después de crear

// Desde Dashboard/Proyectos
Navigation.NavigateTo($"/proyecto/{id}?tab=cuadrilla")
Navigation.NavigateTo($"/catalogos?tab=proyectos&editar={id}")
```

### 5.2 Navegaciones de autenticación
```csharp
Navigation.NavigateTo("/login")
Navigation.NavigateTo("/logged-out", forceLoad: true)
```

---

## 6. Recomendaciones para Fase 2

### 6.1 Decisiones propuestas

| Pregunta | Recomendación | Justificación |
|----------|---------------|---------------|
| ¿Páginas separadas o parámetro Tab? | **Parámetro query `?tab=`** | Menos archivos, más fácil mantener |
| ¿Cómo manejar sub-acciones? | **Híbrido**: `/nuevo` para onboarding, modales para edición inline | Ya funciona así para Empleados |
| ¿Breadcrumbs automáticos? | **Semi-automáticos** basados en ruta + metadata | Más control, mejor UX |
| ¿Compatibilidad con rutas viejas? | **No necesaria** (uso interno) | No hay URLs públicas |

### 6.2 Prioridad de migración

1. **Alta prioridad (afecta UX diaria)**
   - `EmpleadoExpediente.razor` - Tabs no persisten
   - `Catalogos.razor` - Navegación desde otros módulos falla
   
2. **Media prioridad**
   - `Configuracion.razor`
   - `SeguimientoPermisos.razor`
   - `Incapacidades.razor`
   
3. **Baja prioridad**
   - `ReporteProyecto.razor` (solo visualización)

### 6.3 Estructura propuesta para Empleados

```
/empleados                              → Lista
/empleados/nuevo                        → Onboarding (ya existe como /empleados/onboarding)
/empleados/{id}                         → Resumen (redirect a /empleados/{id}?tab=datos)
/empleados/{id}?tab=datos               → Datos personales
/empleados/{id}?tab=documentos          → Documentos
/empleados/{id}?tab=contratos           → Contratos
/empleados/{id}?tab=seguridad-social    → Seguridad social
```

**Cambio clave:** Unificar `EmpleadoExpediente.razor` para leer `?tab=` desde URL.

---

## 7. Archivos a Modificar (estimación)

### Fase 3 - Empleados
- `EmpleadoExpediente.razor` - Agregar `SupplyParameterFromQuery` para tab
- `Empleados.razor` - Ajustar navegación a expediente
- `EmpleadoOnboarding.razor` - Cambiar ruta de `/empleados/onboarding` a `/empleados/nuevo`

### Fase 3 - Catálogos
- `Catalogos.razor` - Agregar lectura de `?tab=`
- `ProyectoDetalle.razor` - Ya soporta (no cambios)
- `DashboardProductividad.razor` - No cambios

### Fase 4 - Componentes de soporte
- Crear `Breadcrumbs.razor`
- Actualizar `NavMenu.razor` para resaltar sección activa
- Crear servicio `INavigationService` para centralizar lógica

---

## 8. Próximos Pasos

1. **Revisar este análisis** y validar recomendaciones
2. **Decidir enfoque** (query params vs rutas separadas)
3. **Crear prompt de implementación** para Fase 3 (Empleados primero)
4. **Implementar en nueva sesión** para contexto fresco

---

*Análisis generado automáticamente - Fase 1 de PROMPT_RUTAS_ABSOLUTAS_NAVEGACION.md*
