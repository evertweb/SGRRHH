# PROMPT: Implementar Hoja de Vida Inteligente (Smart CV)

> **Versión**: 1.1  
> **Fecha**: 2026-01-30  
> **Tipo**: Feature Completa (En Progreso)  
> **Prioridad**: Alta  
> **Progreso**: Fases 1-3 completadas ✅ | Continuar desde Fase 4  
> **Complejidad Restante**: Media (4-5 días de desarrollo)

---

## 🎯 OBJETIVO

Implementar un sistema de **PDFs interactivos (AcroForm)** para capturar información de aspirantes y actualizar datos de empleados existentes. El PDF funciona como formulario digital, llenarlo offline es posible, y al subirlo el sistema extrae automáticamente los datos.

---

## 📋 CONTEXTO DEL PROYECTO

### Stack Tecnológico
- **Backend**: .NET 8, Blazor Server
- **ORM**: Dapper (NO Entity Framework)
- **Base de Datos**: SQLite
- **Estilos**: CSS "hospitalario" (Courier New, terminal-like) - ver `hospital.css`
- **Idioma**: Todo en español (código, UI, comentarios)

### Arquitectura
```
SGRRHH.Local/
├── SGRRHH.Local.Domain/        # Entidades, Enums, DTOs, Interfaces
├── SGRRHH.Local.Infrastructure/ # Repositorios, Servicios, Data
├── SGRRHH.Local.Server/        # Blazor Server, Components, Pages
└── SGRRHH.Local.Shared/        # Código compartido, Helpers
```

### Librería PDF a Usar
- **iText 7** (NuGet: `itext7`)
- Licencia: AGPLv3 (uso interno, sin distribución)
- Funcionalidades: Generación PDF, AcroForm, metadatos XMP

---

## ✅ DECISIONES CONFIRMADAS

| Decisión | Valor | Notas |
|----------|-------|-------|
| Notificaciones email | ❌ NO | Sin integración SMTP |
| Firma digital obligatoria | ✅ SÍ | Campo `Sig` en PDF |
| Reactivar aspirantes descartados | ✅ SÍ | Estado `Reactivado` en flujo |
| SQLite FTS5 para búsqueda | ✅ SÍ | Full-text search en hojas de vida |
| OCR para PDFs externos | ❌ NO | No implementar |
| Cache de PDFs generados | ✅ SÍ | Cachear PDFs, regenerar si datos cambian |

---

## 🗄️ MODELO DE DATOS

### Nuevas Tablas

#### 1. Vacantes
```sql
CREATE TABLE IF NOT EXISTS vacantes (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    cargo_id INTEGER NOT NULL,
    departamento_id INTEGER NOT NULL,
    titulo TEXT NOT NULL,
    descripcion TEXT,
    requisitos TEXT,
    salario_minimo REAL,
    salario_maximo REAL,
    fecha_publicacion TEXT NOT NULL,
    fecha_cierre TEXT,
    estado TEXT NOT NULL DEFAULT 'Borrador',
    cantidad_posiciones INTEGER DEFAULT 1,
    es_activo INTEGER NOT NULL DEFAULT 1,
    fecha_creacion TEXT NOT NULL DEFAULT (datetime('now')),
    fecha_modificacion TEXT,
    FOREIGN KEY (cargo_id) REFERENCES cargos(id),
    FOREIGN KEY (departamento_id) REFERENCES departamentos(id)
);

CREATE INDEX idx_vacantes_estado ON vacantes(estado);
CREATE INDEX idx_vacantes_cargo ON vacantes(cargo_id);
```

#### 2. Aspirantes
```sql
CREATE TABLE IF NOT EXISTS aspirantes (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    vacante_id INTEGER,
    cedula TEXT NOT NULL UNIQUE,
    nombres TEXT NOT NULL,
    apellidos TEXT NOT NULL,
    fecha_nacimiento TEXT NOT NULL,
    genero TEXT NOT NULL,
    estado_civil TEXT NOT NULL,
    direccion TEXT NOT NULL,
    ciudad TEXT NOT NULL,
    departamento TEXT NOT NULL,
    telefono TEXT NOT NULL,
    email TEXT,
    nivel_educacion TEXT NOT NULL,
    titulo_obtenido TEXT,
    institucion_educativa TEXT,
    tallas_casco TEXT,
    tallas_botas TEXT,
    estado TEXT NOT NULL DEFAULT 'Registrado',
    fecha_registro TEXT NOT NULL DEFAULT (datetime('now')),
    fecha_modificacion TEXT,
    notas TEXT,
    puntaje_evaluacion INTEGER,
    es_activo INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (vacante_id) REFERENCES vacantes(id)
);

CREATE INDEX idx_aspirantes_cedula ON aspirantes(cedula);
CREATE INDEX idx_aspirantes_estado ON aspirantes(estado);
CREATE INDEX idx_aspirantes_vacante ON aspirantes(vacante_id);
```

