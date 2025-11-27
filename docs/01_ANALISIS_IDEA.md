# 📋 Análisis y Organización de la Idea - Sistema RRHH

## 🎯 Visión General del Proyecto

**Nombre propuesto:** Sistema de Gestión RRHH Local (SGRRHH)

**Objetivo principal:** Aplicación nativa de Windows, 100% local (sin conexión a internet), para gestionar el control diario de trabajadores, actividades y actas de permisos/licencias.

---

## 🧩 MÓDULOS IDENTIFICADOS

### 📌 MÓDULO 1: Control Diario de Trabajadores y Actividades

**Propósito:** Registrar qué hizo cada empleado cada día, qué actividad realizó, para posteriormente analizar:
- Cuántos días tomó una actividad
- Cuántos trabajadores participaron en cada actividad
- Historial de trabajo por empleado

**Datos a capturar:**
| Campo | Descripción | Ejemplo |
|-------|-------------|---------|
| Fecha | Día del registro | 26/11/2025 |
| Empleado | Quién realizó la actividad | Pedro Pérez |
| Actividad | Qué se hizo | Inventario de almacén |
| Horas trabajadas | Duración | 8 horas |
| Ubicación/Área | Dónde se realizó | Bodega Central |
| Observaciones | Notas adicionales | Completado al 80% |
| Estado | Progreso | En curso / Completado |

**Preguntas para ti:**
- [ ] ¿Las actividades son predefinidas o se crean sobre la marcha?
- [ ] ¿Un empleado puede tener múltiples actividades en un día?
- [ ] ¿Necesitas registrar hora de entrada y salida?
- [ ] ¿Las actividades pertenecen a proyectos más grandes?
- [ ] ¿Necesitas categorías de actividades? (Ej: Administrativo, Operativo, Mantenimiento)

---

### 📌 MÓDULO 2: Gestión de Permisos y Licencias

**Propósito:** Llevar un acta/registro formal de todos los permisos y licencias de los trabajadores.

**Datos a capturar:**
| Campo | Descripción | Ejemplo |
|-------|-------------|---------|
| Empleado | Quien solicita | Pedro Pérez |
| Tipo de permiso | Categoría | Permiso personal / Licencia médica / Vacaciones |
| Motivo | Razón detallada | Motivos personales |
| Fecha solicitud | Cuándo se pidió | 25/11/2025 |
| Fecha(s) del permiso | Días solicitados | 27/11/2025 - 28/11/2025 |
| Estado | Situación actual | Pendiente / Aprobado / Rechazado |
| Aprobado por | Quién autorizó | Gerente Juan García |
| Tipo remuneración | Cómo se compensa | Remunerado / No remunerado / Compensatorio |
| Días a compensar | Si aplica | 29/11/2025 |
| Documento adjunto | Soporte | Certificado médico |
| Observaciones | Notas | - |

**Tipos de permisos comunes:**
- [ ] Permiso personal
- [ ] Licencia médica (enfermedad)
- [ ] Licencia por maternidad/paternidad
- [ ] Vacaciones
- [ ] Permiso por fallecimiento (duelo)
- [ ] Permiso por matrimonio
- [ ] Permiso para citas médicas
- [ ] Licencia no remunerada
- [ ] Compensatorio (día libre por horas extra)
- [ ] ¿Otros que uses frecuentemente?

**Tipos de compensación:**
- [ ] Remunerado (se paga normal)
- [ ] No remunerado (descuento de salario)
- [ ] Compensatorio (se devuelve el día trabajando otro día)
- [ ] A cuenta de vacaciones

**Preguntas para ti:**
- [ ] ¿Necesitas flujo de aprobación? (Ej: Supervisor → Gerente → RRHH)
- [ ] ¿Hay límites de días por tipo de permiso?
- [ ] ¿Necesitas generar documentos/actas imprimibles?
- [ ] ¿Necesitas alertas de vencimiento de licencias?

---

### 📌 MÓDULO 3: Gestión de Empleados (Base)

