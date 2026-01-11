-- =====================================================
-- MIGRACIÓN: Sistema de Notificaciones Profesional
-- Versión: 1.0
-- Fecha: 2026-01-10
-- Descripción: Tabla para persistir notificaciones con
--              soporte para múltiples usuarios y tipos
-- =====================================================

-- Tabla principal de notificaciones
CREATE TABLE IF NOT EXISTS Notificaciones (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    
    -- Usuario destinatario (NULL = todos los aprobadores)
    UsuarioDestinoId INTEGER NULL,
    
    -- Contenido
    Titulo TEXT NOT NULL,
    Mensaje TEXT NOT NULL,
    Icono TEXT DEFAULT '📌',
    
    -- Clasificación
    Tipo TEXT NOT NULL DEFAULT 'Info',  -- Info, Success, Warning, Error, PermisoNuevo, PermisoAprobado, etc.
    Categoria TEXT NOT NULL DEFAULT 'Sistema',  -- Sistema, Permiso, Vacacion, Incapacidad, Contrato, etc.
    Prioridad INTEGER DEFAULT 0,  -- 0=Normal, 1=Alta, 2=Urgente
    
    -- Enlace para navegación
    Link TEXT NULL,
    
    -- Referencia a entidad relacionada (opcional)
    EntidadTipo TEXT NULL,  -- 'Permiso', 'Vacacion', 'Incapacidad', etc.
    EntidadId INTEGER NULL,
    
    -- Estado
    Leida INTEGER DEFAULT 0,
    FechaLectura TEXT NULL,
    
    -- Auditoría
    FechaCreacion TEXT NOT NULL DEFAULT (datetime('now', 'localtime')),
    CreadoPor TEXT NULL,
    
    -- Expiración (opcional)
    FechaExpiracion TEXT NULL,
    
    FOREIGN KEY (UsuarioDestinoId) REFERENCES Usuarios(Id) ON DELETE CASCADE
);

-- Índices para optimizar consultas frecuentes
CREATE INDEX IF NOT EXISTS IX_Notificaciones_UsuarioDestino ON Notificaciones(UsuarioDestinoId);
CREATE INDEX IF NOT EXISTS IX_Notificaciones_Leida ON Notificaciones(Leida);
CREATE INDEX IF NOT EXISTS IX_Notificaciones_Tipo ON Notificaciones(Tipo);
CREATE INDEX IF NOT EXISTS IX_Notificaciones_FechaCreacion ON Notificaciones(FechaCreacion DESC);
CREATE INDEX IF NOT EXISTS IX_Notificaciones_Entidad ON Notificaciones(EntidadTipo, EntidadId);

-- Tabla para preferencias de notificación por usuario
CREATE TABLE IF NOT EXISTS PreferenciasNotificacion (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioId INTEGER NOT NULL UNIQUE,
    
    -- Qué tipos de notificaciones recibir
    RecibirPermisos INTEGER DEFAULT 1,
    RecibirVacaciones INTEGER DEFAULT 1,
    RecibirIncapacidades INTEGER DEFAULT 1,
    RecibirSistema INTEGER DEFAULT 1,
    
    -- Configuración de sonido/visual
    SonidoHabilitado INTEGER DEFAULT 1,
    MostrarEnEscritorio INTEGER DEFAULT 0,
    
    -- Frecuencia de resumen por email (futuro)
    ResumenEmail TEXT DEFAULT 'Nunca',  -- 'Nunca', 'Diario', 'Semanal'
    
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE CASCADE
);

-- Vista para notificaciones no leídas con información del usuario
CREATE VIEW IF NOT EXISTS vw_NotificacionesPendientes AS
SELECT 
    n.*,
    u.NombreCompleto as UsuarioDestinoNombre,
    CASE 
        WHEN n.FechaCreacion >= datetime('now', 'localtime', '-1 hour') THEN 'ahora'
        WHEN n.FechaCreacion >= datetime('now', 'localtime', '-24 hours') THEN 'hoy'
        WHEN n.FechaCreacion >= datetime('now', 'localtime', '-7 days') THEN 'semana'
        ELSE 'antiguo'
    END as Antigüedad
FROM Notificaciones n
LEFT JOIN Usuarios u ON n.UsuarioDestinoId = u.Id
WHERE n.Leida = 0
  AND (n.FechaExpiracion IS NULL OR n.FechaExpiracion > datetime('now', 'localtime'))
ORDER BY n.Prioridad DESC, n.FechaCreacion DESC;

-- =====================================================
-- DATOS INICIALES DE PRUEBA (opcional)
-- =====================================================
-- INSERT INTO Notificaciones (Titulo, Mensaje, Tipo, Categoria, Icono)
-- VALUES ('Sistema Actualizado', 'El sistema de notificaciones ha sido mejorado', 'Info', 'Sistema', '🔔');

PRAGMA table_info(Notificaciones);
