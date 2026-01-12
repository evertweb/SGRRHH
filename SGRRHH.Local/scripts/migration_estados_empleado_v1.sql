-- ============================================================================
-- MIGRACIÓN: Estados de Empleados Extendidos
-- Versión: 1.0
-- Fecha: Enero 2026
-- Descripción: Documenta los valores de estados de empleado soportados y agrega
--              el nuevo estado EnIncapacidad (valor 7).
-- ============================================================================

-- ESTADOS DE EMPLEADO (Enum EstadoEmpleado):
-- 0 = PendienteAprobacion - Creado por Operador, requiere aprobación
-- 1 = Activo              - Empleado activo en funciones
-- 2 = EnVacaciones        - Empleado en periodo de vacaciones
-- 3 = EnLicencia          - Empleado en licencia no remunerada o especial
-- 4 = Suspendido          - Empleado con suspensión disciplinaria
-- 5 = Retirado            - Empleado desvinculado (estado final)
-- 6 = Rechazado           - Solicitud rechazada por Aprobador (estado final)
-- 7 = EnIncapacidad       - Empleado con incapacidad médica activa (NUEVO)

-- No se requieren cambios en la estructura de la tabla empleados.
-- El campo 'estado' ya es INTEGER y acepta el nuevo valor 7.

-- MATRIZ DE TRANSICIONES PERMITIDAS:
-- PendienteAprobacion (0) → Activo (1), Rechazado (6) [Solo Aprobador/Administrador]
-- Activo (1)              → EnVacaciones (2), EnLicencia (3), EnIncapacidad (7) [Todos]
--                         → Suspendido (4), Retirado (5) [Solo Aprobador/Administrador]
-- EnVacaciones (2)        → Activo (1) [Todos]
-- EnLicencia (3)          → Activo (1) [Todos]
-- EnIncapacidad (7)       → Activo (1) [Todos]
-- Suspendido (4)          → Activo (1) [Solo Administrador]
--                         → Retirado (5) [Solo Aprobador/Administrador]
-- Retirado (5)            → Ninguno (Estado Final)
-- Rechazado (6)           → Ninguno (Estado Final)

-- PERMISOS POR ROL EN MODO CORPORATIVO:
-- Operador:      Solo puede crear empleados (estado inicial: PendienteAprobacion)
-- Aprobador:     Puede crear (Activo) y aprobar/rechazar pendientes
-- Administrador: Acceso completo a todas las transiciones

-- Esta migración es solo documentación. No hay cambios requeridos en la BD.
-- El enum EstadoEmpleado.cs ya incluye el valor 7 (EnIncapacidad).
