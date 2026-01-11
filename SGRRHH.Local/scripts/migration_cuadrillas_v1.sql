-- =====================================================
-- MIGRACIÓN: Campos adicionales para ProyectoEmpleado (Gestión de Cuadrillas)
-- Versión: 1.0
-- Fecha: 2026-01-09
-- Descripción: Agrega campos para gestión de cuadrillas forestales
-- =====================================================

-- Verificar estructura actual
SELECT 'Estructura actual de proyectos_empleados:' as info;
SELECT name, type FROM pragma_table_info('proyectos_empleados');

-- Agregar columnas a proyectos_empleados
-- Rol como enum
ALTER TABLE proyectos_empleados ADD COLUMN rol_enum INTEGER;

-- Si es líder de cuadrilla
ALTER TABLE proyectos_empleados ADD COLUMN es_lider_cuadrilla INTEGER DEFAULT 0;

-- Porcentaje de dedicación al proyecto (0-100)
ALTER TABLE proyectos_empleados ADD COLUMN porcentaje_dedicacion INTEGER DEFAULT 100;

-- Tipo de vinculación (Permanente, Temporal, Por Tarea)
ALTER TABLE proyectos_empleados ADD COLUMN tipo_vinculacion TEXT;

-- Motivo de desasignación
ALTER TABLE proyectos_empleados ADD COLUMN motivo_desasignacion TEXT;

-- Métricas de trabajo
ALTER TABLE proyectos_empleados ADD COLUMN total_horas_trabajadas REAL DEFAULT 0;
ALTER TABLE proyectos_empleados ADD COLUMN total_jornales INTEGER DEFAULT 0;
ALTER TABLE proyectos_empleados ADD COLUMN ultima_fecha_trabajo TEXT;
ALTER TABLE proyectos_empleados ADD COLUMN dias_trabajados INTEGER DEFAULT 0;

-- Crear índices para mejorar rendimiento
CREATE INDEX IF NOT EXISTS IX_proyectos_empleados_proyecto_id ON proyectos_empleados(proyecto_id);
CREATE INDEX IF NOT EXISTS IX_proyectos_empleados_empleado_id ON proyectos_empleados(empleado_id);
CREATE INDEX IF NOT EXISTS IX_proyectos_empleados_fecha_desasignacion ON proyectos_empleados(fecha_desasignacion);
CREATE INDEX IF NOT EXISTS IX_proyectos_empleados_rol_enum ON proyectos_empleados(rol_enum);
CREATE INDEX IF NOT EXISTS IX_proyectos_empleados_es_lider ON proyectos_empleados(es_lider_cuadrilla);

-- Verificación final
SELECT 'Migración completada. Nueva estructura de proyectos_empleados:' as info;
SELECT name, type FROM pragma_table_info('proyectos_empleados');

SELECT 'Total de índices creados:' as info, COUNT(*) as total FROM sqlite_master 
WHERE type='index' AND tbl_name='proyectos_empleados';
