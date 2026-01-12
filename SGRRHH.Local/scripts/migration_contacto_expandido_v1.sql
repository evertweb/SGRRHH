-- =====================================================
-- MIGRACIÓN: Campos de Contacto Expandido para Empleados
-- Versión: 1.0
-- Fecha: Enero 2026
-- Descripción: Agrega campos de contacto, información médica
--              y contactos de emergencia expandidos
-- =====================================================
-- INSTRUCCIONES:
-- Este script es IDEMPOTENTE. Puede ejecutarse múltiples veces.
-- SQLite no soporta "IF NOT EXISTS" en ALTER TABLE, por lo que
-- cada columna se agrega en un statement separado.
-- Los errores de "duplicate column name" se ignoran silenciosamente.
-- =====================================================

-- =====================================================
-- CONTACTO EXPANDIDO
-- =====================================================

-- Teléfono celular (principal de contacto)
ALTER TABLE empleados ADD COLUMN telefono_celular TEXT;

-- Teléfono fijo (alternativo)
ALTER TABLE empleados ADD COLUMN telefono_fijo TEXT;

-- WhatsApp (comunicación moderna, grupos de trabajo)
ALTER TABLE empleados ADD COLUMN whatsapp TEXT;

-- Municipio/Ciudad (ubicación geográfica)
ALTER TABLE empleados ADD COLUMN municipio TEXT;

-- Barrio/Vereda (detalle de ubicación, común en zonas rurales)
ALTER TABLE empleados ADD COLUMN barrio TEXT;

-- =====================================================
-- INFORMACIÓN MÉDICA (EMERGENCIAS)
-- Crítico para trabajo en campo forestal
-- =====================================================

-- Tipo de sangre (A+, A-, B+, B-, O+, O-, AB+, AB-)
ALTER TABLE empleados ADD COLUMN tipo_sangre TEXT;

-- Alergias conocidas
ALTER TABLE empleados ADD COLUMN alergias TEXT;

-- Condiciones médicas (diabetes, hipertensión, etc.)
ALTER TABLE empleados ADD COLUMN condiciones_medicas TEXT;

-- Medicamentos que toma actualmente
ALTER TABLE empleados ADD COLUMN medicamentos_actuales TEXT;

-- =====================================================
-- CONTACTOS DE EMERGENCIA EXPANDIDOS
-- =====================================================

-- Parentesco del primer contacto de emergencia (Esposa, Padre, Hijo, etc.)
ALTER TABLE empleados ADD COLUMN parentesco_contacto_emergencia TEXT;

-- Segundo teléfono del primer contacto de emergencia
ALTER TABLE empleados ADD COLUMN telefono_emergencia_2 TEXT;

-- Segundo contacto de emergencia (nombre completo)
ALTER TABLE empleados ADD COLUMN contacto_emergencia_2 TEXT;

-- Teléfono principal del segundo contacto
ALTER TABLE empleados ADD COLUMN telefono_emergencia_2_contacto_2 TEXT;

-- Parentesco del segundo contacto de emergencia
ALTER TABLE empleados ADD COLUMN parentesco_contacto_emergencia_2 TEXT;

-- Teléfono alternativo del segundo contacto
ALTER TABLE empleados ADD COLUMN telefono_emergencia_2_alternativo TEXT;

-- =====================================================
-- VERIFICACIÓN
-- =====================================================
-- Para verificar que las columnas se agregaron correctamente:
-- SELECT sql FROM sqlite_master WHERE type='table' AND name='empleados';

-- =====================================================
-- FIN DE MIGRACIÓN
-- =====================================================
