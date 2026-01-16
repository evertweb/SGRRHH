# PROMPT: Implementar Sistema de Rutas con Query Parameters para Tabs

> **Tipo:** Implementación de arquitectura de navegación  
> **Prioridad:** Media  
> **Complejidad:** Media  
> **Prerrequisito:** Leer este prompt completo antes de implementar

---

## 1. Contexto

El sistema SGRRHH.Local tiene páginas con tabs internos que **NO persisten en la URL**. Esto causa:
- Al refrescar (F5), el usuario vuelve al tab por defecto
- No se pueden compartir enlaces a secciones específicas
- El historial del navegador no funciona correctamente

### Análisis previo:
- Ver [docs/ANALISIS_RUTAS_NAVEGACION.md](SGRRHH.Local/docs/ANALISIS_RUTAS_NAVEGACION.md) para inventario completo

---

## 2. Decisiones de Diseño (Fase 2 Completada)

### 2.1 Enfoque elegido: Query Parameters

**Patrón a usar:**
```
/modulo/{id}?tab=nombre-tab
```

**Razones:**
- Menos archivos .razor que mantener
- Ya existe implementación de referencia en `ProyectoDetalle.razor`
- Compatible con el breadcrumb existente en `MainLayout.razor`
- Permite mantener estado de otros parámetros (fechas, filtros)

### 2.2 Convención de nombres

| Elemento | Formato | Ejemplo |
|----------|---------|---------|
| Rutas | kebab-case | `/seguridad-social` |
| Parámetros de ruta | PascalCase | `{EmpleadoId:int}` |
| Query params | kebab-case | `?tab=seguridad-social` |
| Variables C# | camelCase | `tabActiva` |

### 2.3 Árbol de rutas propuesto

```
EMPLEADOS
├── /empleados                              → Lista
├── /empleados/nuevo                        → Onboarding (renombrar de /empleados/onboarding)
├── /empleados/{id}                         → Redirect a /empleados/{id}?tab=datos
└── /empleados/{id}/expediente?tab=X        → Expediente con tabs
    ├── ?tab=datos                          → Datos personales (default)
    ├── ?tab=documentos                     → Documentos
    ├── ?tab=contratos                      → Contratos
    └── ?tab=seguridad-social               → Seguridad social

CATÁLOGOS
├── /catalogos?tab=X
    ├── ?tab=departamentos                  → (default)
    ├── ?tab=cargos
    ├── ?tab=tipos-permiso
    ├── ?tab=proyectos
    └── ?tab=actividades

CONFIGURACIÓN
├── /configuracion?tab=X
    ├── ?tab=sistema                        → (default)
    ├── ?tab=notificaciones
    ├── ?tab=respaldos
    ├── ?tab=seguridad
    └── ?tab=mantenimiento

INCAPACIDADES
├── /incapacidades?tab=X
    ├── ?tab=activas                        → (default)
    ├── ?tab=transcripcion
    ├── ?tab=cobro
    └── ?tab=historial

SEGUIMIENTO PERMISOS
├── /seguimiento-permisos?tab=X
    ├── ?tab=documentos                     → (default)
    ├── ?tab=compensacion
    ├── ?tab=descuentos
    └── ?tab=vencidos

REPORTE PROYECTO
├── /reporte-proyecto/{id}?tab=X
    ├── ?tab=categoria                      → (default)
    ├── ?tab=actividad
    ├── ?tab=empleado
    └── ?tab=diario
```

---

## 3. Implementación por Orden de Prioridad

### 3.1 FASE A: EmpleadoExpediente (ALTA PRIORIDAD)

**Archivo:** `SGRRHH.Local.Server/Components/Pages/EmpleadoExpediente.razor`

**Cambios requeridos:**

#### 1. Agregar parámetro de query (después de `[Parameter]`):
```csharp
[Parameter] public int EmpleadoId { get; set; }

[SupplyParameterFromQuery(Name = "tab")]
public string? TabQuery { get; set; }
```

#### 2. En `OnInitializedAsync` o `OnParametersSet`, leer el tab:
```csharp
protected override void OnParametersSet()
{
    // Establecer tab desde URL, o usar default
    if (!string.IsNullOrEmpty(TabQuery))
    {
        activeTab = TabQuery;
    }
}
```

#### 3. Modificar `SetActiveTab` para actualizar URL:
```csharp
private void SetActiveTab(string tab)
{
    activeTab = tab;
    // Actualizar URL sin recargar la página
    var uri = Navigation.GetUriWithQueryParameter("tab", tab);
    Navigation.NavigateTo(uri, replace: true);
}
```

#### 4. Actualizar navegaciones entrantes (en `Empleados.razor`):
```csharp
// Cambiar de:
Navigation.NavigateTo($"/empleados/{empleado.Id}/expediente");

// A (opcionalmente especificar tab):
Navigation.NavigateTo($"/empleados/{empleado.Id}/expediente?tab=datos");
```

---

### 3.2 FASE B: Catalogos (ALTA PRIORIDAD)

**Archivo:** `SGRRHH.Local.Server/Components/Pages/Catalogos.razor`

**Problema actual:** `DashboardProductividad` navega a `/catalogos?tab=proyectos` pero `Catalogos` no lee ese parámetro.

**Cambios requeridos:**

#### 1. Agregar inyección y parámetro:
```csharp
@inject NavigationManager Navigation

@code {
    [SupplyParameterFromQuery(Name = "tab")]
    public string? TabQuery { get; set; }
    
    private string activeTab = "departamentos";
```

