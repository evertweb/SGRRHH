# 🔍 AUDITORÍA: MÓDULO PERMISOS (Permissions & Leaves)

## 📊 Resumen del Módulo

**Ubicación:** `Views/PermisosListView.xaml` → `PermisosListViewModel.cs` + `BandejaAprobacionViewModel.cs` → `PermisoService.cs` → `PermisoFirestoreRepository.cs`

**Propósito:** Gestionar solicitudes de permisos/licencias con workflow de aprobación, validación de conflictos con vacaciones y generación de PDFs.

**Complejidad:** 🟠 **ALTA** - Workflow complejo con múltiples validaciones

**Datos Almacenados:**
- NumeroActa (auto: PERM-2025-0001)
- EmpleadoId, TipoPermisoId (FKs)
- FechaSolicitud, FechaInicio, FechaFin, TotalDias
- Tipo de permiso (13 tipos colombianos)
- Estado (Pendiente, Aprobado, Rechazado, Cancelado)
- Observaciones, DocumentoSoportePath
- Compensación: DiasPendientesCompensacion, FechaCompensacion
- Auditoría: SolicitadoPorId, AprobadoPorId, FechaAprobacion, MotivoRechazo

---

## 🎯 Problemas Esperados en PERMISOS

### **CRÍTICOS**
1. ⚠️ **NumeroActa duplicado o no único**
   - Secuencia PERM-2025-0001 puede colisionar
   - ¿Se resetea cada año?
   - ¿Atomicidad en generación?

2. ⚠️ **TotalDias mal calculado**
   - ¿Incluye fines de semana?
   - ¿Incluye festivos colombianos?
   - ¿Manual o automático?

3. ⚠️ **Validación de conflictos incompleta**
   - ¿Se valida contra OTRAS LICENCIAS del mismo empleado?
   - ¿Se valida contra VACACIONES?
   - ¿Se valida contra INCAPACIDADES?
   - ¿Permisos solapados se previenen?

4. ⚠️ **Validación de TipoPermiso**
   - ¿Se valida que TipoPermisoId existe?
   - ¿Se valida que el tipo permite esos días? (ej: Luto máx 3 días)
   - ¿Se valida si requiere documento (RequiereSoporte)?

5. ⚠️ **DocumentoSoportePath sin validación**
   - ¿Se valida que existe si RequiereSoporte=true?
   - ¿Se validaFormato?
   - ¿Se maneja error de descarga?

### **MEDIANOS**
6. ⚠️ **Estados sin máquina de transición**
   - Transiciones válidas: Pendiente→Aprobado→Compensado
   - ¿Se previene Rechazado→Aprobado?
   - ¿Se previene cambiar FechaFin después de aprobado?

7. ⚠️ **Fechas sin lógica**
   - FechaSolicitud: ¿Quién la setea? ¿Automática?
   - FechaInicio: ¿Puede ser pasada?
   - FechaFin: ¿Puede ser < FechaInicio?
   - FechaAprobacion: ¿> FechaSolicitud?

8. ⚠️ **PDF sin validación**
   - ¿Se genera correctamente?
   - ¿Se guarda en Firebase Storage?
   - ¿URL de acceso es válida?

9. ⚠️ **Filtros sin paginación**
   - Pueden devolver 1000+ registros
   - Lista no es scrolleable eficientemente

10. ⚠️ **Compensación mal gestionada**
    - DiasPendientesCompensacion: ¿Se calcula?
    - ¿Se aplica auto en próximos períodos?
    - ¿Manual tracking?

### **UX/ARQUITECTURA**
11. ⚠️ **Bandeja de aprobación sin priorización**
    - ¿Se ordenan por fecha?
    - ¿Se indican urgentes?
    - ¿Se pueden aprobar en batch?

12. ⚠️ **SolicitadoPorId vs EmpleadoId**
    - ¿SolicitadoPorId es usuario que solicita?
    - ¿O supervisor que autoriza?
    - Confusión en auditoría

---

## 📋 ESTRUCTURA ACTUAL

```
Views/
├── PermisosListView.xaml ............... Lista con filtros (fecha, estado, etc)
├── PermisoFormWindow.xaml ............ Crear/editar/ver permiso
└── BandejaAprobacionView.xaml ....... Cola de aprobación

ViewModels/
├── PermisosListViewModel.cs
│   ├── GetPermisosAsync(filters) ..... Búsqueda multi-filtro
│   ├── CancelarPermisoAsync() ........ Cambiar estado
│   └── Statistics ................... Pendientes, aprobados, etc
├── PermisoFormViewModel.cs ......... Form logic + PDF preview
└── BandejaAprobacionViewModel.cs ... Approve/reject workflow

Services/
└── PermisoService.cs
    ├── SolicitarPermisoAsync(permiso) .... Crear, NumeroActa, validar
    ├── AprobarPermisoAsync(id, aprobadorId) ... Aprobación
    ├── RechazarPermisoAsync(id, reason) ... Rechazo
    ├── CancelarPermisoAsync(id) ........ Cancelación
    ├── GetPendientesAsync() .......... Para bandeja
    └── GenerarActaAsync() .......... PDF generation

Repositories/
└── PermisoFirestoreRepository.cs
    ├── Sequence generation (NumeroActa)
    ├── GetByEmpleadoRangoAsync(empl, desde, hasta)
    ├── GetConflictAsync() ............ Detectar solapamientos
    └── Photo/PDF management

Entities/
└── Permiso.cs
    ├── NumeroActa (PK alternativa)
    ├── EmpleadoId, TipoPermisoId (FKs)
    ├── Fechas, Estados, Auditoría
    ├── Relación con TipoPermiso
    └── Relación con Usuario (Solicitador, Aprobador)
```

