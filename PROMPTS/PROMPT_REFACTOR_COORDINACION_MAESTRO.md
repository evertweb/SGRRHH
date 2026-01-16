# 🎯 COORDINACIÓN MAESTRO - REFACTORIZACIÓN PARALELA DE 5 COMPONENTES

## 📋 RESUMEN EJECUTIVO

Este documento coordina la refactorización paralela de los 5 componentes más grandes de SGRRHH, asegurando que **5 agentes diferentes** puedan trabajar simultáneamente **sin interferir** entre sí.

---

## 🗺️ MAPA DE COMPONENTES Y AGENTES

| Agente | Componente | Tamaño | Prioridad | Duración | Prompt |
|--------|------------|--------|-----------|----------|--------|
| **Agente 1** | EmpleadoOnboarding.razor | 1,843 líneas | 🔴 CRÍTICA | 2-3 días | `PROMPT_REFACTOR_AGENTE_1_EMPLEADO_ONBOARDING.md` |
| **Agente 2** | ScannerModal.razor | 1,592 líneas | 🟠 ALTA | 3-4 días | `PROMPT_REFACTOR_AGENTE_2_SCANNER_MODAL.md` |
| **Agente 3** | EmpleadoExpediente.razor | 1,445 líneas | 🟠 ALTA | 2-3 días | `PROMPT_REFACTOR_AGENTE_3_EMPLEADO_EXPEDIENTE.md` |
| **Agente 4** | Permisos.razor | 1,513 líneas | 🟠 ALTA | 2-3 días | `PROMPT_REFACTOR_AGENTE_4_PERMISOS.md` |
| **Agente 5** | ControlDiario.razor | 1,541 líneas | 🔴 CRÍTICA | 3-4 días | `PROMPT_REFACTOR_AGENTE_5_CONTROL_DIARIO.md` |

**Total de líneas a refactorizar:** 7,934 líneas  
**Reducción esperada:** ~6,200 líneas (78%)  
**Componentes nuevos a crear:** ~37  
**Servicios nuevos:** ~3

---

## 🔒 MATRIZ DE NO INTERFERENCIA

### Archivos Exclusivos por Agente

```
AGENTE 1 (EmpleadoOnboarding)
├── ✅ EmpleadoOnboarding.razor
├── ✅ Components/Forms/DatosPersonalesForm.razor
├── ✅ Components/Forms/DatosLaboralesForm.razor
├── ✅ Components/Forms/SeguridadSocialForm.razor
├── ✅ Components/Forms/DatosBancariosForm.razor
├── ✅ Components/Forms/ContactoEmpleadoForm.razor
├── ✅ Components/Shared/WizardNavigation.razor
├── ✅ Components/Shared/WizardProgress.razor
└── ✅ Shared/Helpers/ValidationHelpers.cs

AGENTE 2 (ScannerModal)
├── ✅ Components/Shared/ScannerModal.razor
├── ✅ Components/Scanner/ScannerPreview.razor
├── ✅ Components/Scanner/ScannerToolbar.razor
├── ✅ Components/Scanner/ScannerThumbnails.razor
├── ✅ Components/Scanner/ScannerDeviceSelector.razor
├── ✅ Components/Scanner/ScannerProfileSelector.razor
├── ✅ Components/Scanner/ImageEditorTools.razor
├── ✅ Components/Scanner/OcrPanel.razor
└── ✅ Infrastructure/Services/ImageTransformationService.cs

AGENTE 3 (EmpleadoExpediente)
├── ✅ Components/Pages/EmpleadoExpediente.razor
├── ✅ Components/Expediente/EmpleadoHeader.razor
├── ✅ Components/Expediente/EmpleadoInfoCard.razor
├── ✅ Components/Expediente/TabsNavigation.razor
├── ✅ Components/Expediente/DatosGeneralesTab.razor
├── ✅ Components/Expediente/DocumentosTab.razor
├── ✅ Components/Expediente/FotoChangeModal.razor
├── ✅ Components/Tabs/* (TODOS los tabs)
└── ✅ Shared/Helpers/StringHelpers.cs

AGENTE 4 (Permisos)
├── ✅ Components/Pages/Permisos.razor
├── ✅ Components/Permisos/PermisosHeader.razor
├── ✅ Components/Permisos/PermisosFilters.razor
├── ✅ Components/Permisos/PermisosTable.razor
├── ✅ Components/Permisos/PermisoFormModal.razor
├── ✅ Components/Permisos/PermisoAprobacionModal.razor
├── ✅ Components/Permisos/PermisoSeguimientoPanel.razor
├── ✅ Components/Permisos/PermisoCalculadora.razor
└── ✅ Domain/Services/PermisoCalculationService.cs

AGENTE 5 (ControlDiario)
├── ✅ Components/Pages/ControlDiario.razor
├── ✅ Components/ControlDiario/ControlDiarioHeader.razor
├── ✅ Components/ControlDiario/DateNavigator.razor
├── ✅ Components/ControlDiario/FiltrosDiarios.razor
├── ✅ Components/ControlDiario/EmpleadoRow.razor
├── ✅ Components/ControlDiario/ActividadSelector.razor
├── ✅ Components/ControlDiario/RegistroAsistenciaModal.razor
├── ✅ Components/ControlDiario/AccionesMasivasPanel.razor
├── ✅ Components/ControlDiario/ResumenDiarioCard.razor
└── ✅ Domain/Services/RegistroDiarioService.cs
```

