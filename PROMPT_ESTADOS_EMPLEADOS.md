# PROMPT: Diseño de Lógica de Estados de Empleados

## 🎯 Objetivo

Diseñar e implementar la lógica completa de estados de empleados considerando:
- Sistema de roles (Operador, Aprobador, Administrador)
- Modo Corporativo activo
- Flujos de aprobación
- Estados automáticos basados en vacaciones, permisos e incapacidades

---

## 📋 Fase 1: Investigación de Contexto

**ANTES de proponer soluciones, el agente debe investigar:**

### 1.1 Sistema de Roles Actual
```
Archivos a revisar:
- SGRRHH.Local.Domain/Enums/RolUsuario.cs
- SGRRHH.Local.Domain/Services/ConfiguracionRoles.cs
- SGRRHH.Local.Domain/Enums/PermisosModulo.cs
- SGRRHH.Local.Infrastructure/Services/LocalAuthService.cs
```

**Preguntas a responder:**
- ¿Qué permisos tiene cada rol para el módulo Empleados?
- ¿Cómo funciona el Modo Corporativo?
- ¿Qué métodos existen para verificar permisos?

### 1.2 Estados de Empleado Existentes
```
Archivos a revisar:
- SGRRHH.Local.Domain/Enums/EstadoEmpleado.cs
- SGRRHH.Local.Domain/Entities/Empleado.cs
```

**Preguntas a responder:**
- ¿Qué estados existen actualmente?
- ¿Hay campos de auditoría (CreadoPorId, AprobadoPorId, etc.)?
- ¿Existe campo para fecha de cambio de estado?

### 1.3 Flujo de Creación de Empleados
```
Archivos a revisar:
- SGRRHH.Local.Server/Components/Pages/Empleados.razor
- SGRRHH.Local.Server/Components/Pages/EmpleadoOnboarding.razor
- SGRRHH.Local.Infrastructure/Repositories/EmpleadoRepository.cs
```

**Preguntas a responder:**
- ¿Dónde se crea el empleado (qué método/componente)?
- ¿El usuario puede elegir el estado al crear?
- ¿Se registra quién creó el empleado?

### 1.4 Módulo de Vacaciones
```
Archivos a revisar:
- SGRRHH.Local.Domain/Entities/Vacacion.cs
- SGRRHH.Local.Domain/Enums/EstadoVacacion.cs
- SGRRHH.Local.Server/Components/Pages/Vacaciones.razor
- SGRRHH.Local.Infrastructure/Repositories/VacacionRepository.cs
```

**Preguntas a responder:**
- ¿Qué estados tiene una solicitud de vacaciones?
- ¿Se registran fechas de inicio y fin?
- ¿Hay lógica que afecte el estado del empleado?

### 1.5 Módulo de Permisos
```
Archivos a revisar:
- SGRRHH.Local.Domain/Entities/Permiso.cs
- SGRRHH.Local.Domain/Enums/EstadoPermiso.cs
- SGRRHH.Local.Server/Components/Pages/Permisos.razor
- SGRRHH.Local.Infrastructure/Repositories/PermisoRepository.cs
```

**Preguntas a responder:**
- ¿Qué tipos de permisos existen?
- ¿Los permisos tienen fecha de inicio/fin?
- ¿Hay permisos de día completo vs horas?

### 1.6 Módulo de Incapacidades
```
Archivos a revisar:
- SGRRHH.Local.Domain/Entities/Incapacidad.cs (si existe)
- SGRRHH.Local.Server/Components/Pages/ (buscar incapacidad*)
```

**Preguntas a responder:**
- ¿Existe módulo de incapacidades?
- ¿Cómo se relaciona con el empleado?

---

## 📋 Fase 2: Requerimientos del Usuario

### 2.1 Requerimiento Confirmado: Creación según Rol