**Propósito:** Catálogo maestro de empleados (necesario para los otros módulos).

**Datos básicos del empleado:**
| Campo | Descripción |
|-------|-------------|
| ID/Código | Identificador único |
| Cédula/DNI | Documento de identidad |
| Nombres y Apellidos | Nombre completo |
| Cargo | Posición actual |
| Departamento/Área | Dónde trabaja |
| Fecha de ingreso | Antigüedad |
| Tipo de contrato | Fijo / Temporal / etc. |
| Estado | Activo / Inactivo / Vacaciones |
| Contacto | Teléfono, email |
| Foto | Opcional |

**Preguntas para ti:**
- [ ] ¿Cuántos empleados manejas aproximadamente?
- [ ] ¿Necesitas información adicional del empleado?
- [ ] ¿Los empleados tienen supervisores/jefes directos?

---

## 📊 REPORTES QUE PODRÍAS NECESITAR

### Del Control Diario:
- [ ] Reporte de actividades por empleado (¿Qué hizo Pedro este mes?)
- [ ] Reporte de empleados por actividad (¿Quiénes trabajaron en el inventario?)
- [ ] Reporte de días por actividad (¿Cuánto tomó el proyecto X?)
- [ ] Reporte de productividad diaria
- [ ] Resumen mensual de actividades

### De Permisos/Licencias:
- [ ] Historial de permisos por empleado
- [ ] Permisos pendientes de aprobación
- [ ] Días de vacaciones disponibles por empleado
- [ ] Estadísticas de ausentismo
- [ ] Acta formal de permiso (para imprimir y firmar)

---

## ❓ PREGUNTAS CLAVE PARA DEFINIR EL ALCANCE

### Sobre la operación:
1. ¿Cuántas personas usarán el sistema? (¿Solo tú, o varios en RRHH?)
2. ¿Necesitas que varios usuarios accedan simultáneamente?
3. ¿Necesitas diferentes niveles de acceso? (Admin, Supervisor, Consulta)
4. ¿Necesitas que funcione en varias computadoras? (compartiendo datos)

### Sobre los datos:
5. ¿Tienes datos existentes que migrar? (Excel, otro sistema)
6. ¿Necesitas hacer backups automáticos?
7. ¿Cuánto histórico necesitas mantener?

### Sobre la interfaz:
8. ¿Prefieres algo simple y funcional, o más visual con gráficos?
9. ¿Necesitas imprimir reportes frecuentemente?
10. ¿Usarás el sistema principalmente en escritorio o también tablets?

---

## 🎯 PRIORIZACIÓN SUGERIDA (MVP)

### Fase 1 - MVP (Mínimo Viable):
1. ✅ Gestión básica de empleados (CRUD)
2. ✅ Registro diario de actividades
3. ✅ Registro de permisos/licencias
4. ✅ Consultas básicas y búsquedas
5. ✅ Un reporte básico de cada módulo

### Fase 2 - Mejoras:
- Reportes avanzados y gráficos
- Exportación a Excel/PDF
- Dashboard con estadísticas
- Flujo de aprobaciones

### Fase 3 - Extras:
- Gestión de usuarios y permisos
- Backups automáticos
- Notificaciones/alertas
- Calendario visual

---

## 📝 TUS RESPUESTAS (Completa esta sección)

Por favor, responde las preguntas marcadas con [ ] arriba y agrega aquí cualquier otra información:

### ¿Qué más necesitas que no mencioné?
```
(Escribe aquí)
```

### ¿Hay algún proceso específico de tu empresa que deba considerarse?
```
(Escribe aquí)
```

### ¿Cuál es tu prioridad #1?
```
(Escribe aquí)
```

---

**Una vez que completes estas preguntas, crearemos:**
1. 📋 Documento de requisitos detallados
2. 🏗️ Arquitectura técnica
3. 📅 Plan de desarrollo por fases
4. 🎨 Diseño de pantallas (wireframes)
