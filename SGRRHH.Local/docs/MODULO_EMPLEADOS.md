# 📋 Módulo de Empleados - Documentación Completa

> **Última actualización:** Enero 2026  
> **Proyecto:** SGRRHH.Local (Sistema de Gestión de Recursos Humanos)

---

## 🏗️ Arquitectura del Módulo

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          CAPA DE PRESENTACIÓN                            │
│   Blazor Server (.razor)                                                 │
├──────────────────────────────────────────────────────────────────────────┤
│  📄 Empleados.razor         → Lista principal con filtros y CRUD        │
│  📄 EmpleadoOnboarding.razor → Wizard 2 pasos para crear empleado       │
│  📄 EmpleadoExpediente.razor → Detalle con tabs (datos/docs/contratos)  │
├──────────────────────────────────────────────────────────────────────────┤
│  🔲 EmpleadoCard.razor      → Tarjeta de empleado                        │
│  🔲 EmpleadoSelector.razor  → Autocomplete para selección               │
│  🔲 EstadoBadge.razor       → Badge de estado con colores               │
│  🔲 InputCedula.razor       → Input con formato (1.192.208.848)         │
│  🔲 InputMoneda.razor       → Input moneda con $ y miles                │
└──────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                         CAPA DE INFRAESTRUCTURA                          │
│   Repositories + Services                                                │
├──────────────────────────────────────────────────────────────────────────┤
│  📦 EmpleadoRepository.cs   → CRUD con Dapper + concurrencia optimista  │
│  📦 EstadoEmpleadoService   → Máquina de estados + permisos por rol     │
│  📦 ICatalogCacheService    → Cache de catálogos (cargos, deptos, EPS)  │
└──────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌──────────────────────────────────────────────────────────────────────────┐
│                            CAPA DE DOMINIO                               │
│   Entities + Enums + DTOs                                                │
├──────────────────────────────────────────────────────────────────────────┤
│  👤 Empleado.cs             → Entidad principal (60+ propiedades)        │
│  🏷️ EstadoEmpleado.cs       → Enum (8 estados)                          │
│  📋 IEmpleadoRepository.cs  → Contrato del repositorio                  │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 👤 Entidad: Empleado.cs

### Ubicación
`SGRRHH.Local.Domain/Entities/Empleado.cs`

### Propiedades por Grupo

| Grupo | Propiedades |
|-------|-------------|
| **Identificación** | `Id`, `Codigo` (auto-generado), `Cedula` (único), `Nombres`, `Apellidos` |
| **Laboral** | `CargoId`, `DepartamentoId`, `SalarioBase`, `FechaIngreso`, `FechaRetiro` |
| **Estado** | `Estado` (enum), `MotivoRetiro` |
| **Seguridad Social** | `EPS`, `CodigoEPS`, `AFP`, `CodigoAFP`, `ARL`, `CodigoARL`, `CajaCompensacion`, `CodigoCaja` |
| **Contacto** | `Telefono`, `Celular`, `Email`, `Direccion`, `Ciudad`, `Departamento` (geo), `Barrio` |
| **Info Médica** | `TipoSangre`, `Alergias`, `CondicionesMedicas` |
| **Emergencia** | `ContactoEmergencia`, `TelefonoEmergencia`, `ParentescoEmergencia`, `ContactoEmergencia2`, `TelefonoEmergencia2`, `ParentescoEmergencia2` |
| **Auditoría** | `CreadoPorId`, `AprobadoPorId`, `FechaSolicitud`, `FechaAprobacion`, `MotivoRechazo`, `FechaCreacion`, `FechaModificacion` |
| **Foto** | `FotoUrl` |

### Propiedades de Navegación
- `Cargo` → Relación N:1
- `Departamento` → Relación N:1
- `CreadoPor` → Usuario que creó el registro
- `AprobadoPor` → Usuario que aprobó el registro

---

## 🔄 Máquina de Estados

### Enum: EstadoEmpleado

```csharp
public enum EstadoEmpleado
{
    PendienteAprobacion = 0,  // Estado inicial para Operadores
    Activo = 1,               // Empleado trabajando normalmente
    EnVacaciones = 2,         // En período de vacaciones
    EnLicencia = 3,           // En licencia (maternidad, luto, etc.)
    Suspendido = 4,           // Suspendido temporalmente
    Retirado = 5,             // Estado final - ya no trabaja
    Rechazado = 6,            // Solicitud rechazada
    EnIncapacidad = 7         // En incapacidad médica
}
```

