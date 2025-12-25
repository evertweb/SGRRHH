# 🔍 AUDITORÍA: MÓDULO VACACIONES (Vacation Management)

## 📊 Resumen del Módulo

**Ubicación:** `Views/VacacionesView.xaml` → `VacacionesViewModel.cs` → `VacacionService.cs` → `VacacionFirestoreRepository.cs`

**Propósito:** Gestionar vacaciones anuales con cálculo pro-rata según ley colombiana, aprobación y tracking de saldo disponible.

**Complejidad:** 🟡 **MEDIA** - Lógica de cálculo legal compleja

**Datos Almacenados:**
- EmpleadoId (FK)
- FechaInicio, FechaFin
- DiasTomados (15 días/año colombianos)
- PeriodoCorrespondiente (año: 2024, 2025)
- Estado (Programada, Disfrutada, Cancelada)
- Observaciones
- Auditoría: FechaSolicitud, SolicitadoPorId, AprobadoPorId, FechaAprobacion, MotivoRechazo

---

## 🎯 Problemas Esperados en VACACIONES

### **CRÍTICOS**
1. ⚠️ **Cálculo pro-rata incorrecto**
   - Empleado ingresa 15 ago 2024
   - En 2024: ¿cuántos días tiene? (4.5 meses = 6.25 días aprox)
   - CLAUDE.md dice: "15 días/año, pro-rata basado en hire date"
   - ¿Se implementa correctamente?

2. ⚠️ **Límite de 15 días por año no se valida**
   - Empleado solicita 20 días en año 2025
   - ¿Se rechaza automáticamente?
   - ¿O solo previene durante solicitud?

3. ⚠️ **DiasTomados mal calculado**
   - FechaInicio: 01 ago, FechaFin: 15 ago (15 días)
   - ¿Se cuentan fines de semana?
   - ¿Se restan festivos?
   - Test: 01-15 ago (lunes-viernes de 2 semanas)
     - Teoría: 10 días (2 sábados + 2 domingos = 4 días de fin de semana)
     - ¿Sistema calcula 10 o 15?

4. ⚠️ **Saldo de vacaciones no se trackea**
   - ResumenVacaciones: ¿muestra días tomados + disponibles?
   - ¿Se cargan de períodos anteriores?
   - Ej: 2024 no usó 3 días → 2025 debería tener 15+3=18?

5. ⚠️ **Vacaciones solapadas no se previenen**
   - Solicitud1: 01-15 ago (aprobada)
   - Solicitud2: 10-20 ago (se permite crear?)
   - ¿Se detecta solapamiento?

### **MEDIANOS**
6. ⚠️ **Estado sin máquina de transición**
   - Programada → Disfrutada → Cancelada
   - ¿Se puede volver a Programada?
   - ¿Se puede editar Disfrutada?

7. ⚠️ **Fechas sin lógica temporal**
   - FechaSolicitud: ¿Automática (Now)?
   - FechaInicio: ¿Puede ser pasada?
   - FechaAprobacion: ¿> FechaSolicitud?

8. ⚠️ **PeriodoCorrespondiente ambiguo**
   - ¿Año calendario (ene-dic)?
   - ¿O año laboral (aniversario ingreso)?
   - CLAUDE.md no especifica

9. ⚠️ **Cancellation logic incompleta**
   - Si cancela vacaciones Disfrutadas
   - ¿Recupera días? (vuelve a disponibles)
   - ¿O pierde días?

### **UX/ESCALABILIDAD**
10. ⚠️ **ResumenVacaciones sin cálculo previo**
    - ¿Muestra por período o total anual?
    - ¿Incluye días de períodos anteriores?
    - ¿Muestra "Días disponibles" actualizado?

11. ⚠️ **Sin validación de aprobador**
    - ¿Quién aprueba vacaciones?
    - ¿Supervisor directo?
    - ¿Solo Admin?

---

## 📋 ESTRUCTURA ACTUAL

```
Views/
└── VacacionesView.xaml ............ Crear/editar/cancelar vacaciones

ViewModels/
└── VacacionesViewModel.cs
    ├── GetResumenVacacionesAsync(empleado) ... Resumen por período
    ├── CalcularDiasDisponiblesAsync(empleado) ... Pro-rata
    ├── CreateVacacionAsync() .............. Nueva solicitud
    ├── MarcarComoDisfrutadaAsync() ....... Cambio estado
    └── Employee dropdown ................. Selección

Services/
└── VacacionService.cs
    ├── GetByEmpleadoIdAsync() ........... Vacaciones del empleado
    ├── CreateAsync() ................... Validar límite 15 días
    ├── CalcularDiasDisponiblesAsync() .. Pro-rata cálculo
    └── GetResumenVacacionesAsync() ..... Resumen

Repositories/
└── VacacionFirestoreRepository.cs
    ├── GetByEmpleadoRangoAsync()
    ├── GetByPeriodoAsync(empleado, año)
    └── Conflict detection

Entities/
└── Vacacion.cs
    ├── EmpleadoId
    ├── FechaInicio, FechaFin
    ├── DiasTomados (calculado)
    ├── PeriodoCorrespondiente (año)
    ├── Estado (enum)
    └── Relación Empleado, Usuario
```