#### 2. Leer tab inicial:
```csharp
protected override async Task OnInitializedAsync()
{
    // ... código existente de autenticación ...
    
    // Leer tab de URL
    if (!string.IsNullOrEmpty(TabQuery))
    {
        activeTab = TabQuery;
    }
    
    // ... resto del código ...
}
```

#### 3. Actualizar URL al cambiar tab:
```csharp
private void ChangeTab(string tab)
{
    activeTab = tab;
    var uri = Navigation.GetUriWithQueryParameter("tab", tab);
    Navigation.NavigateTo(uri, replace: true);
}
```

---

### 3.3 FASE C: Configuracion (MEDIA PRIORIDAD)

**Archivo:** `SGRRHH.Local.Server/Components/Pages/Configuracion.razor`

**Mismo patrón que Catalogos:**

```csharp
[SupplyParameterFromQuery(Name = "tab")]
public string? TabQuery { get; set; }

private string seccionActiva = "sistema";

protected override async Task OnInitializedAsync()
{
    // Leer de URL
    if (!string.IsNullOrEmpty(TabQuery))
    {
        seccionActiva = TabQuery;
    }
    // ... resto ...
}

private void CambiarSeccion(string seccion)
{
    seccionActiva = seccion;
    var uri = Navigation.GetUriWithQueryParameter("tab", seccion);
    Navigation.NavigateTo(uri, replace: true);
}
```

---

### 3.4 FASE D: Incapacidades, SeguimientoPermisos, ReporteProyecto (BAJA PRIORIDAD)

Aplicar el mismo patrón a:
- `Incapacidades.razor` → variable `tabActivo`
- `SeguimientoPermisos.razor` → variable `tabActivo`
- `ReporteProyecto.razor` → variable `tabActivo`

---

## 4. Mejora del Breadcrumb (Opcional)

El breadcrumb actual en `MainLayout.razor` ya lee la ruta. Para mejorar:

### 4.1 Crear diccionario de nombres amigables:

```csharp
private static readonly Dictionary<string, string> _routeNames = new()
{
    { "empleados", "Empleados" },
    { "expediente", "Expediente" },
    { "catalogos", "Catálogos" },
    { "configuracion", "Configuración" },
    { "incapacidades", "Incapacidades" },
    { "seguimiento-permisos", "Seguimiento de Permisos" },
    { "reporte-proyecto", "Reporte de Proyecto" },
    // ... más rutas
};

private string GetBreadcrumb()
{
    var uri = new Uri(Navigation.Uri);
    var path = uri.AbsolutePath.Trim('/');

    if (string.IsNullOrEmpty(path)) 
        return "INICIO";

    var segments = path.Split('/');
    var parts = segments.Select(s => 
        _routeNames.TryGetValue(s.ToLower(), out var name) ? name : s.ToUpper()
    );
    
    return "INICIO > " + string.Join(" > ", parts);
}
```

---

## 5. Patrón de Referencia

**Implementación correcta existente:** `ProyectoDetalle.razor` (líneas 975-1030)

```csharp
[Parameter] public int ProyectoId { get; set; }

[SupplyParameterFromQuery(Name = "tab")]
public string? TabQuery { get; set; }

private string tabActiva = "resumen";

protected override async Task OnInitializedAsync()
{
    // Establecer tab inicial desde query string
    if (!string.IsNullOrEmpty(TabQuery))
    {
        tabActiva = TabQuery;
    }
    
    await CargarDatos();
}
```

---

## 6. Testing

### 6.1 Casos de prueba manuales

| Escenario | Acción | Resultado Esperado |
|-----------|--------|-------------------|
| Navegación directa | Ir a `/empleados/1/expediente?tab=documentos` | Abre en tab Documentos |
| Cambio de tab | Click en tab "Contratos" | URL cambia a `?tab=contratos` |
| Refresh | F5 en `/empleados/1/expediente?tab=seguridad-social` | Mantiene tab Seguridad Social |
| Back/Forward | Cambiar tabs y usar botones del navegador | Navega entre tabs anteriores |
| Tab inválido | Ir a `?tab=invalido` | Usa tab por defecto |
| Sin tab | Ir a `/empleados/1/expediente` | Usa tab por defecto (datos) |

### 6.2 Verificar compilación

```powershell
cd SGRRHH.Local
dotnet build 2>&1 | Select-String -Pattern "error|Build succeeded|Build FAILED"
```

---

## 7. Orden de Ejecución

1. **Empezar con `EmpleadoExpediente.razor`** (es el más usado)
2. **Probar manualmente** los 6 escenarios de testing
3. **Continuar con `Catalogos.razor`** (arregla navegación rota desde Dashboard)
4. **Probar navegación** desde `/dashboard-productividad` → `/catalogos?tab=proyectos`
5. **Implementar resto** según prioridad

---

## 8. Criterios de Éxito

- [ ] EmpleadoExpediente persiste tab en URL
- [ ] Catalogos lee `?tab=` de la URL
- [ ] F5 mantiene el tab activo
- [ ] Botones back/forward funcionan
- [ ] Compilación sin errores
- [ ] Sin regresión en funcionalidad existente

---

## 9. Notas Importantes

- **NO crear archivos .razor separados por tab** - usar query params
- **NO modificar estilos** - usar clases existentes de `hospital.css`
- **Usar `replace: true`** en NavigateTo para no llenar el historial
- **Mantener el tab por defecto** como fallback para URLs sin `?tab=`

---

*Prompt generado: Enero 2026*
*Basado en: PROMPT_RUTAS_ABSOLUTAS_NAVEGACION.md + Análisis Fase 1 y 2*
