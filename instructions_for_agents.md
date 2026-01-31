# 📘 Instrucciones para Agentes de IA - SGRRHH

## 🎯 Metodología

Este proyecto utiliza **Antigravity Rules & Skills** para guiar agentes de IA:

- **Rules (`.agent/rules/`):** Reglas obligatorias o condicionales
- **Skills (`.agent/skills/`):** Guías técnicas on-demand

---

## 📋 Reglas Activas

### Siempre Activas (`always_on`)

| Regla | Cuándo Usar | Contenido |
|-------|-------------|-----------|
| **[git.md](.agent/rules/git.md)** | Apply when making git commits | Conventional Commits, scopes españoles, estrategia branches |
| **[ui-style-strict.md](.agent/rules/ui-style-strict.md)** | Apply when creating/modifying Blazor components | Prohibiciones estrictas: NO `<style>` inline, NO colores arbitrarios |
| **[language.md](.agent/rules/language.md)** | Apply when writing any code, comments, or UI text | TODO en español: variables, métodos, propiedades, comentarios |

### Condicionales (`model_decision`)

| Regla | Cuándo Usar | Contenido |
|-------|-------------|-----------|
| **[build.md](.agent/rules/build.md)** | Apply when compiling the project | Comando obligatorio: `dotnet build -v:m /bl:build.binlog 2>&1 \| Tee-Object build.log` |
| **[architecture.md](.agent/rules/architecture.md)** | Apply when creating projects, classes, or configuring DI | Stack .NET 8, Blazor Server, Dapper, Clean Architecture |

---

## 🛠️ Skills Disponibles

| Skill | Cuándo Usar | Contenido |
|-------|-------------|-----------|
| **[blazor-component](.agent/skills/blazor-component/)** | Crear páginas, tabs, formularios Blazor | Patrones, convenciones, code-behind, ciclo de vida |
| **[build-and-verify](.agent/skills/build-and-verify/)** | Después de modificar código C# o Razor | Comando build, interpretación de resultados |
| **[dapper-repository](.agent/skills/dapper-repository/)** | Crear repositorios, queries SQL, migraciones | Patrón repositorio, transacciones, comandos SQLite |
| **[deploy-ssh](.agent/skills/deploy-ssh/)** | Desplegar a servidores remotos | Scripts Deploy-ToServer.ps1/ps2, SSH/SMB, verificación |
| **[git-workflow](.agent/skills/git-workflow/)** | Hacer commits, crear branches | Conventional Commits, cuándo usar rama vs commit directo |
| **[hospital-ui-style](.agent/skills/hospital-ui-style/)** | Crear o modificar componentes UI, estilos CSS | Variables CSS, clases disponibles, cómo agregar estilos nuevos |
| **[metacognitive-reasoning](.agent/skills/metacognitive-reasoning/)** | Cambios de arquitectura, refactors grandes, debugging complejo | Framework 5 pasos: Descomponer, Resolver, Verificar, Sintetizar, Reflexionar |
| **[playwright-e2e](.agent/skills/playwright-e2e/)** | Crear tests, verificar funcionalidades | Estructura tests E2E, page objects, selectores CSS |

---

## ⚙️ Comandos Esenciales

### Build
```powershell
cd c:\Users\evert\Documents\rrhh\SGRRHH.Local
dotnet build -v:m /bl:build.binlog 2>&1 | Tee-Object build.log
```

### Dev Server
```powershell
dotnet watch --project SGRRHH.Local.Server
```

### Deploy Servidor 1 (192.168.1.248)
```powershell
.\scripts\Deploy-ToServer.ps1
```

### Deploy Servidor 2 (192.168.1.72)
```powershell
.\scripts\Deploy-ToServer2.ps1
```

### SQLite
```powershell
# Ver tablas
sqlite3 "C:\SGRRHH\Data\sgrrhh.db" ".tables"

# Ver esquema
sqlite3 "C:\SGRRHH\Data\sgrrhh.db" ".schema empleados"

# Ejecutar migración
sqlite3 "C:\SGRRHH\Data\sgrrhh.db" < scripts/migration_xxx_v1.sql
```

### Tests E2E
```powershell
cd SGRRHH.Local.Tests.E2E
dotnet test --filter "Category=Smoke"
```

---

## 📂 Estructura del Proyecto

```
SGRRHH.Local/
├── .agent/
│   ├── rules/              # Reglas obligatorias/condicionales
│   │   ├── README.md
│   │   ├── git.md          # always_on
│   │   ├── build.md        # model_decision
│   │   ├── architecture.md # model_decision
│   │   ├── ui-style-strict.md # always_on
│   │   └── language.md     # always_on
│   └── skills/             # Guías técnicas on-demand
│       ├── blazor-component/
│       ├── build-and-verify/
│       ├── dapper-repository/
│       ├── deploy-ssh/
│       ├── git-workflow/
│       ├── hospital-ui-style/
│       ├── metacognitive-reasoning/
│       └── playwright-e2e/
├── SGRRHH.Local.Domain/        # Entidades, Enums, Interfaces
├── SGRRHH.Local.Shared/        # DTOs, Validaciones
├── SGRRHH.Local.Infrastructure/ # Repositorios, Servicios, Data
├── SGRRHH.Local.Server/        # Blazor Server, Components, Pages
├── SGRRHH.Local.Tests.E2E/     # Tests Playwright
└── scripts/                    # Migraciones SQL, PowerShell deploy
```

---

## 🏗️ Stack Tecnológico

| Tecnología | Versión | Propósito |
|------------|---------|-----------|
| .NET | 8.0 | Framework |
| Blazor Server | 8.0 | UI |
| Dapper | 2.1+ | ORM |
| SQLite | 3.x | Base de datos |
| Playwright | Latest | Tests E2E |

---

## 🎨 Estilo UI: "Hospital"

- **Fuente:** Courier New, monospace
- **Colores:** Blanco/negro base, rojo (#CC0000) error, verde (#006600) éxito
- **Archivo:** `SGRRHH.Local.Server/wwwroot/css/hospital.css`
- **Prohibido:** Estilos inline, colores arbitrarios

---

## 🌍 Idioma

> [!IMPORTANT]
> **TODO** en español: código, comentarios, UI, mensajes de error.

Nomenclatura C#:
- Variables: `camelCase` → `empleadoActual`
- Métodos: `PascalCase` → `CargarEmpleados()`
- Propiedades: `PascalCase` → `NombreCompleto`

---

## ✅ Checklist Pre-Deploy

Antes de desplegar a producción:

- [ ] ✅ `dotnet build` sin errores
- [ ] ✅ Tests E2E pasando
- [ ] ✅ No hay `console.log` o código debug
- [ ] ✅ Commit siguiendo Conventional Commits
- [ ] ✅ Estilos UI cumplen regla estricta
- [ ] ✅ Migraciones SQL aplicadas en local
- [ ] ✅ Backup de BD en servidor

---

## 📖 Referencias Rápidas

### Conventional Commits

```
tipo(scope): descripción breve

Tipos: feat, fix, refactor, style, docs, chore, test
Scopes: empleados, contratos, vacaciones, ui, db, deploy, etc.
```

### Clean Architecture Layers

```
Domain (núcleo) ← Shared ← Infrastructure ← Server (UI)
```

### Dapper Query Básico

```csharp
using var conn = GetConnection();
return await conn.QueryAsync<Dto>(@"
    SELECT id AS Id, nombre AS Nombre 
    FROM tabla WHERE activo = 1");
```

---

*Última actualización: 2026-01-28*
