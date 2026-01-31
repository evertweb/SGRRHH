---
name: git-workflow
description: Flujo de trabajo Git para SGRRHH. Cambios menores van en commits directos a main, cambios grandes/riesgosos van en ramas feature.
license: MIT
compatibility: opencode
metadata:
  project: SGRRHH
  type: workflow
  git-strategy: conventional-commits
---
# Git Workflow - SGRRHH

## Regla Principal

| Tipo de Cambio | Acción | Ejemplo |
|----------------|--------|---------|
| **Menor/Seguro** | Commit directo a `main` | Corrección de typos, ajustes CSS, fix de bugs simples |
| **Grande/Riesgoso** | Crear rama → PR → Merge | Nuevos módulos, refactors grandes, cambios de BD |

## Cuándo Usar Rama (Branch)

Crear rama cuando el cambio:
- Toca **más de 5 archivos** relacionados
- Modifica **estructura de BD** (nuevas tablas, columnas)
- Agrega **nuevos servicios o repositorios**
- Puede **romper funcionalidad existente**
- Requiere **pruebas extensivas** antes de producir

> [!WARNING]
> Si hay duda, **usar rama**. Es más fácil hacer merge que revertir.

## Flujo para Cambios Menores

```powershell
# 1. Hacer cambios
# 2. Compilar y verificar
dotnet build -v:m /bl:build.binlog 2>&1 | Tee-Object build.log

# 3. Commit descriptivo
git add -A
git commit -m "tipo(modulo): descripcion breve"

# 4. Push
git push origin main
```

### Tipos de Commit:
- `fix` - Corrección de bug
- `feat` - Nueva funcionalidad pequeña
- `refactor` - Mejora de código sin cambio de funcionalidad
- `style` - Cambios de CSS/formato
- `docs` - Documentación
- `chore` - Tareas de mantenimiento

## Flujo para Cambios Grandes (Features)

```powershell
# 1. Crear rama desde main
git checkout main
git pull origin main
git checkout -b feature/nombre-descriptivo

# 2. Hacer cambios (múltiples commits permitidos)
git add -A
git commit -m "feat(modulo): paso 1 de N"

# 3. Compilar y probar
dotnet build -v:m /bl:build.binlog 2>&1 | Tee-Object build.log

# 4. Push de la rama
git push origin feature/nombre-descriptivo

# 5. Crear PR en GitHub/GitLab (opcional pero recomendado)

# 6. Merge cuando esté listo
git checkout main
git merge feature/nombre-descriptivo
git push origin main

# 7. Eliminar rama
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

## Antes de Push a Producción

1. ✅ `dotnet build` sin errores
2. ✅ Pruebas manuales de la funcionalidad afectada
3. ✅ Revisar que no hay `console.log` o código de debug
4. ✅ Commit message descriptivo

## Revertir Cambios

```powershell
# Ver historial
git log --oneline -10

# Revertir último commit (mantiene cambios en staging)
git reset --soft HEAD~1

# Revertir último commit (descarta cambios)
git reset --hard HEAD~1

# Revertir commit específico (crea nuevo commit de reversión)
git revert <commit-hash>
```

## Situaciones de Emergencia

### Subí algo que rompe producción:
```powershell
git revert HEAD
git push origin main
```

### Necesito deshacer TODO y volver a estado limpio:
```powershell
git fetch origin
git reset --hard origin/main
```

> [!CAUTION]
> `git reset --hard` **elimina cambios locales**. Usar con cuidado.