### ❌ Archivos PROHIBIDOS para TODOS los Agentes

```
❌ Components/Shared/InputCedula.razor (ya existe - SOLO LEER)
❌ Components/Shared/InputMoneda.razor (ya existe - SOLO LEER)
❌ Components/Shared/InputUpperCase.razor (ya existe - SOLO LEER)
❌ Components/Shared/Modal.razor (ya existe - SOLO LEER)
❌ Components/Shared/DataTable.razor (ya existe - SOLO LEER)
❌ Components/Shared/EstadoBadge.razor (ya existe - SOLO LEER)
❌ Components/Shared/MessageToast.razor (ya existe - SOLO LEER)
❌ Domain/Entities/* (SOLO LEER, NO MODIFICAR sin coordinación)
❌ Infrastructure/Data/* (SOLO LEER, NO MODIFICAR)
```

---

## 🔄 DEPENDENCIAS Y ORDEN DE EJECUCIÓN

### Fase 1: Independientes (PUEDEN EMPEZAR EN PARALELO)
```
┌─────────────┐
│  Agente 2   │  ScannerModal (100% independiente)
│ 3-4 días    │
└─────────────┘

┌─────────────┐
│  Agente 4   │  Permisos (100% independiente)
│ 2-3 días    │
└─────────────┘

┌─────────────┐
│  Agente 5   │  ControlDiario (100% independiente)
│ 3-4 días    │
└─────────────┘
```

### Fase 2: Con Dependencias Leves
```
┌─────────────┐
│  Agente 1   │  EmpleadoOnboarding
│ 2-3 días    │  DEBE terminar PRIMERO
└─────┬───────┘  (crea componentes reutilizables)
      │
      │ REUTILIZA COMPONENTES
      ▼
┌─────────────┐
│  Agente 3   │  EmpleadoExpediente
│ 2-3 días    │  Puede empezar después de Agente 1
└─────────────┘  (pero NO es bloqueante)
```

### Recomendación de Inicio

**ESCENARIO 1: 5 Agentes Simultáneos**
- Todos inician al mismo tiempo
- Agente 3 podrá reutilizar componentes del Agente 1 cuando estén listos
- Si Agente 1 no termina, Agente 3 puede crear sus propios componentes temporalmente

**ESCENARIO 2: Inicio Escalonado**
1. **Día 1:** Iniciar Agentes 1, 2, 5 (los más críticos)
2. **Día 2:** Iniciar Agentes 3, 4 (cuando ya hay contexto)

---

## 📊 MATRIZ DE REUTILIZACIÓN

