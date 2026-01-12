-- ============================================================================
-- MIGRACIÓN: Modo Corporativo (Sistema de Roles y Permisos)
-- Versión: 1.0
-- Fecha: Enero 2026
-- Descripción: Agrega soporte para el toggle de Modo Corporativo que 
--              activa/desactiva las restricciones de roles en el sistema.
-- ============================================================================

-- La tabla configuracion_sistema ya existe, solo necesitamos insertar 
-- la configuración inicial para modo_corporativo si no existe.

-- Insertar configuración inicial de Modo Corporativo (desactivado por defecto)
INSERT OR IGNORE INTO configuracion_sistema (clave, valor, descripcion, categoria, activo)
VALUES (
    'modo_corporativo',
    '0',
    'Activa o desactiva las restricciones de roles. 0=Acceso libre, 1=Restricciones activas',
    'Seguridad',
    1
);

-- Nota: Con valor '0' (desactivado), el sistema funciona exactamente como antes.
-- Solo cuando el admin active el Modo Corporativo ('1'), se aplicarán las restricciones.
