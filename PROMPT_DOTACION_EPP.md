# PROMPT: Implementación de Módulo Dotación y EPP

## 📋 CONTEXTO DEL PROBLEMA

La empresa forestal requiere cumplir con el **Artículo 230 del Código Sustantivo del Trabajo colombiano** que obliga a entregar dotación a empleados que devenguen hasta 2 SMMLV. Además, debe gestionar la entrega de Elementos de Protección Personal (EPP) para seguridad en campo.

Actualmente NO existe ningún módulo para:
- Registrar tallas de empleados
- Controlar entregas de dotación
- Programar próximas entregas
- Gestionar EPP por actividad
- Obtener firmas digitales de recibido

## 🎯 OBJETIVOS

1. Crear tab "DOTACIÓN Y EPP" en expediente del empleado con formulario inline editable
2. Registrar tallas de empleado (ropa, calzado, guantes)
3. Gestionar historial de entregas con fechas y elementos
4. Programar próximas entregas automáticamente (cada 4 meses x3 al año)
5. Vincular actas de entrega escaneadas desde módulo documentos
6. Validar cumplimiento legal de dotación

---

## 📊 CAMBIOS EN BASE DE DATOS

### 1. Tabla: tallas_empleado

```sql
-- Script: migration_dotacion_epp_v1.sql

-- ====================================================================
-- MIGRACIÓN: Módulo de Dotación y EPP
-- Versión: 1.0
-- Fecha: Enero 2026
-- Descripción: Sistema de control de dotación y elementos de protección
-- ====================================================================

-- 1. TABLA: TALLAS DEL EMPLEADO (1-1 con empleados)
CREATE TABLE IF NOT EXISTS tallas_empleado (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    empleado_id INTEGER NOT NULL UNIQUE,
    
    -- TALLAS ROPA
    talla_camisa TEXT, -- S, M, L, XL, XXL, XXXL
    talla_pantalon TEXT, -- 28, 30, 32, 34, 36, 38, 40, 42, 44
    talla_overall TEXT, -- S, M, L, XL, XXL
    talla_chaqueta TEXT, -- S, M, L, XL, XXL, XXXL
    
    -- TALLAS CALZADO
    talla_calzado_numero INTEGER, -- 36-46
    ancho_calzado TEXT, -- Normal, Ancho
    tipo_calzado_preferido TEXT, -- Bota, Zapato, Tenis
    
    -- TALLAS PROTECCIÓN
    talla_guantes TEXT, -- S, M, L, XL
    talla_casco TEXT, -- Ajustable, S, M, L
    talla_gafas TEXT, -- Universal, Graduadas
    
    -- OBSERVACIONES
    observaciones TEXT, -- Ej: "Pie ancho, necesita bota especial"
    
    -- AUDITORÍA
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL,
    fecha_modificacion TEXT,
    
    FOREIGN KEY (empleado_id) REFERENCES empleados(id) ON DELETE CASCADE
);

CREATE INDEX idx_tallas_empleado ON tallas_empleado(empleado_id);


-- 2. TABLA: ENTREGAS DE DOTACIÓN (1-N con empleados)
CREATE TABLE IF NOT EXISTS entregas_dotacion (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    empleado_id INTEGER NOT NULL,
    
    -- DATOS DE LA ENTREGA
    fecha_entrega TEXT NOT NULL, -- ISO: YYYY-MM-DD
    periodo TEXT NOT NULL, -- Ej: "2024-1" (año-periodo)
    tipo_entrega INTEGER NOT NULL DEFAULT 1, -- 1=Dotación Legal, 2=EPP, 3=Ambos
    numero_entrega_anual INTEGER, -- 1, 2, 3 (para dotación legal)
    
    -- ESTADO
    estado INTEGER NOT NULL DEFAULT 1, -- 1=Programada, 2=Entregada, 3=Parcial, 4=Cancelada
    fecha_entrega_real TEXT, -- Fecha en que se entregó (si estado=Entregada)
    
    -- DOCUMENTACIÓN
    documento_acta_id INTEGER, -- FK a documentos_empleados (acta firmada)
    observaciones TEXT,
    
    -- RESPONSABLES
    entregado_por_usuario_id INTEGER, -- Usuario que hizo la entrega
    entregado_por_nombre TEXT, -- Nombre del responsable
    
    -- AUDITORÍA
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL,
    fecha_modificacion TEXT,
    
    FOREIGN KEY (empleado_id) REFERENCES empleados(id) ON DELETE CASCADE,
    FOREIGN KEY (documento_acta_id) REFERENCES documentos_empleados(id) ON DELETE SET NULL,
    FOREIGN KEY (entregado_por_usuario_id) REFERENCES usuarios(id) ON DELETE SET NULL
);

CREATE INDEX idx_entregas_empleado ON entregas_dotacion(empleado_id);
CREATE INDEX idx_entregas_fecha ON entregas_dotacion(fecha_entrega);
CREATE INDEX idx_entregas_estado ON entregas_dotacion(estado, fecha_entrega);
CREATE INDEX idx_entregas_periodo ON entregas_dotacion(periodo);


-- 3. TABLA: DETALLE DE ELEMENTOS ENTREGADOS (1-N con entregas_dotacion)
CREATE TABLE IF NOT EXISTS detalle_entrega_dotacion (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    entrega_id INTEGER NOT NULL,
    
    -- ELEMENTO
    categoria_elemento INTEGER NOT NULL, -- 1=Camisa, 2=Pantalón, 3=Calzado, 4=EPP, etc.
    nombre_elemento TEXT NOT NULL, -- "Camisa manga larga", "Bota de seguridad", etc.
    cantidad INTEGER NOT NULL DEFAULT 1,
    talla TEXT, -- Talla específica del elemento
    
    -- CLASIFICACIÓN
    es_dotacion_legal INTEGER NOT NULL DEFAULT 0, -- 0=No, 1=Sí
    es_epp INTEGER NOT NULL DEFAULT 0, -- 0=No, 1=Sí (Elemento Protección Personal)
    
    -- DATOS ADICIONALES
    marca TEXT, -- Ej: "3M", "North Face"
    referencia TEXT, -- Código/referencia del producto
    valor_unitario REAL, -- Costo del elemento
    
    observaciones TEXT,
    
    -- AUDITORÍA
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL,
    
    FOREIGN KEY (entrega_id) REFERENCES entregas_dotacion(id) ON DELETE CASCADE
);

CREATE INDEX idx_detalle_entrega ON detalle_entrega_dotacion(entrega_id);
CREATE INDEX idx_detalle_categoria ON detalle_entrega_dotacion(categoria_elemento);


-- 4. TABLA: PLANTILLAS DE DOTACIÓN (por cargo/actividad)
CREATE TABLE IF NOT EXISTS plantillas_dotacion (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    
    nombre TEXT NOT NULL, -- "Dotación Operario Forestal", "EPP Altura"
    descripcion TEXT,
    tipo INTEGER NOT NULL DEFAULT 1, -- 1=Dotación Legal, 2=EPP, 3=Ambos
    
    -- APLICABILIDAD
    aplica_cargo_id INTEGER, -- FK a cargos (NULL = aplica a todos)
    aplica_departamento_id INTEGER, -- FK a departamentos
    
    -- PERIODICIDAD
    periodicidad_meses INTEGER DEFAULT 4, -- Cada cuántos meses se entrega
    
    -- AUDITORÍA
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL,
    fecha_modificacion TEXT,
    
    FOREIGN KEY (aplica_cargo_id) REFERENCES cargos(id) ON DELETE SET NULL,
    FOREIGN KEY (aplica_departamento_id) REFERENCES departamentos(id) ON DELETE SET NULL
);

CREATE INDEX idx_plantillas_cargo ON plantillas_dotacion(aplica_cargo_id);


-- 5. TABLA: ELEMENTOS DE PLANTILLA (1-N con plantillas_dotacion)
CREATE TABLE IF NOT EXISTS elementos_plantilla_dotacion (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    plantilla_id INTEGER NOT NULL,
    
    categoria_elemento INTEGER NOT NULL,
    nombre_elemento TEXT NOT NULL,
    cantidad INTEGER NOT NULL DEFAULT 1,
    es_obligatorio INTEGER NOT NULL DEFAULT 1, -- 0=Opcional, 1=Obligatorio
    
    activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL,
    
    FOREIGN KEY (plantilla_id) REFERENCES plantillas_dotacion(id) ON DELETE CASCADE
);

CREATE INDEX idx_elementos_plantilla ON elementos_plantilla_dotacion(plantilla_id);


-- ====================================================================
-- MIGRACIÓN AUTOMÁTICA: Crear entregas para dotaciones pasadas (si existen)
-- ====================================================================

-- Si ya hay empleados con fecha de ingreso, crear entregas retroactivas programadas
-- (Solo para empleados activos que devenguen <= 2 SMMLV)

INSERT INTO entregas_dotacion (
    empleado_id, 
    fecha_entrega, 
    periodo, 
    tipo_entrega, 
    numero_entrega_anual, 
    estado, 
    observaciones,
    fecha_creacion
)
SELECT 
    e.id,
    DATE(e.fecha_ingreso, '+' || (n.num * 4) || ' months') as fecha_entrega,
    CAST(strftime('%Y', DATE(e.fecha_ingreso, '+' || (n.num * 4) || ' months')) AS TEXT) || '-' || 
    CAST((n.num % 3) + 1 AS TEXT) as periodo,
    1, -- Dotación Legal
    (n.num % 3) + 1, -- Número de entrega en el año (1, 2, 3)
    CASE 
        WHEN DATE(e.fecha_ingreso, '+' || (n.num * 4) || ' months') <= DATE('now') THEN 2 -- Entregada (pasado)
        ELSE 1 -- Programada (futuro)
    END as estado,
    'Generado automáticamente en migración' as observaciones,
    datetime('now') as fecha_creacion
FROM empleados e
CROSS JOIN (
    SELECT 0 as num UNION SELECT 1 UNION SELECT 2 UNION 
    SELECT 3 UNION SELECT 4 UNION SELECT 5 UNION 
    SELECT 6 UNION SELECT 7 UNION SELECT 8
) n
WHERE e.estado IN (1, 2) -- Activo o En Prueba
  AND e.fecha_ingreso IS NOT NULL
  AND DATE(e.fecha_ingreso, '+' || (n.num * 4) || ' months') <= DATE('now', '+12 months')
  AND NOT EXISTS (
      SELECT 1 FROM entregas_dotacion ed 
      WHERE ed.empleado_id = e.id 
      AND ed.periodo = CAST(strftime('%Y', DATE(e.fecha_ingreso, '+' || (n.num * 4) || ' months')) AS TEXT) || '-' || CAST((n.num % 3) + 1 AS TEXT)
  );

-- Registrar cuántas entregas se generaron
SELECT 'Entregas de dotación generadas', COUNT(*) FROM entregas_dotacion WHERE observaciones LIKE '%migración%';

SELECT '✓ Migración completada exitosamente' as resultado;
```