### Diagrama de Transiciones

```
                    ┌─────────────────────┐
                    │ PendienteAprobacion │ ◄── Operador crea
                    └─────────┬───────────┘
                              │
              ┌───────────────┼───────────────┐
              ▼               ▼               ▼
        ┌─────────┐     ┌──────────┐    ┌──────────┐
        │ Activo  │     │Rechazado │    │ (fin)    │
        └────┬────┘     └──────────┘    └──────────┘
             │
    ┌────────┼────────┬────────────┬─────────────┐
    ▼        ▼        ▼            ▼             ▼
┌────────┐┌────────┐┌──────────┐┌───────────┐┌─────────┐
│EnVacac.││EnLicen.││Suspendido││EnIncapac. ││Retirado │
└────┬───┘└────┬───┘└────┬─────┘└─────┬─────┘└─────────┘
     │         │         │            │         (fin)
     └─────────┴─────────┴────────────┘
                    │
                    ▼
              ┌─────────┐
              │ Activo  │ ◄── Puede volver
              └─────────┘
```

### Permisos por Rol

| Transición | Operador | Aprobador | Admin |
|------------|:--------:|:---------:|:-----:|
| Pendiente → Activo | ❌ | ✅ | ✅ |
| Pendiente → Rechazado | ❌ | ✅ | ✅ |
| Activo ↔ EnVacaciones | ✅ | ✅ | ✅ |
| Activo ↔ EnLicencia | ✅ | ✅ | ✅ |
| Activo ↔ EnIncapacidad | ✅ | ✅ | ✅ |
| Activo → Suspendido | ❌ | ✅ | ✅ |
| Activo → Retirado | ❌ | ✅ | ✅ |

### Servicio: EstadoEmpleadoService

**Ubicación:** `SGRRHH.Local.Domain/Services/EstadoEmpleadoService.cs`

**Métodos principales:**

```csharp
// Determina estado inicial según rol del usuario
EstadoEmpleado ObtenerEstadoInicialSegunRol(RolUsuario rol)

// Valida si una transición de estado es válida
bool EsTransicionValida(EstadoEmpleado desde, EstadoEmpleado hacia)

// Verifica permisos del rol para la transición
bool TienePermisoParaTransicion(RolUsuario rol, EstadoEmpleado desde, EstadoEmpleado hacia)

// Obtiene transiciones permitidas para mostrar en UI
IEnumerable<EstadoEmpleado> ObtenerTransicionesPermitidas(EstadoEmpleado estadoActual, RolUsuario rol)

// Helpers para UI
string ObtenerDescripcion(EstadoEmpleado estado)
string ObtenerColorCss(EstadoEmpleado estado)

// Clasificación de estados
bool EsEstadoActivo(EstadoEmpleado estado)      // Activo, EnVacaciones, EnLicencia, EnIncapacidad
bool EsEstadoTemporal(EstadoEmpleado estado)    // EnVacaciones, EnLicencia, EnIncapacidad, Suspendido
bool EsEstadoFinal(EstadoEmpleado estado)       // Retirado, Rechazado
```

---

## 📄 Páginas Principales

### 1. Empleados.razor

**Ruta:** `/empleados` y `/empleados/{EmpleadoIdParam:int?}`

**Funcionalidades:**
- Lista con tabla paginada
- Filtros: por estado, búsqueda texto
- Acciones: Crear, Editar, Ver Expediente, Eliminar, Aprobar
- Polling cada 30 segundos para datos frescos
- Atajos de teclado: F2=Buscar, F3=Nuevo, F5=Actualizar

**Dependencias:**
- `IEmpleadoRepository`
- `IDocumentoEmpleadoRepository`
- `ICatalogCacheService`
- `ILocalStorageService`
- `IExportService`
- `IKeyboardShortcutService`

**Permisos (PermisosModulo):**
- `Crear` - Crear nuevos empleados
- `Editar` - Modificar empleados existentes
- `Eliminar` - Eliminar empleados
- `Aprobar` - Aprobar/rechazar empleados pendientes
- `EditarDatosCriticos` - Modificar salario, cédula
- `Retirar` - Retirar empleados
- `Exportar` - Exportar datos

---

### 2. EmpleadoOnboarding.razor

