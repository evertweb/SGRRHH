# PROMPT: Sistema de Rutas Absolutas para Navegación

> **Tipo:** Refactorización de arquitectura de navegación
> **Prioridad:** Media
> **Complejidad:** Alta (afecta múltiples páginas)

---

## 1. Contexto del Problema

Actualmente la aplicación usa una mezcla de:
- Rutas planas (`/empleados`, `/documentos`, `/nomina`)
- Rutas con parámetros (`/empleados/{id}/expediente`)
- Tabs internos que NO cambian la URL (usuario pierde contexto al refrescar)

**Problemas actuales:**
- No se puede compartir un enlace directo a una sección específica
- Al refrescar la página se pierde el tab activo
- No hay breadcrumbs consistentes
- Historial del navegador no refleja la navegación interna

---

## 2. Objetivo

Implementar un sistema de **rutas absolutas jerárquicas** donde cada vista tenga su propia URL única.

### Estructura propuesta:

```
/modulo                              → Lista principal
/modulo/{id}                         → Detalle/Edición
/modulo/{id}/submodulo               → Sección del detalle
/modulo/{id}/submodulo/accion        → Acción específica
```

### Ejemplo para Empleados:

```
/empleados                                    → Lista de empleados
/empleados/nuevo                              → Crear empleado (onboarding)
/empleados/{id}                               → Resumen/Dashboard del empleado
/empleados/{id}/datos                         → Datos personales
/empleados/{id}/documentos                    → Lista de documentos
/empleados/{id}/documentos/nuevo              → Subir documento
/empleados/{id}/documentos/{docId}            → Ver documento específico
/empleados/{id}/contratos                     → Lista de contratos
/empleados/{id}/contratos/nuevo               → Crear contrato
/empleados/{id}/seguridad-social              → Datos de seguridad social
```

---

## 3. Fases de Implementación

### FASE 1: Investigación (esta sesión)
- [ ] Listar TODAS las rutas actuales (`@page`) en el proyecto
- [ ] Identificar qué páginas usan tabs internos vs rutas
- [ ] Mapear navegaciones con `NavigationManager.NavigateTo()`
- [ ] Identificar componentes compartidos que dependen de rutas
- [ ] Documentar hallazgos

### FASE 2: Diseño de Rutas
- [ ] Definir árbol completo de rutas por módulo
- [ ] Definir convención de nombres (kebab-case: `seguridad-social`)
- [ ] Diseñar componente de Breadcrumbs reutilizable
- [ ] Definir cómo manejar parámetros opcionales
- [ ] Documentar en tabla: Ruta Actual → Ruta Nueva

### FASE 3: Implementación por Módulo
Orden sugerido (de menos a más complejo):

1. **Catálogos** (cargos, departamentos, EPS, etc.)
2. **Usuarios**
3. **Empleados** (el más complejo)
4. **Documentos**
5. **Nómina**
6. **Reportes**

### FASE 4: Componentes de Soporte
- [ ] `Breadcrumbs.razor` - Navegación jerárquica
- [ ] `NavLink` actualizado con rutas activas
- [ ] Actualizar menú lateral si existe
- [ ] Tests de navegación

---

## 4. Decisiones de Diseño a Tomar

| Pregunta | Opciones |
|----------|----------|
| ¿Páginas separadas o una página con parámetro Tab? | A) Archivos .razor separados / B) Un archivo con `@page ".../{Tab?}"` |
| ¿Cómo manejar sub-acciones (nuevo, editar)? | A) Ruta separada `/nuevo` / B) Modal sobre la lista |
| ¿Breadcrumbs automáticos o manuales? | A) Basados en la ruta / B) Definidos por página |
| ¿Mantener compatibilidad con rutas viejas? | A) Redirects 301 / B) Romper rutas viejas |

---

## 5. Ejemplo de Implementación (Empleados)

### Archivos a crear/modificar:

```
Components/Pages/Empleados/
├── Index.razor                    → /empleados (lista)
├── Nuevo.razor                    → /empleados/nuevo
├── Detalle/
│   ├── _Layout.razor              → Layout compartido con header + breadcrumbs
│   ├── Index.razor                → /empleados/{id} (resumen)
│   ├── Datos.razor                → /empleados/{id}/datos
│   ├── Documentos/
│   │   ├── Index.razor            → /empleados/{id}/documentos
│   │   └── Nuevo.razor            → /empleados/{id}/documentos/nuevo
│   ├── Contratos/
│   │   ├── Index.razor            → /empleados/{id}/contratos
│   │   └── Nuevo.razor            → /empleados/{id}/contratos/nuevo
│   └── SeguridadSocial.razor      → /empleados/{id}/seguridad-social
```

### Código ejemplo (Datos.razor):

```razor
@page "/empleados/{EmpleadoId:int}/datos"
@layout EmpleadoDetalleLayout

<PageTitle>Datos - @Empleado?.NombreCompleto</PageTitle>

<!-- Contenido de datos personales -->
```

### Layout compartido (EmpleadoDetalleLayout.razor):

```razor
@inherits LayoutComponentBase

<div class="empleado-detalle">
    <Breadcrumbs Items="@breadcrumbs" />
    <EmpleadoHeader Empleado="@empleado" />
    <NavTabs Items="@tabs" />
    
    @Body
</div>
```

---

## 6. Consideraciones Técnicas

### Blazor Server:
- Cada `@page` crea un componente enrutable
- Se pueden tener múltiples `@page` en un componente
- Los layouts pueden anidarse

### Parámetros de ruta:
```razor
@page "/empleados/{EmpleadoId:int}/documentos/{DocumentoId:int?}"

[Parameter] public int EmpleadoId { get; set; }
[Parameter] public int? DocumentoId { get; set; }  // Opcional
```

### Navegación programática:
```csharp
Navigation.NavigateTo($"/empleados/{id}/documentos");
```

---

## 7. Criterios de Éxito

- [ ] Cada sección tiene su propia URL bookmarkeable
- [ ] F5 (refresh) mantiene el usuario en la misma vista
- [ ] Breadcrumbs visibles y funcionales
- [ ] Historial del navegador funciona (back/forward)
- [ ] No hay regresión en funcionalidad existente
- [ ] Compilación sin errores
- [ ] Tests de navegación pasan

---

## 8. Notas

- **NO crear este archivo en cada fase.** Este prompt es para planificación.
- Ejecutar Fase 1 (investigación) antes de decidir enfoque final.
- Considerar impacto en usuarios actuales (bookmarks existentes).

---

*Prompt creado: Enero 2026*
