# 📁 PROMPTS DE REFACTORIZACIÓN - COMPONENTES GRANDES

## 🎯 Objetivo

Este directorio contiene **6 prompts estructurados** para refactorizar los 5 componentes más grandes de tu aplicación SGRRHH, reduciendo aproximadamente **7,934 líneas a ~1,550 líneas** (78% de reducción).

---

## 📋 Archivos Creados

### 1. **PROMPT_REFACTOR_COORDINACION_MAESTRO.md** ⭐ EMPEZAR AQUÍ
Documento maestro que coordina los 5 agentes para trabajar en paralelo sin conflictos.
- Matriz de archivos exclusivos por agente
- Plan de coordinación y dependencias
- Checklist de integración final
- Métricas de éxito globales

### 2. **PROMPT_REFACTOR_AGENTE_1_EMPLEADO_ONBOARDING.md**
Refactorización de `EmpleadoOnboarding.razor` (1,843 líneas → ~300)
- Wizard de alta de empleados
- Crea 7 componentes reutilizables
- Crea ValidationHelpers.cs
- **Duración:** 2-3 días

### 3. **PROMPT_REFACTOR_AGENTE_2_SCANNER_MODAL.md**
Refactorización de `ScannerModal.razor` (1,592 líneas → ~250)
- Modal de escaneo de documentos
- Crea 7 componentes de scanner
- Crea ImageTransformationService
- **Duración:** 3-4 días

### 4. **PROMPT_REFACTOR_AGENTE_3_EMPLEADO_EXPEDIENTE.md**
Refactorización de `EmpleadoExpediente.razor` (1,445 líneas → ~200)
- Expediente completo del empleado con tabs
- Crea 6 componentes + reutiliza del Agente 1
- Crea StringHelpers.cs
- **Duración:** 2-3 días

### 5. **PROMPT_REFACTOR_AGENTE_4_PERMISOS.md**
Refactorización de `Permisos.razor` (1,513 líneas → ~250)
- Sistema de gestión de permisos laborales
- Crea 7 componentes de permisos
- Crea PermisoCalculationService
- **Duración:** 2-3 días

### 6. **PROMPT_REFACTOR_AGENTE_5_CONTROL_DIARIO.md** 🔴 CRÍTICO
Refactorización de `ControlDiario.razor` (1,541 líneas → ~300)
- Control diario de asistencia (componente más crítico)
- Crea 8 componentes optimizados
- Crea RegistroDiarioService
- **Duración:** 3-4 días
- **Nota:** Requiere optimización de performance

---

## 🚀 Cómo Usar Estos Prompts

### Opción 1: Trabajo Paralelo (5 Agentes Simultáneos)
1. Leer primero `PROMPT_REFACTOR_COORDINACION_MAESTRO.md`
2. Asignar un prompt a cada agente/desarrollador
3. Cada uno trabaja en su componente de forma independiente
4. Al finalizar todos, ejecutar fase de integración

### Opción 2: Trabajo Secuencial (1 Agente)
1. Leer primero `PROMPT_REFACTOR_COORDINACION_MAESTRO.md`
2. Ejecutar prompts en este orden recomendado:
   - **Primero:** Agente 1 (crea componentes que otros reutilizan)
   - **Segundo:** Agentes 2, 4, 5 (independientes)
   - **Tercero:** Agente 3 (puede reutilizar componentes del Agente 1)

### Opción 3: Priorizar Críticos
1. **Día 1-4:** Agente 5 (ControlDiario) - El más crítico y complejo
2. **Día 5-7:** Agente 1 (EmpleadoOnboarding) - Crea base reutilizable
3. **Día 8-10:** Agente 3 (EmpleadoExpediente) - Reutiliza del Agente 1
4. **Día 11-13:** Agente 4 (Permisos)
5. **Día 14-17:** Agente 2 (ScannerModal)

---

## 📊 Métricas Esperadas

### Antes de la Refactorización
```
EmpleadoOnboarding.razor    1,843 líneas
ScannerModal.razor          1,592 líneas
EmpleadoExpediente.razor    1,445 líneas
Permisos.razor              1,513 líneas
ControlDiario.razor         1,541 líneas
─────────────────────────────────────────
TOTAL:                      7,934 líneas
```

### Después de la Refactorización
```
EmpleadoOnboarding.razor      ~300 líneas (-84%)
ScannerModal.razor            ~250 líneas (-84%)
EmpleadoExpediente.razor      ~200 líneas (-86%)
Permisos.razor                ~250 líneas (-83%)
ControlDiario.razor           ~300 líneas (-80%)
─────────────────────────────────────────
TOTAL:                      ~1,300 líneas (-84%)

COMPONENTES NUEVOS:            ~37 componentes
SERVICIOS NUEVOS:              ~3 servicios
```

---

## 🎯 Estructura de Cada Prompt

Todos los prompts siguen la misma estructura rigurosa:

### 📊 FASE 1: INVESTIGACIÓN (2-4 horas)
- Análisis estructural del componente
- Búsqueda de redundancias
- Análisis de dependencias
- Revisión de skills del proyecto

### 🗺️ FASE 2: PLANEACIÓN (2-4 horas)
- Diseño de arquitectura de componentes
- Plan de migración de código
- Identificación de código a consolidar
- Plan de pruebas detallado