---

## 🚀 PROMPT PARA ANALIZAR PERMISOS

```
Realiza un ANÁLISIS EXHAUSTIVO del módulo PERMISOS:

FASE 1: ESTRUCTURA Y FLUJOS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Lee:
- /src/SGRRHH.WPF/Views/PermisosListView.xaml
- /src/SGRRHH.WPF/ViewModels/PermisosListViewModel.cs
- /src/SGRRHH.WPF/ViewModels/BandejaAprobacionViewModel.cs
- /src/SGRRHH.Infrastructure/Services/PermisoService.cs
- /src/SGRRHH.Infrastructure/Firebase/Repositories/PermisoFirestoreRepository.cs
- /src/SGRRHH.Core/Entities/Permiso.cs
- /src/SGRRHH.Core/Entities/TipoPermiso.cs

FASE 2: FLUJOS CRÍTICOS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

1. SOLICITAR PERMISO:
   a) Seleccionar empleado
   b) Seleccionar tipo (13 tipos colombianos)
   c) Ingresar fechas (inicio, fin)
   d) Ingresar observaciones + documento soporte
   e) Sistema calcula: TotalDias, NumeroActa, Estado=Pendiente
   f) Guardar → notificar aprobador

2. VALIDACIÓN DE CONFLICTOS:
   a) ¿Ej: Juan solicita 15-20 nov, pero ya tiene permiso 18-25 nov?
   b) ¿Se detecta solapamiento?
   c) ¿Y si hay vacaciones 19-22 nov?
   d) ¿Se previene?

3. CÁLCULO DE DÍAS:
   Ej: Permiso 15-20 nov (viernes-miércoles)
   ¿TotalDias = 6 (incluye sábados)?
   O ¿TotalDias = 4 (solo laborales)?
   ¿Se consideran festivos colombianos?

4. APROBAR PERMISO:
   a) Aprobador entra a "Bandeja de Aprobación"
   b) Revisa solicitud + empleado + documento
   c) Aprueba → Estado=Aprobado, AprobadoPorId, FechaAprobacion
   d) Sistema genera PDF "Acta de Permiso"
   e) Genera notificación a empleado

5. RECHAZAR PERMISO:
   a) Aprobador rechaza → Estado=Rechazado
   b) MotivoRechazo requerido
   c) Vuelve a Pendiente? O final?

6. CANCELAR PERMISO:
   a) Empleado/Admin cancela permiso aprobado
   b) ¿Se recuperan días? (compensación inversa)

FASE 3: VALIDACIÓN DE LÓGICA
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

A) NÚMERO DE ACTA:
   ✓ Formato: PERM-YYYY-NNNN (ej: PERM-2025-0042)
   ✓ ¿Se genera automáticamente?
   ✓ ¿Es único? (no hay 2x PERM-2025-0042)
   ✓ ¿Se resetea cada año?
   ✓ ¿Es thread-safe en multi-usuario?

B) TIPOS DE PERMISO (13 tipos colombianos):
   ✓ Calamidad doméstica (hasta 5 días)
   ✓ Cita médica (hasta 1 día)
   ✓ Luto (hasta 5 días)
   ✓ Lactancia (30 min/día)
   ✓ Maternidad (12 semanas)
   ✓ Paternidad (8 días)
   ✓ ... otros 7 tipos

   Para cada tipo, valida:
   ✓ ¿Máximo de días permitido?
   ✓ ¿RequiereSoporte (documento)?
   ✓ ¿Remunerado o no?
   ✓ ¿Se respetan estos límites en servicio?

C) DETECCIÓN DE CONFLICTOS:
   Busca GetConflictAsync() o similar que:
   ✓ Detecta permisos solapados
   ✓ Detecta vacaciones solapadas
   ✓ Detecta incapacidades solapadas
   ✓ ¿Se impide solicitar si hay conflicto?

   Test case:
   - Juan solicita permiso 15-20
   - Ya tiene permiso 18-25 (solapamiento)
   - ¿Se rechaza automáticamente?

D) CÁLCULO DE DÍAS:
   ✓ ¿Incluye fines de semana? (script de cálculo)
   ✓ ¿Incluye festivos? (lista de festivos)
   ✓ ¿Es pro-rata para permisos parciales?
   ✓ Ej: Permiso de 2 horas cuenta como 0.25 día?

E) VALIDACIÓN DE FECHAS:
   ✓ FechaSolicitud: ¿Automática (Now)?
   ✓ FechaInicio: ¿Puede ser pasada?
   ✓ FechaFin: ¿Puede ser < FechaInicio?
   ✓ FechaAprobacion: ¿Se setea en AprobarPermisoAsync()?
   ✓ ¿Se previene que sea < FechaSolicitud?

F) DOCUMENTO SOPORTE:
   ✓ ¿Se valida que existe si RequiereSoporte=true?
   ✓ ¿Se valida formato (pdf, jpg, png)?
   ✓ ¿Se valida tamaño máx?
   ✓ ¿Se guarda en Firebase Storage?
   ✓ ¿URL es persistente?

G) ESTADOS Y TRANSICIONES:
   Estados: Pendiente, Aprobado, Rechazado, Cancelado

   Transiciones válidas:
   ✓ Pendiente → Aprobado (aprobar)
   ✓ Pendiente → Rechazado (rechazar)
   ✓ Aprobado → Cancelado (cancelar)
   ✓ ¿Se previenen otras transiciones?
   ✓ ¿Se previene cambiar datos después aprobado?

H) COMPENSACIÓN:
   ✓ DiasPendientesCompensacion: ¿Se calcula?
   ✓ Ej: Permiso no remunerado de 2 días
   ✓ ¿Se restan de vacaciones?
   ✓ FechaCompensacion: ¿Se setea?
   ✓ ¿Se aplica automáticamente?

I) PDF/ACTA:
   ✓ ¿Se genera automáticamente al aprobar?
   ✓ ¿Contiene datos correcto?
   ✓ ¿Se incluye firma de aprobador?
   ✓ ¿Se guarda en Firebase Storage?
   ✓ ¿Se enlaza a permiso?

J) AUDITORÍA:
   ✓ SolicitadoPorId: ¿Usuario que crea? ¿Empleado?
   ✓ AprobadoPorId: ¿Se asigna en AprobarPermisoAsync()?
   ✓ FechaAprobacion: ¿Se asigna?
   ✓ MotivoRechazo: ¿Requerido si Rechazado?

FASE 4: BÚSQUEDA Y FILTROS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Filtros probables:
- Por empleado
- Por estado (Pendiente, Aprobado, Rechazado)
- Por rango de fechas
- Por tipo de permiso
- Por aprobador (para bandeja)

Validar:
✓ ¿Los filtros son eficientes? (Firestore índices)
✓ ¿Se combinan múltiples filtros?
✓ ¿Hay paginación?
✓ ¿Se ordena por fecha? (más recientes primero)

FASE 5: BANDEJA DE APROBACIÓN
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Características:
- Solo para Aprobador/Admin
- Muestra permisos con Estado=Pendiente
- Se pueden aprobar uno por uno O en batch?
- ¿Se auto-refresca?
- ¿Hay indicador de urgencia? (próximo a inicio)
- ¿Notificaciones push?

Validar:
✓ ¿Filtrado por aprobador actual?
✓ ¿Se previene que aprobador apruebes su propio permiso?
✓ ¿Notificación después de aprobar/rechazar?

FASE 6: DOCUMENTACIÓN
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
Por cada problema:

┌──────────────────────────────────────────────────────────┐
│ PROBLEMA #N: [Título]                                   │
├──────────────────────────────────────────────────────────┤
│ Severidad: 🔴 CRÍTICA / 🟡 MEDIA / 🟢 BAJA            │
│ Ubicación: [Archivo:línea]                              │
│                                                          │
│ Análisis:                                                │
│ [Descripción detallada]                                 │
│                                                          │
│ Caso de fallo:                                          │
│ [Escenario específico donde falla]                     │
│                                                          │
│ Impacto:                                                │
│ [Efectos en negocio/usuarios]                         │
│                                                          │
│ Propuesta fix:                                         │
│ [Solución específica]                                  │
└──────────────────────────────────────────────────────────┘

RESULTADO ESPERADO:
═════════════════════════════════════════════════════════════
Total problemas: ~12-15
Críticos: ~4-5 (NumeroActa, conflictos, TotalDias, documento)
Medianos: ~6-8
Bajos: ~2-3

Próximos pasos: Implementar fixes en fases por severidad
```

---

## 📌 ÁREA CRÍTICA: DETECCIÓN DE CONFLICTOS

Este es el problema más grave. Busca específicamente:

```csharp
// ¿Existe este código?
private async Task<bool> HasConflictAsync(int empleadoId, DateTime inicio, DateTime fin)
{
    // Busca permisos/vacaciones solapadas
    // Si FechaInicio <= fin AND FechaFin >= inicio → conflicto
}
```

Si NO existe, ese es un problema CRÍTICO.

---

## ✨ Listo para usar

Copia el PROMPT PARA ANALIZAR PERMISOS cuando estés listo.
