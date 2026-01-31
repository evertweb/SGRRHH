# SGRRHH - Sistema de Gestión RRHH

Este es un proyecto .NET 8 con Blazor Server que gestiona recursos humanos. Utiliza Clean Architecture, Dapper como ORM, SQLite como base de datos y un sistema de estilos CSS personalizado tipo hospital.

## Arquitectura del Proyecto

| Capa | Proyecto | Responsabilidad |
|------|----------|-----------------|
| Domain | `SGRRHH.Local.Domain` | Entidades, Enums, Interfaces |
| Shared | `SGRRHH.Local.Shared` | DTOs, Validaciones |
| Infrastructure | `SGRRHH.Local.Infrastructure` | Repositorios, Servicios |
| Presentation | `SGRRHH.Local.Server` | UI Blazor, Pages, Components |

**Stack Tecnológico:**
- .NET 8
- Blazor Server
- Dapper (NO Entity Framework)
- SQLite
- CSS personalizado (hospital.css)

## Reglas Obligatorias

### Idioma: Todo en Español

> [!IMPORTANT]  
> **TODO** el código, comentarios, UI y mensajes deben estar en **español**.

**Nomenclatura obligatoria:**
- Variables: `camelCase` → `mostrarFormulario`, `mensajeError`
- Métodos: `PascalCase` → `CargarEmpleadosAsync()`, `ValidarFormulario()`
- Propiedades: `PascalCase` → `EmpleadoId`, `NombreCompleto`
- Clases: `PascalCase` → `Empleado`, `ContratoLaboral`
- Comentarios: siempre en español
- UI: botones, labels, mensajes en español
- Logs: mensajes en español

**Ejemplos correctos:**
```csharp
// ✅ CORRECTO
private bool mostrarFormulario = false;
private async Task CargarEmpleadosAsync() { }
public string NombreCompleto { get; set; }

// ❌ INCORRECTO  
private bool showForm = false;
private async Task LoadEmployeesAsync() { }
public string FullName { get; set; }
```

### Git Workflow: Conventional Commits

**Formato obligatorio:** `tipo(scope): descripción breve`

**Tipos permitidos:** feat, fix, refactor, style, docs, chore, test

**Scopes del proyecto:** empleados, contratos, vacaciones, permisos, incapacidades, dotacion, control-diario, inventario, auth, ui, db, deploy, config

**Estrategia de branches:**
- **Cambios menores** → commit directo a `main`
- **Cambios grandes** (>5 archivos, BD, nuevos servicios) → feature branch

**Ejemplos de commits correctos:**
```bash
feat(empleados): agregar campo Predio a actividades
fix(vacaciones): corregir cálculo de días disponibles
refactor(ui): extraer componente SelectorEstado reutilizable
style(expediente): aplicar estilos hospital.css a formularios
```

**Checklist obligatorio antes de push:**
- ✅ `dotnet build` sin errores
- ✅ Pruebas manuales de la funcionalidad afectada
- ✅ No hay `console.log` o código de debug
- ✅ Commit message sigue Conventional Commits

### Estilos UI: Prohibición Estricta

> [!CAUTION]
> **REGLAS ESTRICTAMENTE PROHIBIDAS - VIOLACIÓN = ERROR CRÍTICO:**

❌ **NUNCA** crear bloques `<style>` inline en componentes `.razor`  
❌ **NUNCA** usar atributo `style="..."` en elementos HTML  
❌ **NUNCA** usar colores arbitrarios (#00ff00, rgb(), etc.)  
❌ **NUNCA** crear archivos CSS específicos por componente  
✅ **SIEMPRE** usar las clases CSS existentes en `hospital.css`

**Archivo de estilos:** `SGRRHH.Local.Server/wwwroot/css/hospital.css`

**Variables CSS obligatorias:**
```css
var(--color-bg)           /* #FFFFFF - Fondo principal */
var(--color-text)         /* #000000 - Texto principal */
var(--color-error)        /* #CC0000 - Errores */
var(--color-success)      /* #006600 - Éxito */
var(--font-family)        /* 'Courier New', monospace */
```

**Clases disponibles:** `.btn-primary`, `.modal-overlay`, `.error-block`, `.success-block`, `.campo-form`, `.data-table`, etc.

## Referencias Contextuales

Para tareas específicas, el sistema puede cargar estas guías adicionales:

### Skills Técnicos On-Demand
- **`blazor-component`** - Patrones para crear componentes Blazor, páginas y formularios
- **`dapper-repository`** - Implementación de repositorios con Dapper y SQLite  
- **`hospital-ui-style`** - Guía completa de estilos CSS disponibles
- **`git-workflow`** - Flujo detallado de Git y manejo de branches
- **`deploy-ssh`** - Deploy a servidores de producción vía SSH/SMB
- **`build-and-verify`** - Procesos de build y verificación
- **`playwright-e2e`** - Testing end-to-end automatizado

### Reglas Condicionales
- **Clean Architecture** - Aplicar cuando se crean proyectos, clases o se configura DI
- **Build obligatorio** - Compilar con `dotnet build -v:m /bl:build.binlog` antes de commits en cambios de código

## Módulos Principales

- **Empleados** - Gestión de empleados y expedientes
- **Contratos** - Contratos laborales  
- **Vacaciones** - Solicitudes de vacaciones
- **Permisos** - Permisos laborales
- **Incapacidades** - Incapacidades médicas
- **Dotación** - Dotación y EPP
- **Control Diario** - Control diario de actividades
- **Inventario** - Inventario y movimientos

## Comandos Importantes

```bash
# Build obligatorio
dotnet build -v:m /bl:build.binlog 2>&1 | Tee-Object build.log

# Deploy a producción
.\scripts\Deploy-ToServer.ps1

# Ejecutar tests E2E
npx playwright test
```

---

**Proyecto:** SGRRHH  
**Actualizado:** 29 enero 2026