---

## 🏗️ ENTIDADES Y ENUMS

### Enums

```csharp
// SGRRHH.Local.Domain/Enums/TallaCamisa.cs
namespace SGRRHH.Local.Domain.Enums;

public enum TallaCamisa
{
    S = 1,
    M = 2,
    L = 3,
    XL = 4,
    XXL = 5,
    XXXL = 6
}
```

```csharp
// SGRRHH.Local.Domain/Enums/TipoEntregaDotacion.cs
namespace SGRRHH.Local.Domain.Enums;

public enum TipoEntregaDotacion
{
    DotacionLegal = 1,
    EPP = 2,
    Ambos = 3
}
```

```csharp
// SGRRHH.Local.Domain/Enums/EstadoEntregaDotacion.cs
namespace SGRRHH.Local.Domain.Enums;

public enum EstadoEntregaDotacion
{
    Programada = 1,
    Entregada = 2,
    Parcial = 3,
    Cancelada = 4
}
```

```csharp
// SGRRHH.Local.Domain/Enums/CategoriaElementoDotacion.cs
namespace SGRRHH.Local.Domain.Enums;

public enum CategoriaElementoDotacion
{
    Camisa = 1,
    Pantalon = 2,
    Calzado = 3,
    Overall = 4,
    Chaqueta = 5,
    Casco = 6,
    Guantes = 7,
    GafasProteccion = 8,
    Botas = 9,
    ProtectorAuditivo = 10,
    Mascarilla = 11,
    Arnes = 12,
    ChalecoBrillante = 13,
    Otros = 99
}
```

