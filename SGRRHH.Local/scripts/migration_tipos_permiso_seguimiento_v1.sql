-- ============================================================================
-- MIGRACIÓN: Campos de Seguimiento en Tipos de Permiso v1
-- Fecha: 2026-01-09
-- Descripción: Agrega campos de configuración de seguimiento a tipos_permiso
-- ============================================================================

-- Tipo de resolución por defecto: 0=PendienteDefinir, 1=Remunerado, 2=Descontado, 3=Compensado
ALTER TABLE tipos_permiso ADD COLUMN tipo_resolucion_por_defecto INTEGER DEFAULT 1;

-- Días límite para entregar documento después de aprobar (0 = sin límite)
ALTER TABLE tipos_permiso ADD COLUMN dias_limite_documento INTEGER DEFAULT 7;

-- Días límite para completar compensación de horas (0 = sin límite)
ALTER TABLE tipos_permiso ADD COLUMN dias_limite_compensacion INTEGER DEFAULT 30;

-- Horas a compensar por cada día de permiso
ALTER TABLE tipos_permiso ADD COLUMN horas_compensar_por_dia INTEGER DEFAULT 8;

-- Indica si este tipo genera descuento automático
ALTER TABLE tipos_permiso ADD COLUMN genera_descuento INTEGER DEFAULT 0;

-- Porcentaje del salario diario a descontar (si aplica)
ALTER TABLE tipos_permiso ADD COLUMN porcentaje_descuento REAL;


-- ============================================================================
-- ACTUALIZAR TIPOS EXISTENTES CON VALORES POR DEFECTO INTELIGENTES
-- ============================================================================

-- Los que requieren documento se configuran como remunerados con 7 días para entregar
UPDATE tipos_permiso 
SET tipo_resolucion_por_defecto = 1,
    dias_limite_documento = 7
WHERE requiere_documento = 1;

-- Los compensables se configuran como compensados con 30 días límite
UPDATE tipos_permiso 
SET tipo_resolucion_por_defecto = 3,
    dias_limite_compensacion = 30,
    horas_compensar_por_dia = 8
WHERE es_compensable = 1;

-- Los que no requieren documento ni son compensables, remunerados sin seguimiento
UPDATE tipos_permiso 
SET tipo_resolucion_por_defecto = 1,
    dias_limite_documento = 0
WHERE requiere_documento = 0 AND es_compensable = 0;


-- ============================================================================
-- FIN DE MIGRACIÓN
-- ============================================================================