#### 3. Formacion Aspirante
```sql
CREATE TABLE IF NOT EXISTS formacion_aspirante (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    aspirante_id INTEGER NOT NULL,
    nivel TEXT NOT NULL,
    titulo TEXT NOT NULL,
    institucion TEXT NOT NULL,
    fecha_inicio TEXT NOT NULL,
    fecha_fin TEXT,
    en_curso INTEGER DEFAULT 0,
    es_activo INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (aspirante_id) REFERENCES aspirantes(id) ON DELETE CASCADE
);
```

#### 4. Experiencia Aspirante
```sql
CREATE TABLE IF NOT EXISTS experiencia_aspirante (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    aspirante_id INTEGER NOT NULL,
    empresa TEXT NOT NULL,
    cargo TEXT NOT NULL,
    fecha_inicio TEXT NOT NULL,
    fecha_fin TEXT,
    trabajo_actual INTEGER DEFAULT 0,
    funciones TEXT,
    motivo_retiro TEXT,
    es_activo INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (aspirante_id) REFERENCES aspirantes(id) ON DELETE CASCADE
);
```

#### 5. Referencias Aspirante
```sql
CREATE TABLE IF NOT EXISTS referencias_aspirante (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    aspirante_id INTEGER NOT NULL,
    tipo TEXT NOT NULL,
    nombre_completo TEXT NOT NULL,
    telefono TEXT NOT NULL,
    relacion TEXT NOT NULL,
    empresa TEXT,
    cargo TEXT,
    es_activo INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (aspirante_id) REFERENCES aspirantes(id) ON DELETE CASCADE
);
```

#### 6. Hoja Vida PDF (Metadatos)
```sql
CREATE TABLE IF NOT EXISTS hoja_vida_pdf (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    aspirante_id INTEGER,
    empleado_id INTEGER,
    documento_empleado_id INTEGER,
    version INTEGER NOT NULL DEFAULT 1,
    hash_contenido TEXT NOT NULL,
    origen TEXT NOT NULL,
    fecha_generacion TEXT,
    fecha_subida TEXT NOT NULL DEFAULT (datetime('now')),
    datos_extraidos TEXT,
    tiene_firma INTEGER DEFAULT 0,
    es_valido INTEGER DEFAULT 1,
    errores_validacion TEXT,
    es_activo INTEGER NOT NULL DEFAULT 1,
    FOREIGN KEY (aspirante_id) REFERENCES aspirantes(id),
    FOREIGN KEY (empleado_id) REFERENCES empleados(id),
    FOREIGN KEY (documento_empleado_id) REFERENCES documentos_empleado(id)
);

CREATE INDEX idx_hoja_vida_aspirante ON hoja_vida_pdf(aspirante_id);
CREATE INDEX idx_hoja_vida_empleado ON hoja_vida_pdf(empleado_id);
```

#### 7. FTS5 para Búsqueda
```sql
CREATE VIRTUAL TABLE IF NOT EXISTS hojas_vida_fts USING fts5(
    aspirante_id,
    empleado_id,
    nombres,
    apellidos,
    cedula,
    formacion,
    experiencia,
    habilidades,
    content='hoja_vida_pdf',
    content_rowid='id'
);
```

---

## 📊 NUEVOS ENUMS

### EstadoAspirante
```csharp
public enum EstadoAspirante
{
    Registrado,
    EnRevision,
    Preseleccionado,
    Entrevistado,
    Contratado,
    Descartado,
    Reactivado  // Para aspirantes que vuelven a aplicar
}
```

### EstadoVacante
```csharp
public enum EstadoVacante
{
    Borrador,
    Abierta,
    EnProceso,
    Cerrada,
    Cancelada
}
```

### OrigenHojaVida
```csharp
public enum OrigenHojaVida
{
    Forestech,   // PDF generado por el sistema
    Externo,     // PDF subido sin metadatos Forestech
    Manual       // Datos ingresados manualmente
}
```