---

## 🚀 PROMPT PARA ANALIZAR VACACIONES

```
Realiza un ANÁLISIS PROFUNDO del módulo VACACIONES:

⚠️ NOTA: Este módulo implementa LEY COLOMBIANA. Errores pueden ser legales.

FASE 1: EXPLORACIÓN
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Lee:
- /src/SGRRHH.WPF/Views/VacacionesView.xaml
- /src/SGRRHH.WPF/ViewModels/VacacionesViewModel.cs
- /src/SGRRHH.Infrastructure/Services/VacacionService.cs
- /src/SGRRHH.Infrastructure/Firebase/Repositories/VacacionFirestoreRepository.cs
- /src/SGRRHH.Core/Entities/Vacacion.cs
- /src/CLAUDE.md (líneas sobre "Vacaciones")

FASE 2: LEY COLOMBIANA
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Según CLAUDE.md:
✓ "15 días per year (cumulative)"
✓ "Calculated pro-rata based on hire date"

Esto significa:
1. EMPLEADO NUEVO EN 2024:
   - Ingresa: 15 aug 2024
   - Hasta: 31 dic 2024 (4.5 meses)
   - Vacaciones 2024: 15 * (4.5/12) = 5.625 ≈ 6 días

2. EMPLEADO EN 2025:
   - A partir: 01 ene 2025
   - Vacaciones 2025: 15 días completos

3. PERO ¿ACUMULACIÓN?
   - Si no usó 6 días en 2024
   - 2025 tiene 15+6=21 días?
   - O ¿se pierden?

FASE 3: CÁLCULO DE DÍAS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

¿DiasTomados incluye fines de semana o no?

Escenarios:
✓ Caso 1: Lunes-Viernes (1-5 ago, no hay festivos)
  - FechaInicio: 01 ago (lunes)
  - FechaFin: 05 ago (viernes)
  - Calendario: L M M J V S D
  - ¿DiasTomados = 5? (solo laborales)
  - O ¿DiasTomados = 7? (incluye fin de semana)

✓ Caso 2: Viernes-Lunes (07-10 ago, 07=viernes)
  - FechaInicio: 07 ago (viernes)
  - FechaFin: 10 ago (lunes)
  - ¿DiasTomados = 2? (viernes + lunes)
  - ¿DiasTomados = 4? (viernes a lunes inclusive)
  - ¿DiasTomados = 7? (incluye fin de semana)

✓ Festivos Colombianos 2025:
  - 01 ene (Año Nuevo)
  - 10 mar (Lunes carnaval)
  - 21 abr (San Jorge)
  - 01 may (Trabajo)
  - 19 jun (Corpus Christi)
  - 26 jun (Sagrado Corazón)
  - 07 ago (Batalla de Boyacá)
  - 07 nov (Independencia)
  - 08 dic (Inmaculada)
  - 25 dic (Navidad)

  Test case:
  - Solicita vacaciones 07-09 ago (Batalla de Boyacá es feriado)
  - ¿DiasTomados = 3?
  - O ¿DiasTomados = 2? (resta el festivo)

FASE 4: PRO-RATA CALCULATION
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Busca la función CalcularDiasDisponiblesAsync():

✓ Test: Juan ingresa 20 ago 2024
  - Períodos 2024:
    - Aug: 11 días (20-31 ago)
    - Sep: 30 días
    - Oct: 31 días
    - Nov: 30 días
    - Dec: 31 días
    - TOTAL: 133 días
    - Vacaciones = 15 * (133/365) = 5.47 ≈ 5 días?

  O ¿se calcula:
  - Meses completos (sep, oct, nov, dec) = 4 meses
  - Pro-rata = 15 * (4/12) = 5 días?

  ¿Cuál implementa el sistema?

FASE 5: PERÍODO Y ACUMULACIÓN
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

✓ PeriodoCorrespondiente: ¿significa qué?
  - Año calendario (ene-dic)?
  - Año laboral (aniversario ingreso)?
  - ¿O ambos?

✓ Acumulación:
  - Juan 2024: tiene 6 días, usa 2 → quedan 4
  - Juan 2025: ¿tiene 15 o 15+4=19?
  - ¿Acumulación es automática?

✓ Máximo acumulable:
  - ¿Puede acumular infinitamente?
  - ¿O máximo 30 días?
  - ¿O máximo 15 nuevos + 15 anteriores?

FASE 6: VALIDACIÓN DE SOLICITUD
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

✓ Límite por período:
  - ¿Rechaza si DiasTomados > DiasDisponibles?
  - Test: Juan tiene 5 días, solicita 7 → ¿rechaza?

✓ Solapamientos:
  - Solicitud1: 01-15 ago (aprobada)
  - Solicitud2: 10-20 ago
  - ¿Se permite crear Solicitud2?
  - ¿O se rechaza (solapamiento)?

✓ Conflictos con permisos:
  - Solicitud de permiso: 15-20 ago
  - Solicitud de vacaciones: 18-25 ago
  - ¿Se detecta conflicto?
  - ¿Se previene?

✓ Fechas:
  - FechaInicio: ¿Puede ser pasada?
  - FechaFin: ¿Puede ser pasada?
  - FechaInicio: ¿Puede ser futura? (ej: próx mes)
  - FechaFin: ¿> FechaInicio?

FASE 7: ESTADOS Y TRANSICIONES
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Estados: Programada, Disfrutada, Cancelada

✓ Transiciones válidas:
  - Programada → Disfrutada (empleado regresa)
  - Programada → Cancelada (cancela)
  - Disfrutada → Cancelada (cancelar post-facto?)
  - ¿Se previenen otras?

✓ Edición:
  - ¿Se puede editar Programada?
  - ¿Se puede editar Disfrutada? (nunca)
  - ¿Se requiere nueva aprobación si cambia fecha?

✓ Cancelación:
  - Si cancela Programada → ¿recupera días?
  - Si cancela Disfrutada → ¿recupera días?
  - ¿Automático o manual?

FASE 8: AUDITORÍA Y APROBACIÓN
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

✓ SolicitadoPorId:
  - ¿Usuario que solicita?
  - ¿Automáticamente CurrentUser?

✓ AprobadoPorId:
  - ¿Quién aprueba vacaciones?
  - ¿Supervisor del empleado?
  - ¿Solo Admin?
  - ¿Se valida en AprobarAsync()?

✓ FechaAprobacion:
  - ¿Se setea automáticamente?
  - ¿> FechaSolicitud?

✓ MotivoRechazo:
  - ¿Requerido si está en Cancelada?

FASE 9: RESUMEN DE VACACIONES
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

GetResumenVacacionesAsync() debería mostrar:
✓ Por cada periodo (año):
  - Días asignados (pro-rata)
  - Días tomados (sum(Disfrutadas))
  - Días disponibles (asignados - tomados)
  - Vacaciones Programadas (pendientes)
  - Días vencidos (años anteriores no usados)

Test: Juan
- 2024 (pro-rata): 6 días
  - Disfrutadas: 2 días
  - Disponibles: 4 días
- 2025 (completo): 15 días
  - Disfrutadas: 0 días
  - Disponibles: 15 + 4 (vencidos) = 19 días?

¿El resumen calcula esto?

FASE 10: DOCUMENTACIÓN
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Por cada problema:

┌──────────────────────────────────────────────────────────┐
│ PROBLEMA #N: [Título]                                   │
├──────────────────────────────────────────────────────────┤
│ Severidad: 🔴 CRÍTICA / 🟡 MEDIA / 🟢 BAJA            │
│ Ubicación: [Archivo:línea]                              │
│                                                          │
│ Análisis:                                                │
│ [Descripción]                                           │
│                                                          │
│ Impacto legal:                                          │
│ [Cumplimiento con ley colombiana?]                     │
│                                                          │
│ Caso de fallo:                                          │
│ [Escenario específico]                                 │
│                                                          │
│ Propuesta fix:                                         │
│ [Solución]                                             │
└──────────────────────────────────────────────────────────┘

RESULTADO ESPERADO:
═════════════════════════════════════════════════════════════
Total problemas: ~12-15
Críticos: ~3-4 (pro-rata, acumulación, solapamientos)
Medianos: ~6-8
Bajos: ~2-3

⚠️ NOTA: Errores aquí afectan cumplimiento legal
Próximos pasos: Implementar fixes con validación legal
```

---

## 📌 ÁREA CRÍTICA: CÁLCULO PRO-RATA

Este es el corazón del módulo. Busca:

```csharp
public decimal CalcularDiasProRata(DateTime fechaIngreso, int año)
{
    // Días desde ingreso hasta fin del año
    // Debe ser correcto LEGALMENTE

    // Opción A: Meses completos
    // Opción B: Días calendarios
    // Opción C: Días laborales

    // El código actual ¿cuál usa?
}
```

Errores aquí = problemas legales con empleados.

---

## ✨ Listo para usar

Copia el PROMPT PARA ANALIZAR VACACIONES cuando estés listo.