| Componente | Creador | Reutilizado Por | Tipo |
|------------|---------|-----------------|------|
| **ValidationHelpers.cs** | Agente 1 | Agente 3 | Helper |
| **DatosPersonalesForm** | Agente 1 | Agente 3 | Componente |
| **SeguridadSocialForm** | Agente 1 | Agente 3 | Componente |
| **StringHelpers.cs** | Agente 3 | Todos | Helper |
| **ScannerModal** | Agente 2 | Agente 3 (DocumentosTab) | Componente |
| **InputCedula** | Existente | Agentes 1, 3, 4 | Componente |
| **InputMoneda** | Existente | Agentes 1, 3, 4 | Componente |

---

## 🔍 COORDINACIÓN Y COMUNICACIÓN

### Canales de Comunicación

**Archivo de Estado Global:**
```
PROMPTS/REFACTOR_STATUS.md

# Estado de Refactorización

## Agente 1 - EmpleadoOnboarding
- Estado: EN PROGRESO
- Fase actual: 3.2 - Creando componentes de formulario
- Archivos completados: DatosPersonalesForm.razor ✅
- Bloqueadores: Ninguno
- Última actualización: 2026-01-16 14:30

## Agente 2 - ScannerModal
- Estado: COMPLETADO ✅
- Reducción lograda: 84%
- Componentes creados: 7/7
- Pruebas: PASADAS
- Última actualización: 2026-01-16 12:00

[... otros agentes ...]
```

### Protocolo de Resolución de Conflictos

**SI HAY CONFLICTO DE ARCHIVOS:**

1. **Identificar:** ¿Qué agente tiene exclusividad sobre el archivo?
2. **Consultar:** Revisar matriz de archivos exclusivos
3. **Decidir:**
   - Si el archivo es exclusivo de Agente X → Agente X tiene prioridad
   - Si el archivo es compartido → Crear en carpeta Shared/ con consenso
4. **Documentar:** Actualizar `REFACTOR_STATUS.md`

**EJEMPLO:**
```
CONFLICTO: Agente 1 y Agente 3 necesitan crear "EmpleadoFormBase.razor"

SOLUCIÓN:
1. Verificar matriz → NO está asignado a ninguno
2. Decisión: Crearlo en Shared/ para ambos
3. Agente 1 lo crea primero (es base)
4. Agente 3 lo reutiliza
5. Documentar en REFACTOR_STATUS.md
```

---

## ✅ CHECKLIST DE COORDINACIÓN

### Antes de Comenzar (CADA AGENTE)
```markdown
- [ ] Leer su prompt específico completamente
- [ ] Leer este documento de coordinación
- [ ] Crear REFACTOR_STATUS.md con su entrada
- [ ] Verificar matriz de archivos exclusivos
- [ ] Identificar dependencias con otros agentes
- [ ] Compilar proyecto ANTES de empezar
- [ ] Hacer backup del componente original
```

### Durante el Trabajo (CADA AGENTE)
```markdown
- [ ] Actualizar REFACTOR_STATUS.md cada hora
- [ ] NO modificar archivos fuera de su zona
- [ ] Compilar después de CADA componente creado
- [ ] Documentar problemas en REFACTOR_STATUS.md
- [ ] Si necesita archivo de otro agente, SOLO LEER
- [ ] Comunicar bloqueos inmediatamente
```

### Al Finalizar (CADA AGENTE)
```markdown
- [ ] Compilación final exitosa
- [ ] Todas las pruebas pasadas
- [ ] Documentación completada
- [ ] REFACTOR_SUMMARY creado
- [ ] Actualizar REFACTOR_STATUS.md a "COMPLETADO"
- [ ] NO hacer push sin aprobación final
```

---

## 🚀 PLAN DE INTEGRACIÓN FINAL

Una vez que TODOS los agentes terminen:

### Fase de Integración (1 día)

1. **Merge de Cambios:**
   - Revisar que no haya conflictos
   - Compilar proyecto completo
   - Resolver warnings

2. **Pruebas de Integración:**
   - Probar flujo completo: Onboarding → Expediente → Documentos (con Scanner)
   - Probar Control Diario con Permisos
   - Verificar componentes compartidos funcionan en todos lados

