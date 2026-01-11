# PROMPT: Corrección Completa de Nomenclatura SQL a snake_case

## 🎯 Objetivo

Estandarizar **TODAS** las consultas SQL del proyecto para que usen nombres de tablas y columnas en **snake_case en inglés**, eliminando cualquier referencia a PascalCase o español.

---

## 📋 Contexto del Problema

El proyecto SGRRHH.Local migró su base de datos SQLite de nomenclatura mixta (PascalCase español) a **snake_case inglés**. Sin embargo, múltiples archivos de código C# aún contienen consultas SQL con la nomenclatura antigua.

### Errores típicos encontrados:
```
SQLite Error 1: 'no such column: AreaHectareas'
SQLite Error 1: 'no such table: EspeciesForestales'
SQLite Error 1: 'no such column: a.CategoriaId'
```

---

## 🔍 Instrucciones para el Agente

### PASO 1: Investigar la Estructura Actual de la Base de Datos

Ejecutar los siguientes comandos para obtener la estructura real de cada tabla:

```powershell
sqlite3 "C:\SGRRHH\Data\sgrrhh.db" ".tables"
```

Para cada tabla, obtener sus columnas:
```powershell
sqlite3 "C:\SGRRHH\Data\sgrrhh.db" "PRAGMA table_info(nombre_tabla)"
```

### PASO 2: Buscar Archivos con Consultas SQL

Buscar **PROFUNDAMENTE** en todo el proyecto archivos que contengan consultas SQL. Ubicaciones prioritarias:

- `SGRRHH.Local.Infrastructure/Repositories/` - Todos los repositorios
- `SGRRHH.Local.Infrastructure/Services/` - Todos los servicios
- `SGRRHH.Local.Server/Components/Pages/` - Componentes Blazor con queries inline

Patrones de búsqueda:
```
- Archivos *.cs con "SELECT", "INSERT", "UPDATE", "DELETE", "FROM", "JOIN"
- Strings SQL multilínea (@"...")
- Referencias a tablas PascalCase
```

### PASO 3: Mapeo de Nomenclatura

#### Tablas (PascalCase → snake_case):
| Antiguo | Nuevo |
|---------|-------|
| Proyectos | proyectos |
| Empleados | empleados |
| ProyectosEmpleados | proyectos_empleados |
| RegistrosDiarios | registros_diarios |
| DetallesActividades | detalles_actividad |
| Actividades | actividades |
| EspeciesForestales | especies_forestales |
| CategoriasActividades | activity_categories |
| TiposPermiso | tipos_permiso |
| Permisos | permisos |
| Incapacidades | incapacidades |
| Vacaciones | vacaciones |
| Contratos | contratos |
| Departamentos | departamentos |
| Cargos | cargos |
| Usuarios | usuarios |
| ConfiguracionSistema | configuracion_sistema |
| AuditLogs | audit_logs |
| ScanProfiles | scan_profiles |

#### Columnas Comunes (PascalCase → snake_case):
| Antiguo | Nuevo |
|---------|-------|
| Id | id |
| EmpleadoId | empleado_id |
| ProyectoId | proyecto_id |
| ActividadId | actividad_id |
| RegistroDiarioId | registro_diario_id |
| CategoriaId | category_id |
| EspecieId | especie_id |
| AreaHectareas | area_hectareas |
| TotalHorasTrabajadas | total_horas_trabajadas |
| CostoManoObraAcumulado | costo_mano_obra_acumulado |
| FechaDesasignacion | fecha_desasignacion |
| FechaCreacion | fecha_creacion |
| FechaModificacion | fecha_modificacion |
| FechaInicio | fecha_inicio |
| FechaFin | fecha_fin |
| TipoProyecto | tipo_proyecto |
| NombreComun | nombre_comun |
| SalarioBase | salario_base |
| RendimientoEsperado | expected_yield |
| UnidadMedida | unit_of_measure |
| UnidadAbreviatura | unit_abbreviation |
| CategoriaTexto | category_text |
| Activo | activo |
| Estado | estado |
| Codigo | codigo |
| Nombre | nombre |
| Descripcion | descripcion |
| Horas | horas |
| Cantidad | cantidad |
| Fecha | fecha |