---

## 🏗️ ENTIDADES A CREAR

### Domain/Entities/

1. **Vacante.cs**
2. **Aspirante.cs**
3. **FormacionAspirante.cs**
4. **ExperienciaAspirante.cs**
5. **ReferenciaAspirante.cs**
6. **HojaVidaPdf.cs**

---

## 🔧 SERVICIOS A CREAR

### Infrastructure/Services/

#### 1. PdfHojaVidaService.cs
```csharp
public interface IPdfHojaVidaService
{
    // Generar PDF vacío para aspirante nuevo
    Task<byte[]> GenerarPdfVacioAsync();
    
    // Generar PDF prellenado para empleado existente
    Task<byte[]> GenerarPdfEmpleadoAsync(int empleadoId);
    
    // Validar y parsear PDF subido
    Task<ResultadoParseo> ProcesarPdfAsync(Stream pdfStream, string nombreArchivo);
    
    // Verificar si es formato Forestech
    Task<bool> EsFormatoForestechAsync(Stream pdfStream);
}
```

#### 2. ContratacionService.cs
```csharp
public interface IContratacionService
{
    // Migrar aspirante a empleado (transacción atómica)
    Task<Empleado> ContratarAspiranteAsync(int aspiranteId, DatosContratacion datos);
}
```

#### 3. XmpMetadataHandler.cs
```csharp
public interface IXmpMetadataHandler
{
    // Escribir metadatos al PDF
    void EscribirMetadatos(PdfDocument doc, Dictionary<string, string> datos);
    
    // Leer metadatos del PDF
    Dictionary<string, string> LeerMetadatos(PdfDocument doc);
}
```

---

## 📄 CAMPOS DEL PDF ACROFORM

### Sección: Datos Personales
| Campo PDF | Tipo | Mapeo Aspirante |
|-----------|------|-----------------|
| `Nombres` | Texto | `Nombres` |
| `Apellidos` | Texto | `Apellidos` |
| `Cedula` | Texto | `Cedula` |
| `FechaNacimiento` | Fecha | `FechaNacimiento` |
| `Genero` | Radio | `Genero` |
| `EstadoCivil` | Dropdown | `EstadoCivil` |
| `Direccion` | Texto | `Direccion` |
| `Ciudad` | Texto | `Ciudad` |
| `Departamento` | Dropdown | `Departamento` |
| `Telefono` | Texto | `Telefono` |
| `Email` | Texto | `Email` |

### Sección: Formación Académica (3 bloques repetidos)
| Campo PDF | Tipo |
|-----------|------|
| `Form[N]_Nivel` | Dropdown |
| `Form[N]_Titulo` | Texto |
| `Form[N]_Institucion` | Texto |
| `Form[N]_FechaInicio` | Fecha |
| `Form[N]_FechaFin` | Fecha |
| `Form[N]_EnCurso` | Checkbox |

### Sección: Experiencia Laboral (3 bloques repetidos)
| Campo PDF | Tipo |
|-----------|------|
| `Exp[N]_Empresa` | Texto |
| `Exp[N]_Cargo` | Texto |
| `Exp[N]_FechaInicio` | Fecha |
| `Exp[N]_FechaFin` | Fecha |
| `Exp[N]_TrabajoActual` | Checkbox |
| `Exp[N]_Funciones` | Texto multilínea |
| `Exp[N]_MotivoRetiro` | Texto |

### Sección: Referencias (2 personales + 2 laborales)
| Campo PDF | Tipo |
|-----------|------|
| `Ref[N]_Tipo` | Radio (Personal/Laboral) |
| `Ref[N]_Nombre` | Texto |
| `Ref[N]_Telefono` | Texto |
| `Ref[N]_Relacion` | Texto |
| `Ref[N]_Empresa` | Texto |
| `Ref[N]_Cargo` | Texto |

### Sección: Tallas EPP
| Campo PDF | Tipo |
|-----------|------|
| `TallaCasco` | Dropdown |
| `TallaBotas` | Texto (número) |

### Sección: Firma
| Campo PDF | Tipo |
|-----------|------|
| `Sig` | Firma digital (OBLIGATORIA) |
| `FechaFirma` | Fecha auto |

---

## 🔄 FLUJOS A IMPLEMENTAR

