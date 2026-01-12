-- =====================================================
-- MIGRACIÓN: Asignación de Departamentos a Cargos
-- Fecha: Enero 2026
-- Descripción: Establece la relación cargo-departamento
--              y agrega nuevos cargos (Operario Forestal, Tractorista)
-- =====================================================

-- =====================================================
-- PASO 1: Asignar departamentos a cargos existentes
-- =====================================================

-- Dirección General (departamento_id = 1)
UPDATE cargos SET departamento_id = 1 WHERE nombre = 'Gerente General';

-- Operaciones Forestales (departamento_id = 2)
UPDATE cargos SET departamento_id = 2 WHERE nombre = 'Ingeniero Forestal';
UPDATE cargos SET departamento_id = 2 WHERE nombre = 'Supervisor de Campo';
UPDATE cargos SET departamento_id = 2 WHERE nombre = 'Operario Forestal - Motosierrista';
UPDATE cargos SET departamento_id = 2 WHERE nombre = 'Auxiliar de Campo';

-- Administración y Finanzas (departamento_id = 3)
UPDATE cargos SET departamento_id = 3 WHERE nombre = 'Auxiliar Administrativo';
UPDATE cargos SET departamento_id = 3 WHERE nombre = 'Contador';

-- SST y Ambiente (departamento_id = 4)
UPDATE cargos SET departamento_id = 4 WHERE nombre = 'Coordinador SST';

-- Logística y Maquinaria (departamento_id = 5)
UPDATE cargos SET departamento_id = 5 WHERE nombre = 'Conductor';
UPDATE cargos SET departamento_id = 5 WHERE nombre = 'Almacenista';
UPDATE cargos SET departamento_id = 5 WHERE nombre = 'Mecánico';
UPDATE cargos SET departamento_id = 5 WHERE nombre LIKE 'Mec_nico';

-- Vivero (departamento_id = 6)
UPDATE cargos SET departamento_id = 6 WHERE nombre = 'Viverista';

-- =====================================================
-- PASO 2: Insertar nuevos cargos
-- =====================================================

-- Operario Forestal (sin especialización) - Operaciones Forestales
INSERT OR IGNORE INTO cargos (codigo, nombre, descripcion, nivel, departamento_id, requisitos, activo)
VALUES ('OPE06', 'Operario Forestal', 'Actividades silviculturales generales sin especialización', 1, 2, 'Primaria o experiencia en campo', 1);

-- Tractorista - Logística y Maquinaria
INSERT OR IGNORE INTO cargos (codigo, nombre, descripcion, nivel, departamento_id, requisitos, activo)
VALUES ('LOG03', 'Tractorista', 'Operación de tractores para actividades forestales y agrícolas', 2, 5, 'Licencia de conducción, experiencia en maquinaria agrícola', 1);

-- =====================================================
-- VERIFICACIÓN
-- =====================================================
-- Ejecutar para verificar la asignación:
-- SELECT c.id, c.codigo, c.nombre, d.nombre as departamento 
-- FROM cargos c 
-- LEFT JOIN departamentos d ON c.departamento_id = d.id 
-- WHERE c.activo = 1 
-- ORDER BY d.id, c.nombre;