3. **Pruebas de Regresión:**
   - Ejecutar suite completa de pruebas
   - Verificar que NO haya funcionalidad perdida
   - Validar performance general

4. **Documentación Final:**
   - Consolidar todos los REFACTOR_SUMMARY en uno maestro
   - Actualizar architecture.md
   - Crear guía de componentes reutilizables

---

## 📈 MÉTRICAS DE ÉXITO GLOBAL

El proyecto de refactorización se considera **EXITOSO** si:

1. ✅ **Reducción de líneas:** 
   - ANTES: 7,934 líneas
   - DESPUÉS: ~1,550 líneas
   - Reducción: ≥ 78%

2. ✅ **Componentes creados:** 
   - Meta: 37 componentes
   - Mínimo: 30 componentes

3. ✅ **Compilación:** 
   - 0 errores de build
   - Máximo 5 warnings no críticos

4. ✅ **Funcionalidad:** 
   - 100% operativa
   - 0 regresiones

5. ✅ **Performance:** 
   - ControlDiario: ≥50% más rápido
   - Scanner: Sin degradación
   - Otros: Sin degradación

6. ✅ **Pruebas:** 
   - Todas las pruebas individuales pasadas
   - Pruebas de integración pasadas
   - Pruebas de regresión pasadas

---

## 🎯 CRONOGRAMA ESTIMADO

### Semana 1
- **Día 1-2:** Agentes 1, 2, 5 en fase de investigación y planeación
- **Día 3-4:** Agentes 1, 2, 5 en ejecución
- **Día 5:** Agentes 3, 4 inician (investigación)

### Semana 2
- **Día 1-2:** Todos en ejecución
- **Día 3:** Agentes 2, 4 terminan
- **Día 4:** Agentes 1, 3 terminan
- **Día 5:** Agente 5 termina (el más complejo)

### Semana 3
- **Día 1:** Integración y pruebas
- **Día 2:** Correcciones finales
- **Día 3:** Documentación consolidada
- **Día 4:** Aprobación final
- **Día 5:** Deploy/Merge

**DURACIÓN TOTAL ESTIMADA:** 3 semanas (15 días laborables)

---

## 📞 CONTACTO Y SOPORTE

### Responsable del Proyecto
**Usuario:** evert  
**Workspace:** C:\Users\evert\Documents\rrhh

### En Caso de Problemas

1. **Conflictos de archivos:** Consultar matriz de exclusividad
2. **Errores de compilación:** Verificar que solo se modifican archivos asignados
3. **Dudas de arquitectura:** Revisar `.cursor/skills/blazor-component/SKILL.md`
4. **Bloqueo total:** Documentar en REFACTOR_STATUS.md y pausar

---

## 📚 RECURSOS ADICIONALES

### Skills del Proyecto
- `.cursor/skills/blazor-component/SKILL.md` - Patrones de componentes
- `.cursor/skills/hospital-ui-style/SKILL.md` - Estilos UI
- `.cursor/skills/build-and-verify/SKILL.md` - Compilación
- `.cursor/skills/dapper-repository/SKILL.md` - Acceso a datos

### Documentos de Referencia
- `architecture.md` - Arquitectura general
- `GLOSARIO_DOMINIO.md` - Términos de negocio
- `CHANGELOG.md` - Historial de cambios

### Comando de Compilación
```bash
dotnet build SGRRHH.Local/SGRRHH.Local.Server/SGRRHH.Local.Server.csproj --no-incremental
```

---

## 🎉 CONCLUSIÓN

Este es un proyecto de refactorización masivo pero **bien coordinado**. Cada agente tiene:
- ✅ Su zona exclusiva de trabajo
- ✅ Su prompt estructurado detallado
- ✅ Sus objetivos claros
- ✅ Sus checkpoints de compilación
- ✅ Sus pruebas definidas

**REGLA DE ORO:** Cuando tengas duda, compila y prueba. Es mejor ir lento y seguro que rápido y con errores.

**¡ÉXITO EN LA REFACTORIZACIÓN!** 🚀

---

**Creado:** 2026-01-16  
**Versión:** 1.0  
**Estado:** ACTIVO