**Ruta:** `/empleados/onboarding`

**Funcionalidades:**
- Wizard de 2 pasos para crear empleado
- Validación de campos requeridos

**Step 1 - Datos Básicos:**
- Datos Personales (cédula, nombres, apellidos, fecha nacimiento, género)
- Datos Laborales (cargo, departamento, salario, fecha ingreso)
- Seguridad Social (EPS, AFP, ARL, Caja Compensación)
- Contacto (teléfono, celular, email, dirección)
- Info Médica (tipo sangre, alergias, condiciones)
- Contactos de Emergencia (2 contactos con parentesco)

**Step 2 - Revisar y Confirmar:**
- Resumen de todos los datos ingresados
- Botón confirmar para guardar

**Lógica de Estado Inicial:**
- Operadores → `PendienteAprobacion`
- Aprobadores/Admin → `Activo`

**Post-Guardado:**
- Redirige a `/documentos/{empleadoId}` para subir documentos

---

### 3. EmpleadoExpediente.razor

**Ruta:** `/empleados/{EmpleadoId:int}/expediente`

**Tabs:**

| Tab | Contenido |
|-----|-----------|
| **Datos Personales** | Info personal completa, edición inline |
| **Documentos** | Lista de documentos, preview, escaneo, descarga, impresión |
| **Contratos** | Historial de contratos laborales |
| **Seguridad Social** | EPS, AFP, ARL, Caja Compensación |

**Funcionalidades:**
- Cambio de estado desde dropdown (filtrado por permisos del rol)
- Integración con escáner (ScannerModal)
- Preview de documentos (DocumentPreviewModal)
- Impresión de documentos (PrinterModal)

**Dependencias:**
- `IEmpleadoRepository`
- `IContratoRepository`
- `IDocumentoEmpleadoRepository`
- `ICatalogCacheService`

---

## 🔲 Componentes Compartidos

### EmpleadoCard.razor
**Ubicación:** `Components/Shared/EmpleadoCard.razor`

Muestra tarjeta con:
- Foto del empleado
- Nombre completo
- Cargo
- Departamento
- Badge de estado

### EmpleadoSelector.razor
**Ubicación:** `Components/Shared/EmpleadoSelector.razor`

Autocomplete para selección de empleado:
- Búsqueda por nombre o cédula
- Muestra cargo y departamento
- Filtro por empleados activos

### EstadoBadge.razor
**Ubicación:** `Components/Shared/EstadoBadge.razor`

Badge con colores según estado:
- 🟢 Verde: Activo
- 🟡 Amarillo: PendienteAprobacion, EnVacaciones, EnLicencia
- 🔴 Rojo: Retirado, Rechazado, Suspendido
- 🔵 Azul: EnIncapacidad

### InputCedula.razor
**Ubicación:** `Components/Shared/InputCedula.razor`

Input con formato automático:
- Entrada: `1192208848`
- Salida: `1.192.208.848`

### InputMoneda.razor
**Ubicación:** `Components/Shared/InputMoneda.razor`

Input de moneda colombiana:
- Prefijo `$`
- Separador de miles
- Solo números

---

## 📦 Repositorio: EmpleadoRepository

**Ubicación:** `SGRRHH.Local.Infrastructure/Repositories/EmpleadoRepository.cs`

### Métodos CRUD

```csharp
Task<int> AddAsync(Empleado empleado)
Task<bool> UpdateAsync(Empleado empleado)  // Con concurrencia optimista
Task<bool> DeleteAsync(int id)
Task<Empleado?> GetByIdAsync(int id)
Task<IEnumerable<Empleado>> GetAllAsync()
```

### Métodos de Consulta

```csharp
Task<Empleado?> GetByIdWithRelationsAsync(int id)
Task<IEnumerable<Empleado>> GetAllWithRelationsAsync()
Task<Empleado?> GetByCodigoAsync(string codigo)
Task<Empleado?> GetByCedulaAsync(string cedula)
Task<IEnumerable<Empleado>> SearchAsync(string searchTerm)
```

### Métodos de Validación

```csharp
Task<bool> ExistsCodigoAsync(string codigo, int? excludeId = null)
Task<bool> ExistsCedulaAsync(string cedula, int? excludeId = null)
Task<bool> ExistsEmailAsync(string email, int? excludeId = null)
```

### Métodos Utilitarios

