-- =====================================================
-- MIGRACIÓN: Campos adicionales de Seguridad Social
-- Fecha: 2026-01-10
-- Descripción: Agrega campos completos de seguridad social colombiana
-- =====================================================

-- Agregar columnas de seguridad social que faltan
ALTER TABLE empleados ADD COLUMN codigo_eps TEXT;
ALTER TABLE empleados ADD COLUMN codigo_arl TEXT;
ALTER TABLE empleados ADD COLUMN clase_riesgo_arl INTEGER DEFAULT 1;
ALTER TABLE empleados ADD COLUMN codigo_afp TEXT;
ALTER TABLE empleados ADD COLUMN caja_compensacion TEXT;
ALTER TABLE empleados ADD COLUMN codigo_caja_compensacion TEXT;

-- Verificar que se agregaron correctamente
-- SELECT sql FROM sqlite_master WHERE type='table' AND name='empleados';