---

## ⚠️ Reglas Críticas

### 1. SOLO modificar strings SQL
- **NO** cambiar nombres de propiedades C# (ej: `entity.EmpleadoId` debe mantenerse)
- **NO** cambiar nombres de parámetros (ej: `@EmpleadoId` puede mantenerse porque Dapper mapea automáticamente)
- **SÍ** cambiar nombres de columnas en SELECT, WHERE, JOIN, ORDER BY, GROUP BY

### 2. Usar alias cuando sea necesario
```sql
-- ANTES
SELECT p.AreaHectareas FROM Proyectos p

-- DESPUÉS (con alias para mapear a propiedad C#)
SELECT p.area_hectareas as AreaHectareas FROM proyectos p
```

### 3. Verificar JOINs
```sql
-- ANTES
LEFT JOIN EspeciesForestales ef ON p.EspecieId = ef.Id

-- DESPUÉS
LEFT JOIN especies_forestales ef ON p.especie_id = ef.id
```

### 4. Mantener compatibilidad con COALESCE solo donde sea necesario
Si una tabla tiene AMBAS columnas (legacy y nueva), usar COALESCE. Si solo tiene snake_case, usar directamente.

---

## 🔧 Archivos Conocidos con Problemas

Estos archivos HAN SIDO identificados con errores, pero **debes buscar más**:

1. `ReporteProductividadService.cs` - Múltiples consultas con PascalCase
2. `IncapacidadRepository.cs` - Referencias a columnas antiguas
3. `PermisoRepository.cs` - Referencias a columnas antiguas
4. `ProyectoRepository.cs` - Algunas columnas PascalCase
5. `ActividadRepository.cs` - Ya corregido parcialmente

---

## ✅ Proceso de Validación

### Después de cada corrección:

1. **Compilar**:
```powershell
cd "c:\Users\evert\Documents\rrhh\SGRRHH.Local"
dotnet build 2>&1 | Select-String -Pattern "error|Build succeeded|Build FAILED"
```

2. **Ejecutar la aplicación** y verificar que no hay errores SQLite:
```powershell
dotnet run --project SGRRHH.Local.Server 2>&1 | Select-String -Pattern "SQLite Error|no such column|no such table"
```

3. **Probar cada módulo** navegando en la aplicación:
   - Dashboard de productividad
   - Lista de empleados
   - Lista de proyectos
   - Actividades
   - Permisos
   - Incapacidades

---

## 📝 Entregables Esperados

1. Todos los archivos con consultas SQL corregidos
2. Compilación exitosa sin errores
3. Aplicación funcionando sin errores SQLite
4. Lista de archivos modificados

---

## 🚫 Lo que NO debes hacer

- NO crear archivos de documentación adicionales
- NO modificar el schema de la base de datos
- NO cambiar la lógica de negocio
- NO renombrar propiedades C# ni DTOs
- NO modificar migraciones existentes

---

## 📍 Ubicación del Proyecto

```
c:\Users\evert\Documents\rrhh\SGRRHH.Local\
├── SGRRHH.Local.Infrastructure\
│   ├── Repositories\  ← BUSCAR AQUÍ
│   ├── Services\      ← BUSCAR AQUÍ
│   └── Data\          ← REVISAR DatabaseInitializer.cs como referencia
├── SGRRHH.Local.Server\
│   └── Components\Pages\  ← BUSCAR consultas inline
└── SGRRHH.Local.Domain\
    └── DTOs\  ← REFERENCIA para nombres de propiedades C#
```

---

## 🏁 Criterio de Éxito

La tarea está completa cuando:
1. `dotnet build` compila sin errores
2. La aplicación inicia sin errores SQLite en la consola
3. Todas las páginas cargan correctamente (Dashboard, Empleados, Proyectos, Actividades, Permisos, Incapacidades)

---

*Prompt creado: Enero 2026*
