-- ============================================================================
-- MIGRACIÓN: Usuarios de prueba para testing de roles
-- Fecha: 2025-01-XX
-- Descripción: Crea usuarios de prueba con diferentes roles para probar
--              el sistema de permisos en Modo Corporativo.
-- ============================================================================

-- IMPORTANTE: Este script es para PRUEBAS. No usar en producción con estos passwords.

-- Verificar que no existan antes de insertar
-- (evita duplicados si se ejecuta múltiples veces)

-- Usuario: secretaria (Rol = 3 = Operador)
-- Password: secretaria123
INSERT OR IGNORE INTO usuarios (
    username,
    password_hash,
    nombre_completo,
    email,
    rol,
    activo,
    fecha_creacion
) VALUES (
    'secretaria',
    '$2a$11$0SF.0dDY/mUNHoR1pW2r0OSdYuWbkrrKa4JAnlmBLhLjScx6/KIcW',
    'María Secretaria (Prueba)',
    'secretaria@prueba.local',
    3,  -- Operador
    1,
    datetime('now')
);

-- Usuario: ingeniera (Rol = 2 = Aprobador)
-- Password: ingeniera123
INSERT OR IGNORE INTO usuarios (
    username,
    password_hash,
    nombre_completo,
    email,
    rol,
    activo,
    fecha_creacion
) VALUES (
    'ingeniera',
    '$2a$11$EnVeznqyCWRal7ECcTlPhe9E6nVtOl.dlY0bo2BWz1B/HizPyey6G',
    'Laura Ingeniera (Prueba)',
    'ingeniera@prueba.local',
    2,  -- Aprobador
    1,
    datetime('now')
);

-- Mostrar usuarios creados
SELECT id, username, nombre_completo, rol, 
    CASE rol 
        WHEN 1 THEN 'Administrador'
        WHEN 2 THEN 'Aprobador'
        WHEN 3 THEN 'Operador'
    END as rol_nombre
FROM usuarios 
WHERE username IN ('secretaria', 'ingeniera', 'admin')
ORDER BY rol;

-- ============================================================================
-- INSTRUCCIONES DE PRUEBA:
-- ============================================================================
--
-- 1. Activar Modo Corporativo:
--    - Login como admin
--    - Ir a Configuración → Seguridad
--    - Activar "Modo Corporativo"
--
-- 2. Probar con secretaria (Operador):
--    - Login: secretaria / secretaria123
--    - Puede: Ver, Crear, Editar empleados
--    - NO puede: Eliminar, Aprobar, Rechazar
--
-- 3. Probar con ingeniera (Aprobador):
--    - Login: ingeniera / ingeniera123
--    - Puede: Ver, Crear, Editar, Aprobar, Rechazar
--    - NO puede: Eliminar (solo admin)
--
-- 4. Flujo de aprobación:
--    a) secretaria crea un empleado nuevo → Estado: PendienteAprobacion
--    b) ingeniera lo aprueba → Estado: Activo
--
-- ============================================================================