```csharp
Task<string> GetNextCodigoAsync()
Task<int> CountActiveAsync()
void InvalidateCache()
```

### Control de Concurrencia

El repositorio implementa **concurrencia optimista** usando el campo `fecha_modificacion`:

```csharp
public async Task<bool> UpdateAsync(Empleado empleado)
{
    // Verifica que fecha_modificacion coincida
    var sql = @"UPDATE empleados SET ... 
                WHERE id = @Id AND fecha_modificacion = @FechaModificacionOriginal";
    
    var rowsAffected = await connection.ExecuteAsync(sql, empleado);
    
    if (rowsAffected == 0)
        throw new ConcurrencyConflictException("El registro fue modificado por otro usuario");
    
    return true;
}
```

---

## 🔗 Entidades Relacionadas

| Entidad | Relación | Tabla | Descripción |
|---------|----------|-------|-------------|
| `Contrato` | 1:N | `contratos` | Contratos laborales del empleado |
| `DocumentoEmpleado` | 1:N | `documentos_empleado` | Documentos escaneados/subidos |
| `Incapacidad` | 1:N | `incapacidades` | Registro de incapacidades médicas |
| `Nomina` | 1:N | `nominas` | Registros de nómina mensual |
| `Permiso` | 1:N | `permisos` | Permisos solicitados |
| `Prestacion` | 1:N | `prestaciones` | Prestaciones sociales (cesantías, primas) |
| `ProyectoEmpleado` | N:M | `proyectos_empleados` | Asignación a proyectos forestales |
| `RegistroDiario` | 1:N | `registros_diarios` | Control de asistencia diaria |
| `Vacacion` | 1:N | `vacaciones` | Períodos de vacaciones |
| `Cargo` | N:1 | `cargos` | Cargo asignado |
| `Departamento` | N:1 | `departamentos` | Departamento asignado |

---

## 🚀 Flujo Completo de un Empleado

```
1. CREAR (/empleados/onboarding)
   └─► Operador ingresa datos → Estado: PendienteAprobacion
   └─► Aprobador/Admin ingresa datos → Estado: Activo

2. APROBAR (/empleados)
   └─► Aprobador ve listado filtrado por "Pendientes"
   └─► Revisa datos y cambia a Activo o Rechazado

3. DOCUMENTOS (/documentos/{empleadoId})
   └─► Escanear/subir documentos requeridos:
       • Cédula
       • Certificado EPS
       • Certificado AFP
       • Contrato firmado
       • Exámenes médicos

4. EXPEDIENTE (/empleados/{id}/expediente)
   └─► Ver/editar datos personales
   └─► Gestionar contratos
   └─► Ver/agregar documentos
   └─► Cambiar estado según permisos

5. CICLO DE VIDA
   └─► Activo ↔ EnVacaciones (programar vacaciones)
   └─► Activo ↔ EnLicencia (licencias especiales)
   └─► Activo ↔ EnIncapacidad (incapacidades médicas)
   └─► Activo → Suspendido (medida disciplinaria)
   └─► Activo → Retirado (terminación laboral - estado final)
```

---

## ✅ Estado Actual del Módulo

| Componente | Estado | Notas |
|------------|--------|-------|
| Compilación | ✅ | 0 errores |
| Entidad Empleado | ✅ | 60+ propiedades |
| EmpleadoRepository | ✅ | Con concurrencia optimista |
| EstadoEmpleadoService | ✅ | Máquina de estados completa |
| Empleados.razor | ✅ | Lista con filtros y paginación |
| EmpleadoOnboarding.razor | ✅ | Wizard de 2 pasos |
| EmpleadoExpediente.razor | ✅ | 4 tabs funcionales |
| Componentes Shared | ✅ | 5 componentes reutilizables |

---

## 📝 Notas Técnicas

### Base de Datos
- **Motor:** SQLite
- **ORM:** Dapper
- **Convención:** snake_case para nombres de tablas y columnas

### Validaciones
- Cédula única por empleado
- Email único (si se proporciona)
- Código auto-generado único
- Campos requeridos validados en frontend

### Seguridad
- Permisos por módulo (`PermisosModulo`)
- Transiciones de estado controladas por rol
- Auditoría de creación y modificación

### Performance
- Cache de catálogos (cargos, departamentos, EPS, etc.)
- Polling configurable para datos frescos
- Paginación en listados

---

*Documentación generada: Enero 2026*