### Flujo 1: Aspirante Nuevo
```
1. HR accede a Vacantes → Selecciona vacante
2. Click "Agregar Aspirante"
3. Opción A: Ingresar datos manual
   Opción B: Descargar PDF vacío → Aspirante llena → HR sube PDF
4. Sistema valida y crea Aspirante
5. Aspirante pasa por estados: Registrado → EnRevision → Preseleccionado → Entrevistado
6. Si aprobado: Contratar → Migra a Empleado
```

### Flujo 2: Empleado Actualiza Datos
```
1. HR accede a Expediente → Empleado existente
2. Click "Descargar Hoja de Vida"
3. Sistema genera PDF prellenado con datos actuales
4. Empleado actualiza campos en PDF offline
5. HR sube PDF actualizado
6. Sistema parsea y actualiza Empleado (con confirmación)
```

### Flujo 3: Contratación (Migración Aspirante → Empleado)
```
1. Aspirante en estado "Entrevistado"
2. HR click "Contratar"
3. Modal solicita: Fecha ingreso, Salario, Cargo final
4. Sistema (transacción):
   a. Crea Empleado con datos de Aspirante
   b. Crea TallasEmpleado con tallas del Aspirante
   c. Crea Contrato básico
   d. Actualiza Aspirante.Estado = "Contratado"
   e. Vincula documentos del Aspirante al Empleado
5. Redirige a Expediente del nuevo Empleado
```

---

## 📦 COMPONENTES UI A CREAR

### Server/Components/Pages/

1. **Vacantes.razor** - CRUD de vacantes
2. **Aspirantes.razor** - CRUD de aspirantes con filtros por vacante/estado

### Server/Components/Shared/

1. **ModalContratacion.razor** - Modal para contratar aspirante
2. **PdfPreview.razor** - Componente para previsualizar PDF en modal
3. **SelectorVacante.razor** - Dropdown de vacantes activas

### Extensiones a Componentes Existentes

1. **Documentos.razor** - Agregar detección de PDF Forestech al subir
2. **DocumentosTab.razor** - Agregar botón "Generar HV" e indicador visual

---

## 🗺️ MAPEO DE CAMPOS: ASPIRANTE → EMPLEADO

| Campo Aspirante | Campo Empleado | Notas |
|-----------------|----------------|-------|
| `Cedula` | `Cedula` | Directo |
| `Nombres` | `Nombres` | Directo |
| `Apellidos` | `Apellidos` | Directo |
| `FechaNacimiento` | `FechaNacimiento` | Directo |
| `Genero` | `Genero` | Directo |
| `EstadoCivil` | `EstadoCivil` | Directo |
| `Direccion` | `Direccion` | Directo |
| `Telefono` | `Telefono` | Directo |
| `Email` | `Email` | Directo |
| `TallasCasco` | `TallasEmpleado.TallaCasco` | Crear registro |
| `TallasBotas` | `TallasEmpleado.TallaCalzadoNumero` | Crear registro |
| - | `Codigo` | Generar automático |
| - | `Estado` | Fijar en `Activo` |
| - | `FechaIngreso` | Parámetro de contratación |
| - | `SalarioBase` | Parámetro de contratación |
| - | `CargoId` | De la Vacante o parámetro |

---

## 📁 ARCHIVOS DE REFERENCIA

Para entender patrones existentes, revisar:

```
# Entidades
SGRRHH.Local.Domain/Entities/Empleado.cs
SGRRHH.Local.Domain/Entities/DocumentoEmpleado.cs
SGRRHH.Local.Domain/Entities/TallasEmpleado.cs

# Repositorios
SGRRHH.Local.Infrastructure/Repositories/EmpleadoRepository.cs
SGRRHH.Local.Infrastructure/Repositories/DocumentoEmpleadoRepository.cs

# Páginas
SGRRHH.Local.Server/Components/Pages/Empleados.razor
SGRRHH.Local.Server/Components/Pages/Documentos.razor
SGRRHH.Local.Server/Components/Pages/EmpleadoExpediente.razor.cs

# Estilos
SGRRHH.Local.Server/wwwroot/css/hospital.css
```

---

## 📋 ESTADO DE IMPLEMENTACIÓN

---

### ✅ FASE 1: Base de Datos y Entidades (COMPLETADA)

**Fecha de finalización:** 2026-01-30

#### Resumen
Se creó toda la infraestructura de base de datos y entidades del dominio para soportar el módulo de Hoja de Vida Inteligente.