### Entidades

```csharp
// SGRRHH.Local.Domain/Entities/TallasEmpleado.cs
namespace SGRRHH.Local.Domain.Entities;

public class TallasEmpleado : EntidadBase
{
    public int EmpleadoId { get; set; }
    
    // Tallas Ropa
    public string? TallaCamisa { get; set; } // S, M, L, XL, XXL, XXXL
    public string? TallaPantalon { get; set; } // 28, 30, 32, 34, 36, 38, 40, 42, 44
    public string? TallaOverall { get; set; }
    public string? TallaChaqueta { get; set; }
    
    // Tallas Calzado
    public int? TallaCalzadoNumero { get; set; } // 36-46
    public string? AnchoCalzado { get; set; } // Normal, Ancho
    public string? TipoCalzadoPreferido { get; set; } // Bota, Zapato, Tenis
    
    // Tallas Protección
    public string? TallaGuantes { get; set; } // S, M, L, XL
    public string? TallaCasco { get; set; } // Ajustable, S, M, L
    public string? TallaGafas { get; set; } // Universal, Graduadas
    
    public string? Observaciones { get; set; }
    
    // Navegación
    public Empleado? Empleado { get; set; }
}
```

```csharp
// SGRRHH.Local.Domain/Entities/EntregaDotacion.cs
namespace SGRRHH.Local.Domain.Entities;

public class EntregaDotacion : EntidadBase
{
    public int EmpleadoId { get; set; }
    
    public DateTime FechaEntrega { get; set; }
    public string Periodo { get; set; } = string.Empty; // "2024-1"
    public TipoEntregaDotacion TipoEntrega { get; set; }
    public int? NumeroEntregaAnual { get; set; } // 1, 2, 3
    
    public EstadoEntregaDotacion Estado { get; set; }
    public DateTime? FechaEntregaReal { get; set; }
    
    public int? DocumentoActaId { get; set; }
    public string? Observaciones { get; set; }
    
    public int? EntregadoPorUsuarioId { get; set; }
    public string? EntregadoPorNombre { get; set; }
    
    // Navegación
    public Empleado? Empleado { get; set; }
    public DocumentoEmpleado? DocumentoActa { get; set; }
    public List<DetalleEntregaDotacion> Detalles { get; set; } = new();
}
```

