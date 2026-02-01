# 📋 Reglas de Antigravity - SGRRHH

Esta carpeta contiene las **reglas** que guían el comportamiento de agentes de IA en este proyecto.

## 📖 Reglas Disponibles

### 🔄 Siempre Activas (`trigger: always_on`)

Estas reglas se aplican **siempre**, sin excepción:

| Regla | Descripción | Cuándo Usar |
|-------|-------------|-------------|
| **[git.md](git.md)** | Conventional Commits + estrategia de branches | Al hacer cualquier commit |
| **[ui-style-strict.md](ui-style-strict.md)** | Prohibiciones estrictas de estilos inline | Al crear/modificar componentes Blazor |
| **[language.md](language.md)** | Todo en español (código, comentarios, UI) | Al escribir cualquier código |

### 🤔 Activación Condicional (`trigger: model_decision`)

El modelo decide cuándo aplicar estas reglas basándose en el contexto:

| Regla | Descripción | Cuándo Usar |
|-------|-------------|-------------|
| **[build.md](build.md)** | Comando build obligatorio con binary logging | Al compilar el proyecto |
| **[architecture.md](architecture.md)** | Clean Architecture + stack .NET 8 | Al crear proyectos, clases, o configurar DI |

## 📝 Formato de Reglas

Todas las reglas siguen este formato:

```yaml
---
trigger: always_on | model_decision
description: Apply when [situación]... (máx 250 chars)
---

# Contenido de la regla
```

## 🎯 Triggers Explicados

### `always_on`
La regla se carga y aplica en **todos** los contextos. Usar para:
- Prohibiciones estrictas
- Convenciones de código obligatorias
- Validaciones automáticas

### `model_decision`
El modelo evalúa la descripción y decide si aplicar la regla. Usar para:
- Reglas contextuales (compilación, arquitectura)
- Guías que se aplican solo en ciertas tareas
- Validaciones condicionales

## 📚 Ejemplo de Uso

### Commit de Código

**Reglas aplicadas:**
1. `git.md` (always_on) → Valida formato Conventional Commits
2. `language.md` (always_on) → Valida nombres en español
3. `build.md` (model_decision) → Si modificaste `.cs`, ejecuta build
4. `ui-style-strict.md` (always_on) → Si modificaste `.razor`, valida estilos

### Crear Nueva Entidad

**Reglas aplicadas:**
1. `architecture.md` (model_decision) → Valida capas Clean Architecture
2. `language.md` (always_on) → Valida nomenclatura en español
3. `build.md` (model_decision) → Compila después de crear archivos

## 🔗 Ver También

- [**Skills**](../skills/) - Guías técnicas on-demand (Dapper, Blazor, Deploy)
- [**Instructions for Agents**](../../instructions_for_agents.md) - Índice completo

---

**Última actualización:** 2026-01-28