#### Archivos Creados

| Tipo | Archivo | Descripción |
|------|---------|-------------|
| Migración SQL | `scripts/migration_hoja_vida_inteligente.sql` | Script con todas las tablas nuevas |
| Entidad | `Domain/Entities/Vacante.cs` | Entidad de vacante con propiedad `EsActivo` |
| Entidad | `Domain/Entities/Aspirante.cs` | Entidad de aspirante con propiedad `EsActivo` |
| Entidad | `Domain/Entities/FormacionAspirante.cs` | Formación académica del aspirante |
| Entidad | `Domain/Entities/ExperienciaAspirante.cs` | Experiencia laboral del aspirante |
| Entidad | `Domain/Entities/ReferenciaAspirante.cs` | Referencias personales/laborales |
| Entidad | `Domain/Entities/HojaVidaPdf.cs` | Metadatos de PDF con propiedad `EsActivo` |
| Enum | `Domain/Enums/EstadoAspirante.cs` | Estados del flujo de aspirantes |
| Enum | `Domain/Enums/EstadoVacante.cs` | Estados de las vacantes |
| Enum | `Domain/Enums/OrigenHojaVida.cs` | Origen del PDF (Forestech/Externo/Manual) |

#### Decisiones de Diseño
- Se agregó `EsActivo` a `Vacante`, `Aspirante` y `HojaVidaPdf` para soft deletes
- Las entidades relacionadas (Formación, Experiencia, Referencias) tienen `ON DELETE CASCADE`
- Índices creados en columnas frecuentemente consultadas

---

### ✅ FASE 2: Repositorios (COMPLETADA)

**Fecha de finalización:** 2026-01-30

#### Resumen
Se implementaron los repositorios con Dapper para todas las entidades nuevas, siguiendo los patrones existentes del proyecto.

#### Archivos Creados

| Tipo | Archivo | Descripción |
|------|---------|-------------|
| Interfaz | `Domain/Interfaces/IVacanteRepositorio.cs` | Contrato para vacantes |
| Interfaz | `Domain/Interfaces/IAspiranteRepositorio.cs` | Contrato para aspirantes (incluye entidades relacionadas) |
| Interfaz | `Domain/Interfaces/IHojaVidaPdfRepositorio.cs` | Contrato para metadatos PDF |
| Repositorio | `Infrastructure/Repositories/VacanteRepositorio.cs` | Implementación CRUD vacantes |
| Repositorio | `Infrastructure/Repositories/AspiranteRepositorio.cs` | Implementación con transacciones |
| Repositorio | `Infrastructure/Repositories/HojaVidaPdfRepositorio.cs` | Implementación metadatos PDF |

#### Decisiones de Diseño
- `AspiranteRepositorio` maneja Formación, Experiencia y Referencias internamente con transacciones
- Se usa `SqliteConnection` con `BeginTransaction()` síncrono (no `BeginTransactionAsync`)
- Query `ObtenerTodosAsync(bool incluirInactivos = false)` para soft deletes
- Registros en DI: `Program.cs` líneas 96-102

#### Nota Técnica
El manejo asíncrono de transacciones en SQLite requiere usar el cast `(SqliteConnection)` ya que `IDbConnection` no expone métodos async para transacciones.

---

### ✅ FASE 3: Módulo Vacantes UI (COMPLETADA)

**Fecha de finalización:** 2026-01-30

#### Resumen
Se creó la página de gestión de vacantes con funcionalidad CRUD completa siguiendo el estilo hospital.css.

#### Archivos Creados/Modificados

| Tipo | Archivo | Descripción |
|------|---------|-------------|
| Página | `Server/Components/Pages/Vacantes.razor` | CRUD completo (~520 líneas) |
| Navegación | `Server/Components/Layout/NavMenu.razor` | Agregado enlace en sección PERSONAL |

#### Funcionalidades Implementadas
- Tabla de vacantes con ordenamiento y paginación
- Filtros por estado y búsqueda de texto
- Modal para crear/editar vacantes
- Acciones: Publicar, Cerrar, Eliminar (soft delete)
- Validación de campos requeridos
- Indicadores visuales de estado (`estado-pendiente`, `estado-aprobada`, etc.)
- Atajos de teclado (F2=Nuevo, F9=Guardar, ESC=Cancelar)