```csharp
// SGRRHH.Local.Domain/Entities/DetalleEntregaDotacion.cs
namespace SGRRHH.Local.Domain.Entities;

public class DetalleEntregaDotacion : EntidadBase
{
    public int EntregaId { get; set; }
    
    public CategoriaElementoDotacion CategoriaElemento { get; set; }
    public string NombreElemento { get; set; } = string.Empty;
    public int Cantidad { get; set; } = 1;
    public string? Talla { get; set; }
    
    public bool EsDotacionLegal { get; set; }
    public bool EsEPP { get; set; }
    
    public string? Marca { get; set; }
    public string? Referencia { get; set; }
    public decimal? ValorUnitario { get; set; }
    
    public string? Observaciones { get; set; }
    
    // Navegación
    public EntregaDotacion? Entrega { get; set; }
}
```

---

## 🔌 INTERFACES DE REPOSITORIOS

```csharp
// SGRRHH.Local.Domain/Interfaces/ITallasEmpleadoRepository.cs
namespace SGRRHH.Local.Domain.Interfaces;

public interface ITallasEmpleadoRepository : IRepository<TallasEmpleado>
{
    Task<TallasEmpleado?> GetByEmpleadoIdAsync(int empleadoId);
    Task<bool> EmpleadoTieneTallasRegistradasAsync(int empleadoId);
}
```

```csharp
// SGRRHH.Local.Domain/Interfaces/IEntregaDotacionRepository.cs
namespace SGRRHH.Local.Domain.Interfaces;

public interface IEntregaDotacionRepository : IRepository<EntregaDotacion>
{
    Task<IEnumerable<EntregaDotacion>> GetByEmpleadoIdAsync(int empleadoId);
    Task<IEnumerable<EntregaDotacion>> GetByEmpleadoIdWithDetallesAsync(int empleadoId);
    Task<EntregaDotacion?> GetByIdWithDetallesAsync(int id);
    Task<IEnumerable<EntregaDotacion>> GetProximasEntregasAsync(int diasAnticipacion = 30);
    Task<IEnumerable<EntregaDotacion>> GetEntregasPendientesAsync();
    Task<bool> EmpleadoTieneEntregaProgramadaAsync(int empleadoId, string periodo);
}
```