| Rol del Creador | Estado Inicial del Empleado |
|-----------------|----------------------------|
| Operador (Secretaria) | `PendienteAprobacion` (forzado, sin opción) |
| Aprobador (Ingeniera) | `Activo` (por defecto) |
| Administrador | `Activo` (por defecto) |

### 2.2 Requerimiento Confirmado: Cambio a Inactivo/Vacaciones

- Todos los usuarios pueden cambiar estado de `Activo` a:
  - `Inactivo`
  - `EnVacaciones`

### 2.3 Requerimientos a Definir (después de investigar)

El agente debe presentar opciones con pros/contras basándose en el contexto encontrado:

**A) Estados Automáticos por Vacaciones:**
- Opción 1: Manual (usuario marca cuando inicia/termina)
- Opción 2: Semi-automático (al aprobar vacación, preguntar si cambiar estado)
- Opción 3: Automático (job que revisa fechas y cambia estados)

**B) Estados Automáticos por Permisos:**
- Opción 1: Permisos no afectan estado (recomendado para permisos cortos)
- Opción 2: Permisos de día completo cambian estado temporalmente
- Opción 3: Crear estado `EnPermiso`

**C) Estados Automáticos por Incapacidades:**
- Opción 1: Manual
- Opción 2: Automático basado en fechas de incapacidad

**D) Retorno a Activo:**
- Opción 1: Manual siempre
- Opción 2: Automático cuando termina el período
- Opción 3: Notificación para recordar cambiar estado

**E) Transiciones Permitidas:**
- ¿De `Inactivo` se puede volver a `Activo`? ¿Quién puede?
- ¿Se puede ir de `EnVacaciones` a `Inactivo` directamente?

---

## 📋 Fase 3: Preguntas Aclaratorias

**DESPUÉS de investigar**, el agente debe presentar:

1. **Resumen del contexto encontrado** (1 párrafo por módulo)
2. **Diagrama de estados propuesto** (basado en lo que existe)
3. **Preguntas específicas** para cada decisión pendiente
4. **Recomendación técnica** basada en la arquitectura existente

---

## 📋 Fase 4: Implementación (después de respuestas)

Una vez el usuario responda las preguntas, implementar:

### 4.1 Cambios en Backend
- [ ] Modificar `EmpleadoRepository.CreateAsync()` para forzar estado según rol
- [ ] Crear servicio `EstadoEmpleadoService` para centralizar lógica
- [ ] Implementar validaciones de transición de estados

### 4.2 Cambios en Frontend
- [ ] Ocultar/deshabilitar selector de estado en creación para Operador
- [ ] Mostrar solo transiciones válidas según estado actual
- [ ] Agregar confirmación antes de cambiar estado

### 4.3 Automatizaciones (si aplica)
- [ ] Background service para cambios automáticos de estado
- [ ] Notificaciones cuando cambia estado

---

## 🔧 Contexto Técnico Adicional

### Arquitectura
- Blazor Server (.NET 8)
- SQLite con Dapper
- Clean Architecture (Domain → Infrastructure → Server)

### Base de Datos del Servidor
- Host: 192.168.1.248
- Path: C:\SGRRHH\Data\sgrrhh.db
- Herramienta: C:\SGRRHH\sqlite3.exe

### Usuarios de Prueba
| Usuario | Password | Rol |
|---------|----------|-----|
| admin | (existente) | Administrador |
| secretaria | secretaria123 | Operador |
| ingeniera | ingeniera123 | Aprobador |

### Modo Corporativo
- Actualmente: **ACTIVO** (restricciones de roles aplicadas)
- Toggle en: Configuración → Seguridad

---

## ⚠️ Notas Importantes

1. **No modificar la BD de producción** sin backup previo
2. **Respetar estilos CSS** existentes en `hospital.css`
3. **Probar con los 3 usuarios** antes de desplegar
4. **El servidor se inicia** con `C:\SGRRHH\IniciarServidor.bat`

---

*Prompt creado: Enero 2026*
*Para: Diseño de lógica de estados de empleados*