#### Decisiones de Diseño
- Se usa el patrón toolbar + tabla + modal del proyecto
- Estados mapeados a clases CSS existentes
- Método `ObtenerTodosAsync` para listar (no `ObtenerTodasAsync`)

---

## 📋 FASES PENDIENTES

### ✅ FASE 4: Módulo Aspirantes UI (COMPLETADA)

**Fecha de finalización:** 2026-01-31

#### Resumen
Se creó la página de gestión de aspirantes con funcionalidad CRUD completa, tabs para datos relacionados (formación, experiencia, referencias) y flujo de estados.

#### Archivos Creados

| Tipo | Archivo | Descripción |
|------|---------|-------------|
| Página | `Server/Components/Pages/Aspirantes.razor` | CRUD completo (~850 líneas) |
| Componente | `Server/Components/Shared/ModalContratacion.razor` | Modal para contratar aspirante |
| Componente | `Server/Components/Shared/SelectorVacante.razor` | Dropdown de vacantes activas |

#### Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `Server/Components/Layout/NavMenu.razor` | Agregado enlace a Aspirantes en sección PERSONAL |

#### Funcionalidades Implementadas
- Tabla de aspirantes con filtros por vacante y estado
- Búsqueda de texto (nombre, cédula, teléfono, email)
- Modal CRUD con 4 tabs:
  - **Datos Personales**: información básica, contacto, educación, tallas
  - **Formación**: lista editable de estudios
  - **Experiencia**: lista editable de trabajos anteriores
  - **Referencias**: personales y laborales
- Flujo de estados con botones contextuales:
  - Registrado → En Revisión → Preseleccionado → Entrevistado → Contratado
  - Opción de Descartar/Reactivar desde cualquier estado
- Modal de contratación con campos: fecha ingreso, salario, cargo, departamento, tipo contrato
- Estilos: clases CSS de `hospital.css` (estado-pendiente, estado-aprobada, etc.)

#### Decisiones de Diseño
- El enum `NivelEducacion` usa `Secundaria` (no `Bachillerato`)
- La migración Aspirante→Empleado queda como `// TODO` para Fase 6 (ContratacionService)
- ModalContratacion incluye DTO interno `DatosContratacion` para parámetros
- SelectorVacante es reutilizable con parámetros `SoloAbiertas`, `Requerido`, `Deshabilitado`

---

### Fase 5: Servicio PDF (Siguiente)
- [ ] Instalar iText7: `dotnet add package itext7`
- [ ] `XmpMetadataHandler.cs` - Leer/escribir metadatos
- [ ] `PdfFieldMapper.cs` - Mapear campos AcroForm a entidades
- [ ] `PdfHojaVidaService.cs` - Generación y parseo de PDFs
- [ ] Diseñar template PDF base con campos AcroForm

### Fase 6: Servicio de Contratación (Día 3-4)
- [ ] `ContratacionService.cs` - Migrar aspirante a empleado
- [ ] `DatosContratacion.cs` - DTO para parámetros de contratación
- [ ] Transacción atómica: crear Empleado + Tallas + Contrato
- [ ] Tests unitarios de migración

### Fase 7: Integración y Polish (Día 4-5)
- [ ] Extender `Documentos.razor` para detectar PDF Forestech
- [ ] Extender `DocumentosTab.razor` con botón "Generar HV"
- [ ] Implementar cache de PDFs generados
- [ ] Configurar FTS5 para búsqueda full-text
- [ ] Testing E2E con Playwright

---

## ⚠️ REGLAS CRÍTICAS

1. **TODO en español** (variables, métodos, comentarios, UI)
2. **NO usar Entity Framework** - Solo Dapper
3. **NO crear bloques `<style>` inline** - Solo clases de `hospital.css`
4. **Compilar con**: `dotnet build -v:m /bl:build.binlog 2>&1 | Tee-Object build.log`
5. **Seguir patrones de repositorios existentes** (ver EmpleadoRepository.cs)

---

## 🔗 DOCUMENTOS RELACIONADOS

- [RFC Completo](file:///C:/Users/evert/.gemini/antigravity/brain/b4aa7069-6486-40c7-b353-017acd87832f/implementation_plan.md)
- [Análisis de Impacto](file:///C:/Users/evert/.gemini/antigravity/brain/b4aa7069-6486-40c7-b353-017acd87832f/analisis_impacto.md)

---

*Documento actualizado: 2026-01-30 (Fases 1-3 completadas)*