```csharp
// SGRRHH.Local.Domain/Interfaces/IDetalleEntregaDotacionRepository.cs
namespace SGRRHH.Local.Domain.Interfaces;

public interface IDetalleEntregaDotacionRepository : IRepository<DetalleEntregaDotacion>
{
    Task<IEnumerable<DetalleEntregaDotacion>> GetByEntregaIdAsync(int entregaId);
    Task DeleteByEntregaIdAsync(int entregaId);
}
```

---

## 💾 IMPLEMENTACIÓN DE REPOSITORIOS (DAPPER)

Implementar los 3 repositorios en `SGRRHH.Local.Infrastructure/Repositories/` siguiendo el patrón existente:
- Usar `DapperContext` para conexiones
- Mapear snake_case a PascalCase con clases helper `*Db`
- Conversión de enums a int (SQLite)
- Conversión de bool a int (0/1)
- Manejo de fechas ISO string

**IMPORTANTE:** Revisar `CuentaBancariaRepository.cs` como referencia para el patrón correcto.

---

## 🎨 COMPONENTE UI: TAB DOTACIÓN Y EPP

### Agregar en EmpleadoExpediente.razor

#### 1. Inyecciones de dependencias
```csharp
@inject ITallasEmpleadoRepository TallasRepo
@inject IEntregaDotacionRepository EntregaDotacionRepo
@inject IDetalleEntregaDotacionRepository DetalleEntregaRepo
```

#### 2. Botón de tab
```html
<button class="expediente-tab @(activeTab == "dotacion" ? "active" : "")" 
        @onclick='() => SetActiveTab("dotacion")'>
    DOTACIÓN Y EPP (@entregasDotacion.Count)
</button>
```

#### 3. Variables de estado
```csharp
// Dotación
private TallasEmpleado? tallasEmpleado = null;
private List<EntregaDotacion> entregasDotacion = new();
private int? entregaEditandoId = null;
private EntregaDotacion? entregaEnEdicion = null;
private List<DetalleEntregaDotacion> detallesEnEdicion = new();
private bool isSavingEntrega = false;
private bool showTallasForm = false;
private bool showDetalleEntregaModal = false;
```

#### 4. Cargar datos en OnInitializedAsync
```csharp
// Dentro de CargarDatos()
tallasEmpleado = await TallasRepo.GetByEmpleadoIdAsync(EmpleadoId);
entregasDotacion = (await EntregaDotacionRepo.GetByEmpleadoIdWithDetallesAsync(EmpleadoId))
    .OrderByDescending(e => e.FechaEntrega)
    .ToList();
```

#### 5. RenderFragment principal

**ESTILO INLINE IGUAL A INFORMACIÓN BANCARIA:**

- Lista de entregas en cards expandibles
- Botón "Registrar Tallas" si no existen (obligatorio antes de primera entrega)
- Botón "+ PROGRAMAR ENTREGA" flotante arriba-derecha
- Cada entrega muestra:
  - Header: Fecha, Periodo, Tipo, Estado con badge de color
  - Body: Lista de elementos con cantidades y tallas
  - Footer: Acta escaneada (link) o botón "📷 Escanear Acta"
  - Acciones: ✏️ Editar, 📋 Ver Detalles, ✅ Marcar Entregada, 🗑️ Eliminar

**Mini-formulario inline para tallas:**
- Grid 3 columnas con todos los campos de `TallasEmpleado`
- Botón "💾 Guardar Tallas"
- Aparece en modal o colapsable arriba de la lista

**Mini-formulario inline para nueva entrega:**
- Fecha programada
- Tipo (Dotación/EPP/Ambos)
- Botón "+ Agregar Elemento" (abre mini-modal)
- Lista de elementos agregados con X para eliminar
- Botón "💾 Programar Entrega"

#### 6. Métodos CRUD

