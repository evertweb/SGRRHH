---
trigger: always_on
description: Apply when making git commits. Enforces Conventional Commits with Spanish scopes and branch strategy (direct to main for small changes, feature branches for large).
---

# Git Workflow - SGRRHH

## Regla Principal

Esta regla **siempre está activa** y se aplica cuando se realiza cualquier commit o se trabaja con Git.

## Formato de Commits: Conventional Commits

Todos los commits deben seguir el formato:

```
tipo(scope): descripción breve

[cuerpo opcional]
```

### Tipos Permitidos

| Tipo | Uso |
|------|-----|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de bug |
| `refactor` | Mejora de código sin cambio de funcionalidad |
| `style` | Cambios de CSS/formato |
| `docs` | Documentación |
| `chore` | Tareas de mantenimiento |
| `test` | Agregar o modificar tests |

### Scopes del Proyecto

| Scope | Área |
|-------|------|
| `empleados` | Gestión de empleados |
| `contratos` | Contratos laborales |
| `vacaciones` | Solicitudes de vacaciones |
| `permisos` | Permisos laborales |
| `incapacidades` | Incapacidades médicas |
| `dotacion` | Dotación y EPP |
| `control-diario` | Control diario de actividades |
| `inventario` | Inventario y movimientos |
| `auth` | Autenticación y permisos |
| `ui` | Componentes UI generales |
| `db` | Migraciones y cambios de BD |
| `deploy` | Scripts de deployment |
| `config` | Configuración del proyecto |
| `vacantes` | Gestión de vacantes de empleo |
| `aspirantes` | Gestión de candidatos/aspirantes |
| `hoja-vida` | PDFs de hoja de vida inteligente |
| `contratacion` | Proceso de contratación |
| `prompts` | Prompts de delegación para agentes |

### Ejemplos de Commits Correctos

```bash
feat(empleados): agregar campo Predio a actividades
fix(vacaciones): corregir cálculo de días disponibles
refactor(ui): extraer componente SelectorEstado reutilizable
style(expediente): aplicar estilos hospital.css a formularios
docs(readme): actualizar instrucciones de instalación
chore(deps): actualizar Dapper a versión 2.1.0
test(e2e): agregar test para aprobación de empleados
```

## Estrategia de Branches

### Cambios Menores → Commit Directo a `main`

Aplica cuando:
- Corrección de typos o errores pequeños
- Ajustes de CSS/estilos
- Fix de bugs simples (1-2 archivos)
- Cambios que no afectan funcionalidad crítica

```bash
git add -A
git commit -m "fix(ui): corregir alineación de botones en modal"
git push origin main
```

### Cambios Grandes → Feature Branch

Crear rama cuando:
- Tocar **más de 5 archivos** relacionados
- Modificar **estructura de BD** (nuevas tablas, columnas)
- Agregar **nuevos servicios o repositorios**
- Cambios que pueden **romper funcionalidad existente**
- Requiere **pruebas extensivas**

```bash
# Crear rama
git checkout main
git pull origin main
git checkout -b feature/nombre-descriptivo

# Trabajar (múltiples commits permitidos)
git add -A
git commit -m "feat(módulo): paso 1 de N"

# Push de la rama
git push origin feature/nombre-descriptivo

# Merge cuando esté listo
git checkout main
git merge feature/nombre-descriptivo
git push origin main

# Eliminar rama
git branch -d feature/nombre-descriptivo
git push origin --delete feature/nombre-descriptivo
```

## Nombres de Ramas

| Prefijo | Uso |
|---------|-----|
| `feature/` | Nuevas funcionalidades |
| `refactor/` | Reestructuración de código |
| `fix/` | Correcciones complejas |
| `db/` | Cambios de base de datos |

Ejemplo: `feature/wizard-control-diario`, `refactor/expediente-components`, `db/migration-predio`

## Antes de Push

Checklist obligatorio:

- [ ] ✅ `dotnet build` sin errores
- [ ] ✅ Pruebas manuales de la funcionalidad afectada
- [ ] ✅ No hay `console.log` o código de debug
- [ ] ✅ Commit message sigue Conventional Commits
- [ ] ✅ Scope es correcto según la tabla

## Revertir Cambios (Emergencias)

```bash
# Ver historial
git log --oneline -10

# Revertir último commit (mantiene cambios en staging)
git reset --soft HEAD~1

# Revertir último commit (descarta cambios)
git reset --hard HEAD~1

# Revertir commit específico (crea nuevo commit de reversión)
git revert <commit-hash>
```

### Situación: Subí algo que rompe producción

```bash
git revert HEAD
git push origin main
```

### Situación: Necesito volver a estado limpio

```bash
git fetch origin
git reset --hard origin/main
```

> [!CAUTION]
> `git reset --hard` **elimina cambios locales**. Usar con extremo cuidado.

---

**Última actualización:** 2026-01-28