### ⚙️ FASE 3: EJECUCIÓN CONTROLADA (8-16 horas)
- Creación de componentes uno por uno
- Checkpoint de compilación después de cada paso
- Refactorización del componente principal
- Consolidación de redundancias
- Pruebas exhaustivas

### 📝 FASE 4: DOCUMENTACIÓN (1-2 horas)
- ANALISIS_[COMPONENTE].md
- PLAN_ARQUITECTURA_[COMPONENTE].md
- TEST_PLAN_[COMPONENTE].md
- RESULTADO_PRUEBAS_[COMPONENTE].md
- REFACTOR_SUMMARY_[COMPONENTE].md

---

## ✅ Características de los Prompts

### ✨ Investigación Exhaustiva
- Análisis línea por línea del componente
- Identificación de redundancias con líneas específicas
- Mapeo completo de dependencias

### 🎯 Planeación Detallada
- Diagramas de arquitectura de componentes
- Especificación de props/parámetros
- Tabla de migración de código con líneas origen/destino

### 🔒 Ejecución Segura
- Checkpoints de compilación obligatorios
- Backups automáticos
- Validación paso por paso
- NO permite saltar pasos

### 📋 Consolidación de Redundancias
- Identifica código duplicado
- Propone helpers/servicios compartidos
- Muestra ANTES/DESPUÉS de cada consolidación

### ✅ Validación Completa
- Checklist de funcionalidad
- Pruebas de regresión
- Validación de performance (especialmente ControlDiario)

---

## 🔒 Reglas de No Interferencia

Cada agente tiene **archivos exclusivos** que puede modificar:

```
AGENTE 1: Components/Forms/*
AGENTE 2: Components/Scanner/* + ImageTransformationService
AGENTE 3: Components/Expediente/* + Components/Tabs/*
AGENTE 4: Components/Permisos/* + PermisoCalculationService
AGENTE 5: Components/ControlDiario/* + RegistroDiarioService
```

### ❌ Archivos Prohibidos para TODOS
- Componentes en `Shared/` existentes (solo leer)
- Entidades en `Domain/Entities/` (solo leer)
- Repositorios (solo leer)
- Archivos de otros agentes

---

## 🎁 Beneficios de Esta Refactorización

### 1. Mantenibilidad
- Componentes pequeños y enfocados
- Fácil encontrar y corregir bugs
- Código auto-documentado

### 2. Reutilización
- 37 componentes nuevos reutilizables
- Helpers compartidos (ValidationHelpers, StringHelpers)
- Servicios de negocio centralizados

### 3. Testing
- Cada componente se puede probar individualmente
- Menor acoplamiento = tests más fáciles

### 4. Trabajo en Equipo
- Múltiples desarrolladores sin conflictos
- Responsabilidades claras

### 5. Performance
- Renderizado selectivo (especialmente ControlDiario)
- Optimización de queries (batch loading)
- Componentes con ShouldRender() optimizado

---

## ⚠️ Notas Importantes

### 🔴 Componentes Críticos
- **ControlDiario:** El más complejo, requiere atención especial a performance
- **EmpleadoOnboarding:** Crea base reutilizable, hacerlo bien desde el inicio

### 🟡 Dependencias
- **Agente 3** puede reutilizar componentes de **Agente 1** (no bloqueante)
- **Agentes 2, 4, 5** son 100% independientes

### 🟢 Compilación
- **OBLIGATORIO:** Compilar después de cada componente creado
- **Comando:** `dotnet build SGRRHH.Local/SGRRHH.Local.Server/SGRRHH.Local.Server.csproj`

---

## 📞 Próximos Pasos

### 1. Preparación (30 min)
- [ ] Leer `PROMPT_REFACTOR_COORDINACION_MAESTRO.md` completamente
- [ ] Decidir estrategia (paralelo vs secuencial)
- [ ] Crear backup de repositorio
- [ ] Crear rama de refactorización: `git checkout -b refactor/componentes-grandes`

### 2. Ejecución (2-3 semanas)
- [ ] Asignar/ejecutar prompts según estrategia elegida
- [ ] Mantener archivo `REFACTOR_STATUS.md` actualizado
- [ ] Compilar frecuentemente

### 3. Integración (1 día)
- [ ] Merge de todos los cambios
- [ ] Pruebas de integración
- [ ] Pruebas de regresión
- [ ] Documentación final consolidada

### 4. Deploy
- [ ] Code review
- [ ] Merge a main/master
- [ ] Deploy a producción

---

## 📚 Recursos Adicionales

### Skills del Proyecto
- `.cursor/skills/blazor-component/SKILL.md`
- `.cursor/skills/hospital-ui-style/SKILL.md`
- `.cursor/skills/build-and-verify/SKILL.md`

### Documentación
- `architecture.md`
- `GLOSARIO_DOMINIO.md`
- `CHANGELOG.md`

---

## 🎉 ¡Listo para Empezar!

1. Abre `PROMPT_REFACTOR_COORDINACION_MAESTRO.md`
2. Elige tu estrategia de ejecución
3. Comienza con el prompt del Agente 1 (o el que elijas)
4. Sigue las fases: Investigación → Planeación → Ejecución → Documentación

**¡Mucho éxito con la refactorización!** 🚀

---

**Creado:** 2026-01-16  
**Autor:** Cursor AI Assistant  
**Versión:** 1.0
