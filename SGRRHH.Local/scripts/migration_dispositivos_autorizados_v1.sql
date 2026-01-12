-- ============================================================================
-- MIGRACIÓN: Sistema de Dispositivos Autorizados (Login sin contraseña)
-- Versión: 1.0
-- Fecha: 2026-01-11
-- Descripción: Permite a usuarios autorizar dispositivos para login automático
-- ============================================================================

-- Tabla de dispositivos autorizados por usuario
CREATE TABLE IF NOT EXISTS dispositivos_autorizados (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    usuario_id INTEGER NOT NULL,
    device_token TEXT NOT NULL UNIQUE,
    nombre_dispositivo TEXT NOT NULL,
    huella_navegador TEXT,
    ip_autorizacion TEXT,
    fecha_creacion DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion DATETIME,
    fecha_ultimo_uso DATETIME,
    fecha_expiracion DATETIME,
    activo INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (usuario_id) REFERENCES usuarios(id) ON DELETE CASCADE
);

-- Índices para búsqueda rápida
CREATE INDEX IF NOT EXISTS idx_dispositivos_token ON dispositivos_autorizados(device_token);
CREATE INDEX IF NOT EXISTS idx_dispositivos_usuario ON dispositivos_autorizados(usuario_id);
CREATE INDEX IF NOT EXISTS idx_dispositivos_activo ON dispositivos_autorizados(activo);

-- ============================================================================
-- NOTAS DE IMPLEMENTACIÓN:
-- 
-- 1. device_token: UUID v4 + hash SHA256 del user-agent (único por dispositivo)
-- 2. nombre_dispositivo: Descripción amigable (ej: "PC Oficina", "Laptop Campo")
-- 3. huella_navegador: User-Agent para identificar el navegador
-- 4. fecha_expiracion: Opcional, para tokens que expiran (ej: 30 días)
-- 5. El admin puede ver/revocar dispositivos de cualquier usuario
-- 6. Cada usuario puede ver/revocar sus propios dispositivos
-- ============================================================================