Implementar siguiendo patrón de `RenderInformacionBancaria()`:
- `RegistrarTallas()` / `EditarTallas()` / `GuardarTallas()`
- `ProgramarNuevaEntrega()` / `EditarEntrega()` / `GuardarEntrega()`
- `AgregarElementoDetalle()` / `EliminarElementoDetalle()`
- `MarcarComoEntregada()` / `VincularActaEntrega()`
- `EliminarEntrega()` con modal de confirmación

#### 7. Integración con documentos

- Tipo documento nuevo: `TipoDocumentoEmpleado.ActaEntregaDotacion = 19`
- Al escanear acta desde una entrega → vincular automáticamente con `DocumentoActaId`
- Mostrar preview del acta en la card de entrega

---

## 📝 VALIDACIONES

1. **Tallas obligatorias** antes de primera entrega
2. **Fecha de entrega** no puede ser menor a fecha de ingreso
3. **Periodo único** por empleado (no duplicar "2024-1")
4. **Al menos 1 elemento** en detalle de entrega
5. **Estado "Entregada"** solo si tiene fecha_entrega_real
6. **Dotación legal** solo aplica si salario <= 2 SMMLV

---

## 🧪 CASOS DE PRUEBA

1. Empleado nuevo sin tallas → debe pedir registrar tallas primero
2. Registrar tallas completas → debe guardarse correctamente
3. Programar entrega con 5 elementos → debe crear entrega + detalles
4. Marcar entrega como "Entregada" → debe actualizar estado y fecha_real
5. Escanear acta de entrega → debe vincular con `DocumentoActaId`
6. Calcular próximas 3 entregas automáticamente cada 4 meses
7. Validar que no se puedan duplicar entregas del mismo periodo

---

## 📦 ORDEN DE IMPLEMENTACIÓN

### Fase 1: Backend (1-2 horas)
1. Crear enums en `/Domain/Enums/`
2. Crear entidades en `/Domain/Entities/`
3. Crear interfaces en `/Domain/Interfaces/`
4. Implementar repositorios en `/Infrastructure/Repositories/`
5. Registrar repositorios en `Program.cs`
6. Ejecutar migración SQL

### Fase 2: UI (2-3 horas)
7. Agregar tab en `EmpleadoExpediente.razor`
8. Crear `RenderDotacionEPP()` RenderFragment
9. Implementar formulario de tallas inline
10. Implementar lista de entregas con cards
11. Implementar formulario de nueva entrega
12. Agregar modales de confirmación

### Fase 3: Integración (30 min)
13. Vincular con módulo documentos (ActaEntregaDotacion)
14. Agregar tipo documento al enum existente
15. Probar escaneo y vinculación de actas

### Fase 4: Testing (30 min)
16. Compilar y verificar errores
17. Probar flujo completo: Tallas → Entrega → Acta
18. Validar cálculo automático de entregas

---

## 🎯 RESULTADO ESPERADO

Al finalizar:
- ✅ Tab "DOTACIÓN Y EPP" funcional en expediente
- ✅ CRUD completo de tallas con formulario inline
- ✅ CRUD completo de entregas con detalles
- ✅ Integración con documentos (actas escaneadas)
- ✅ Validaciones de cumplimiento legal
- ✅ Historial completo de entregas por empleado
- ✅ Próximas entregas visibles y programables

---

## 📌 NOTAS IMPORTANTES

- **Estilo UI:** Debe ser IDÉNTICO al tab de Información Bancaria (inline editing, no modales complejos)
- **Colores de estado:**
  - Programada: `#E8F0FF` (azul claro)
  - Entregada: `#E8FFE8` (verde claro)
  - Parcial: `#FFF9E6` (amarillo)
  - Cancelada: `#FFE8E8` (rojo claro)
- **Iconos:** Usar emojis simples: 👕 📦 ✅ ❌ 📋 📷
- **Periodicidad:** Dotación legal = cada 4 meses (3 entregas/año)
- **Cumplimiento:** Art. 230 CST aplica solo a empleados con salario <= 2 SMMLV

---

**PROMPT LISTO PARA EJECUTAR** ✅

Este prompt contiene toda la información necesaria para implementar el módulo completo de Dotación y EPP. Puede ser ejecutado directamente por el agente en una nueva sesión sin necesidad de investigación adicional.
