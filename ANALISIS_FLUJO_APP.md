# 📊 ANÁLISIS COMPLETO DE FLUJO LÓGICO - SGRRHH LOCAL

**Fecha de Análisis:** 8 de Enero de 2026  
**Versión de la App:** 1.0.0  
**Tecnología:** Blazor Server (.NET 8) + SQLite

---

## 🎯 RESUMEN EJECUTIVO

**SGRRHH** (Sistema de Gestión de Recursos Humanos) es una aplicación de escritorio web para gestión de RRHH en empresas pequeñas (~20 empleados). La arquitectura sigue Clean Architecture con separación clara de capas.

---

## 🔄 FLUJO LÓGICO PRINCIPAL

### PUNTO 0: Inicio de la Aplicación

```
┌─────────────────────────────────────────────────────────────────┐
│                        STARTUP                                   │
│  Program.cs → Configuración de servicios → Migraciones DB       │
└─────────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                      App.razor                                   │
│  Carga CSS + JS → Routes.razor → Router                         │
└─────────────────────────────────────────────────────────────────┘
                               │
                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Routes.razor                                  │
│  DefaultLayout = MainLayout                                      │
└─────────────────────────────────────────────────────────────────┘
                               │
          ┌────────────────────┴────────────────────┐
          ▼                                         ▼
┌─────────────────────┐               ┌─────────────────────────┐
│  ¿Autenticado?      │               │  EmptyLayout            │
│  (MainLayout.razor) │               │  (Solo para Login)      │
└─────────────────────┘               └─────────────────────────┘
          │ NO                                      │
          ▼                                         │
┌─────────────────────┐                             │
│  RedirectToLogin    │ ◄───────────────────────────┘
│  → /login           │
└─────────────────────┘
          │
          ▼
┌─────────────────────────────────────────────────────────────────┐
│                       LOGIN.RAZOR                                │
│  @page "/login"  @page "/"                                       │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │ FORMULARIO:                                                  ││
│  │ • Campo Usuario (autofocus)                                  ││
│  │ • Campo Contraseña                                           ││
│  │ • Botón INGRESAR                                             ││
│  │ • Soporte Enter para login                                   ││
│  └─────────────────────────────────────────────────────────────┘│
│                              │                                   │
│                              ▼                                   │
│  AuthService.LoginAsync(username, password)                      │
│  → Valida credenciales contra DB SQLite                         │
│  → Hashea password con BCrypt                                   │
│  → Crea sesión en memoria                                       │
└─────────────────────────────────────────────────────────────────┘
          │ Login Exitoso
          ▼
┌─────────────────────────────────────────────────────────────────┐
│                   REDIRECCIÓN POR ROL                            │
│  • Administrador → /usuarios (Gestión de Usuarios)              │
│  • Aprobador (Ingeniera) → /empleados (Empleados)               │
│  • Operador (Secretaria) → /control-diario (Control Diario)     │
│                                                                  │
│  Los usuarios acceden directamente a su pantalla principal       │
│  según su rol sin pasar por un dashboard.                        │
└─────────────────────────────────────────────────────────────────┘
└─────────────────────────────────────────────────────────────────┘
```

---

## 🗺️ MAPA DE NAVEGACIÓN COMPLETO

```
                              ┌─────────────────┐
                              │     LOGIN       │
                              │   @page "/"     │
                              └────────┬────────┘
                                       │
                                       ▼
┌──────────────────────────────────────────────────────────────────────────────┐
│                            MAIN LAYOUT                                        │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │ HEADER: SGRRHH LOCAL v1.0 | Usuario: [Nombre] ([Rol]) | Fecha/Hora     │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │ NAV MENU (Horizontal):                                                   │ │
│  │ INICIO | EMPLEADOS | DOCUMENTOS | PERMISOS | VACACIONES | CONTRATOS    │ │
│  │ | CONTROL DIARIO | [ADMIN: CATÁLOGOS | USUARIOS | REPORTES | CONFIG]   │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │ BREADCRUMB: Ruta: INICIO > [SECCIÓN ACTUAL]                             │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────────────────────────┐ │
│  │                          WORK AREA (@Body)                               │ │
│  │                                                                          │ │
│  └─────────────────────────────────────────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

## 📋 INVENTARIO DE PANTALLAS Y COMPONENTES

### 1. 👥 EMPLEADOS (`/empleados`, `/empleados/{id}`)

| Elemento | Tipo | Descripción |
|----------|------|-------------|
| Barra Superior | Toolbar | WIZARD ONBOARDING, NUEVO RÁPIDO, ACTUALIZAR, EDITAR, EXPORTAR EXCEL |
| Campo Búsqueda | Input | Búsqueda por código, cédula, nombre |
| Filtro Estado | Select | Activos, Retirados, Pendientes, En Vacaciones, Suspendidos |
| Tabla Empleados | DataTable | 8 columnas: Foto, Código, Cédula, Nombre, Cargo, Departamento, Estado, Acciones |
| `FormModal` | Modal | Formulario de creación/edición con 2 columnas (Modo Rápido) |
| `Wizard Onboarding` | Página | Proceso guiado de ingreso de empleado en 4 pasos |

**Campos del Formulario (Modo Rápido):**
- **Columna Izquierda (Datos Básicos):** Código*, Cédula*, Nombres*, Apellidos*, Fecha Nacimiento, Género, Estado Civil
- **Columna Derecha (Contacto/Laboral):** Teléfono, Email, Dirección, Fecha Ingreso*, Departamento, Cargo, Estado, Foto
- **Sección Adicional:** Contacto Emergencia (Nombre, Teléfono)
- **Observaciones:** Textarea

**Atajos de Teclado:**
| Tecla | Acción |
|-------|--------|
| F2 | Buscar (focus) |
| F3 | Nuevo Empleado Rápido |
| F4 | Editar Seleccionado |
| F5 | Actualizar Lista |
| F10 | Exportar Excel |
| ESC | Cerrar Modal |

---

### 1b. 🧑‍💼 WIZARD ONBOARDING (`/empleados/onboarding`)

**Proceso Guiado de Ingreso de Empleado en 4 Pasos:**

#### PASO 1: Datos Básicos del Empleado
| Elemento | Descripción |
|----------|-------------|
| Formulario Completo | Mismos campos que modo rápido, organizado en 2 columnas |
| Validación en Tiempo Real | Indica campos obligatorios faltantes |
| Vista Previa de Foto | Muestra preview inmediato de foto seleccionada |

#### PASO 2: Documentos Obligatorios (17)
| Documento | Legislación Colombiana |
|-----------|------------------------|
| 📄 Fotocopia de Cédula | Art. 23 CST |
| 📄 Hoja de Vida / Curriculum | Requerido |
| 📄 Certificados de Estudios | Validación de formación |
| 📄 Certificados Laborales | Experiencia previa |
| 📄 Referencias Personales | Mínimo 2 |
| 📄 Referencias Laborales | Mínimo 2 |
| 📄 Examen Médico de Ingreso | Resolución 2346/2007 |
| 📄 Afiliación EPS | Ley 100 de 1993 |
| 📄 Afiliación AFP (Pensión) | Ley 100 de 1993 |
| 📄 Afiliación ARL | Decreto 1295 de 1994 |
| 📄 Afiliación Caja Compensación | Ley 21 de 1982 |
| 📄 Certificado de Antecedentes | Procuraduría, Contraloría, Policía |
| 📄 RUT (Registro Único Tributario) | DIAN |
| 📄 Certificación Bancaria | Cuenta para salario |
| 📄 Contrato de Trabajo Firmado | Art. 39 CST |
| 📄 Libreta Militar | Solo hombres hasta 50 años |
| 📄 Foto 3x4 tipo documento | Identificación |

**Características:**
- Progreso visual: muestra documentos seleccionados
- Campos por documento: Archivo, Fecha Emisión, Fecha Vencimiento
- No bloquea si faltan documentos (se pueden subir después)

#### PASO 3: Documentos Opcionales (5)
| Documento | Uso |
|-----------|-----|
| 📎 Licencia de Conducción | Si el cargo requiere conducir |
| 📎 Acta Entrega de Dotación | Uniformes, EPP |
| 📎 Certificados de Capacitación | Cursos, diplomados |
| 📎 Exámenes Médicos Periódicos | Seguimiento |
| 📎 Otros Documentos | Adicionales |

#### PASO 4: Revisar y Confirmar
| Elemento | Descripción |
|----------|-------------|
| Resumen de Datos | Muestra todos los datos del empleado |
| Estadísticas de Documentos | Obligatorios, Opcionales, Total a subir |
| Lista de Archivos | Previsualización de documentos seleccionados |
| Confirmación Final | Botón FINALIZAR Y GUARDAR |

**Proceso al Finalizar:**
1. Valida datos básicos obligatorios
2. Crea registro del empleado en BD
3. Guarda foto (si hay)
4. Sube documentos obligatorios seleccionados
5. Sube documentos opcionales seleccionados
6. Muestra resumen de éxito
7. Redirige a lista de empleados

**Navegación:**
- Botones: CANCELAR, ◀ ANTERIOR, SIGUIENTE ▶, ✓ FINALIZAR Y GUARDAR
- Barra de progreso visual con 4 pasos
- Validación antes de avanzar (solo en paso 1)

---

### 3. 📄 DOCUMENTOS (`/documentos`, `/documentos/{empleadoId}`)

| Elemento | Tipo | Descripción |
|----------|------|-------------|
| Selector Empleado | Select | Dropdown con todos los empleados |
| Tabla Documentos | DataTable | Tipo, Nombre, Emisión, Vencimiento, Estado, Acciones |
| Modal Subir Documento | Modal | Tipo, Nombre, Descripción, Fechas, Archivo |
| Modal Confirmar Eliminación | Modal | Confirmación para eliminar documento |

**Tipos de Documento:**
- Cédula, Hoja de Vida, Certificado Estudios, Certificado Laboral
- Examen Médico (Ingreso/Periódico/Egreso)
- Afiliaciones (EPS, AFP, ARL, Caja Compensación)
- Referencias (Personales/Laborales), Antecedentes
- Licencia Conducción, Libreta Militar, RUT
- Certificado Bancario, Acta Entrega Dotación
- Capacitación, Contrato Firmado, Foto, Otro

---

### 4. 🗓️ PERMISOS (`/permisos`, `/permisos/{id}`)

| Elemento | Tipo | Descripción |
|----------|------|-------------|
| Barra Superior | Toolbar | NUEVO, ACTUALIZAR, VER DETALLE, APROBAR*, RECHAZAR*, GENERAR PDF* |
| Campo Búsqueda | Input | Búsqueda por acta, empleado, tipo |
| Filtro Estado | Select | Pendientes, Aprobados, Rechazados, Cancelados |
| Filtro Empleado | Select | Todos los empleados |
| Tabla Permisos | DataTable | N° Acta, Empleado, Tipo, Fechas, Días, Estado, Acciones |
| `FormModal` | Modal | Nueva/Detalle solicitud |
| Dialog Rechazo | Modal | Motivo del rechazo |

**Campos del Formulario:**
- Empleado*, Tipo Permiso*, Fecha Inicio*, Fecha Fin*, Total Días (calculado)
- Motivo*, Observaciones, Documento Soporte (opcional)
- Estado, Fecha Solicitud, Solicitado Por, Aprobado Por, Fecha Aprobación

**Estados del Permiso:**
- `Pendiente` → `Aprobado` | `Rechazado`
- `Aprobado` → Se puede generar PDF
- `Cancelado` → Estado final

---

### 5. 🏖️ VACACIONES (`/vacaciones`, `/vacaciones/{id}`)

| Elemento | Tipo | Descripción |
|----------|------|-------------|
| `ResumenVacacionesPanel` | Panel | Días disponibles/usados por empleado |
| Barra Superior | Toolbar | NUEVA, ACTUALIZAR, VER DETALLE, APROBAR*, RECHAZAR* |
| Filtros | Inputs/Selects | Búsqueda, Estado, Empleado, Período |
| Tabla Vacaciones | DataTable | Empleado, Período, Fechas, Días, Estado, Acciones |
| `FormModal` | Modal | Nueva/Editar vacación |
| Dialog Rechazo | Modal | Motivo del rechazo |

**Campos del Formulario:**
- Empleado*, Período Correspondiente*, Fecha Inicio*, Fecha Fin*, Días a Tomar
- Observaciones, Estado, Historial de Vacaciones del Empleado

**Cálculo de Días:**
- Base: 15 días/año
- Adicional: +1 día cada 5 años (máximo 5 días adicionales)
- Excluye fines de semana

---

### 6. 📋 CONTRATOS (`/contratos`, `/contratos/{empleadoId}`)

| Elemento | Tipo | Descripción |
|----------|------|-------------|
| Barra Superior | Toolbar | NUEVO, ACTUALIZAR, EDITAR |
| Filtros | Inputs/Selects | Búsqueda, Estado, Tipo Contrato |
| Tabla Contratos | DataTable | Código Emp, Empleado, Tipo, Cargo, Fechas, Salario, Estado, Acciones |
| Panel Alertas | Panel | Contratos por vencer (30 días) |
| `FormModal` Contrato | Modal | Editar/Crear contrato |
| `FormModal` Historial | Modal | Ver historial de contratos del empleado |

**Tipos de Contrato:**
- Indefinido, Fijo, Obra o Labor, Prestación de Servicios, Aprendizaje

**Estados del Contrato:**
- Activo, Finalizado, Renovado, Cancelado

---

### 7. ⏱️ CONTROL DIARIO (`/control-diario`, `/control-diario/{fecha}`)

| Elemento | Tipo | Descripción |
|----------|------|-------------|
| Navegador de Fecha | Toolbar | ◀ ANTERIOR, Fecha, SIGUIENTE ▶, HOY |
| Estadísticas del Día | Cards | Registros, Completados, Total Horas, Total Actividades |
| Filtros | Inputs/Selects | Búsqueda, Estado, Departamento, Solo Activos |
| Tabla Registros | DataTable | Check, Foto, Código, Empleado, Depto, Entrada, Salida, Horas, Actividades, Estado, Acciones |
| Panel Detalle | Panel | Detalles del empleado + tabla de actividades |
| Panel Sin Registro | Panel | Empleados activos sin registro para la fecha |
| `FormModal` Actividad | Modal | Agregar/Editar actividad |

**Estados del Registro:**
- Borrador → Completado → Aprobado

**Campos Actividad:**
- Actividad*, Proyecto (si requiere), Horas*, Orden
- Hora Inicio, Hora Fin, Descripción

---

### 8. 📚 CATÁLOGOS (`/catalogos`) - Solo Admin

| Tab | Descripción |
|-----|-------------|
| DEPARTAMENTOS | CRUD de departamentos de la empresa |
| CARGOS | CRUD de cargos/posiciones |
| TIPOS DE PERMISO | CRUD de tipos de permiso (con días por defecto y color) |
| PROYECTOS | CRUD de proyectos (para control diario) |
| ACTIVIDADES | CRUD de actividades (para control diario) |

Cada tab tiene: Tabla + FormModal para CRUD

---

### 9. 👤 USUARIOS (`/usuarios`, `/usuarios/{id}`) - Solo Admin

| Elemento | Tipo | Descripción |
|----------|------|-------------|
| Barra Superior | Toolbar | NUEVO, ACTUALIZAR, EDITAR, RESET PASSWORD, HABILITAR/DESHABILITAR |
| Filtros | Inputs/Selects | Búsqueda, Rol, Estado |
| Tabla Usuarios | DataTable | Username, Nombre, Email, Teléfono, Rol, Último Acceso, Estado, Acciones |
| `FormModal` Usuario | Modal | Crear/Editar usuario |
| `FormModal` Reset | Modal | Reset de contraseña |
| `FormModal` Confirmar | Modal | Confirmar habilitar/deshabilitar |

**Roles:**
| Rol | Permisos |
|-----|----------|
| Administrador | Acceso total, gestión de usuarios, configuración |
| Aprobador | Aprobar/rechazar permisos y vacaciones |
| Operador | Operaciones del día a día |

---

### 10. ⚙️ CONFIGURACIÓN (`/configuracion`) - Solo Admin

| Tab | Descripción |
|-----|-------------|
| DATOS DE LA EMPRESA | Nombre, RUC, Dirección, Teléfono, Email, Web, Logo |
| CONFIGURACIÓN DEL SISTEMA | Tabla de configuraciones clave-valor por categoría |
| BACKUP/RESTORE | Crear backup, Restaurar backup, Info del sistema |

**Categorías de Configuración:**
- General, Permisos, Vacaciones, Contratos, Reportes, Email, Sistema

---

### 11. 📊 REPORTES (`/reportes`) - Solo Admin

*(Página pendiente de revisar contenido específico)*

### 12. 🔍 AUDITORÍA (`/auditoria`) - Solo Admin/Aprobador

*(Página para ver logs de auditoría del sistema)*

---

## 🧩 COMPONENTES COMPARTIDOS

### FormModal
```
Propiedades:
- IsVisible, Title, Width
- OnSaveClicked, OnCancelClicked
- ShowSaveButton, ShowCancelButton
- IsSaving, CloseOnBackdropClick

Características:
- Overlay con clic para cerrar (opcional)
- Barra de atajos: F9 Guardar, ESC Cancelar
- Botones CANCELAR y GUARDAR
```

### KeyboardHandler
```
Propiedades:
- ShowShortcutBar, IsEnabled
- Shortcuts (lista de atajos)
- OnKeyPressedCallback

Atajos Predefinidos:
- F1: Ayuda
- F2: Buscar
- F3: Nuevo
- F4: Editar
- F5: Actualizar
- F8: Eliminar
- F9: Guardar (en formularios)
- ESC: Cancelar/Cerrar
```

### DataTable
*(Componente genérico para tablas con selección)*

### EmpleadoCard
*(Tarjeta de empleado para visualización rápida)*

### EmpleadoSelector
*(Selector de empleado con búsqueda)*

### EstadoBadge
*(Badge de estado con colores)*

### ConfirmDialog
*(Diálogo de confirmación genérico)*

### CalendarioMini
*(Calendario pequeño para selección de fechas)*

### ResumenVacacionesPanel
*(Panel de resumen de vacaciones del empleado)*

---

## 🔐 FLUJO DE AUTENTICACIÓN

```
┌─────────────────────────────────────────────────────────────────┐
│                    IAuthService                                  │
├─────────────────────────────────────────────────────────────────┤
│ Propiedades:                                                     │
│ • IsAuthenticated: bool                                         │
│ • CurrentUser: Usuario?                                         │
│ • CurrentUserId: int                                            │
│ • IsAdmin: bool                                                 │
│ • IsAprobador: bool                                             │
│ • IsSupervisor: bool                                            │
├─────────────────────────────────────────────────────────────────┤
│ Métodos:                                                         │
│ • LoginAsync(username, password) → ServiceResult                │
│ • LogoutAsync() → Task                                          │
│ • ResetPasswordAsync(userId, newPassword) → ServiceResult       │
│ • HashPassword(password) → string                               │
├─────────────────────────────────────────────────────────────────┤
│ Eventos:                                                         │
│ • OnAuthStateChanged: event                                     │
└─────────────────────────────────────────────────────────────────┘
```

---

## 📊 FLUJO DE DATOS POR MÓDULO

### Empleados
```
Empleados.razor
    │
    ├── IEmpleadoRepository
    │   ├── GetAllAsync()
    │   ├── GetAllWithRelationsAsync()
    │   ├── GetByIdWithRelationsAsync(id)
    │   ├── GetNextCodigoAsync()
    │   ├── ExistsCodigoAsync(codigo, excludeId?)
    │   ├── ExistsCedulaAsync(cedula, excludeId?)
    │   ├── ExistsEmailAsync(email, excludeId?)
    │   ├── AddAsync(empleado)
    │   └── UpdateAsync(empleado)
    │
    ├── ICargoRepository.GetAllAsync()
    ├── IDepartamentoRepository.GetAllAsync()
    ├── ILocalStorageService.SaveEmpleadoFotoAsync()
    └── IExportService.ExportEmpleadosToExcelAsync()
```

### Permisos
```
Permisos.razor
    │
    ├── IPermisoRepository
    │   ├── GetAllAsync()
    │   ├── GetByIdAsync(id)
    │   ├── GetByEstadoAsync(estado)
    │   ├── GetProximoNumeroActaAsync()
    │   ├── ExisteSolapamientoAsync(empleadoId, fechaInicio, fechaFin, excludeId?)
    │   ├── AddAsync(permiso)
    │   └── UpdateAsync(permiso)
    │
    ├── IEmpleadoRepository.GetAllAsync()
    ├── ITipoPermisoRepository.GetAllAsync()
    └── ILocalStorageService.SavePermisoDocumentoAsync()
```

### Vacaciones
```
Vacaciones.razor
    │
    ├── IVacacionRepository
    │   ├── GetAllAsync()
    │   ├── GetByIdAsync(id)
    │   ├── GetByEstadoAsync(estado)
    │   ├── GetByEmpleadoIdAsync(empleadoId)
    │   ├── GetByEmpleadoYPeriodoAsync(empleadoId, periodo)
    │   ├── GetResumenVacacionesAsync(empleadoId)
    │   ├── ExisteTraslapeAsync(empleadoId, fechaInicio, fechaFin, excludeId?)
    │   ├── AddAsync(vacacion)
    │   └── UpdateAsync(vacacion)
    │
    └── IEmpleadoRepository.GetAllAsync()
```

### Control Diario
```
ControlDiario.razor
    │
    ├── IRegistroDiarioRepository
    │   ├── GetByFechaAsync(fecha)
    │   ├── GetByIdWithDetallesAsync(id)
    │   ├── AddAsync(registro)
    │   ├── UpdateAsync(registro)
    │   ├── AddDetalleAsync(registroId, detalle)
    │   ├── UpdateDetalleAsync(detalle)
    │   └── DeleteDetalleAsync(registroId, detalleId)
    │
    ├── IEmpleadoRepository.GetAllActiveAsync()
    ├── IDepartamentoRepository.GetAllActiveAsync()
    ├── IActividadRepository.GetAllActiveAsync()
    ├── IProyectoRepository.GetByEstadoAsync(Activo)
    └── IDetalleActividadRepository.GetByRegistroAsync(registroId)
```

---

## 🎨 PATRONES DE UI CONSISTENTES

### Estructura de Página
1. `PageTitle` - Título del navegador
2. `KeyboardHandler` - Manejo de atajos
3. `h1.page-title` - Título de la página
4. Mensajes de Error/Éxito - Bloques condicionales
5. Toolbar - Botones de acción + Filtros
6. Info de resultados - "Mostrando X de Y"
7. Tabla principal - DataTable con selección
8. FormModal(s) - Para CRUD

### Convenciones de Botones
- **F2**: Buscar/Focus en búsqueda
- **F3**: Nuevo
- **F4**: Editar seleccionado
- **F5**: Actualizar/Refrescar
- **F6**: Aprobar (donde aplique)
- **F7**: Rechazar (donde aplique)
- **F8**: Eliminar
- **F9**: Guardar (en modales)
- **F10**: Exportar
- **F12**: Generar PDF
- **ESC**: Cerrar/Cancelar

### Estados y Colores
```css
.badge-activo     { background: #00AA00; }  /* Verde */
.badge-inactivo   { background: #CC0000; }  /* Rojo */
.badge-pendiente  { background: #FF9800; }  /* Naranja */
.badge-aprobado   { background: #4CAF50; }  /* Verde claro */
.badge-rechazado  { background: #F44336; }  /* Rojo */
.badge-completado { background: #2196F3; }  /* Azul */
```

---

## 🔧 MEJORAS IMPLEMENTADAS (Enero 2026)

Las siguientes mejoras fueron implementadas para mejorar la experiencia de usuario y el rendimiento:

### ✅ 1. Paginación en Tablas
- **Ubicación:** `DataTable.razor`, `Empleados.razor`
- **Descripción:** Control completo de paginación con:
  - Navegación: ⏮ ← Página X de Y → ⏭
  - Selector de tamaño de página (10, 20, 50, 100)
  - Indicador "Mostrando X-Y de Z registros"
- **Uso:** `<DataTable PageSize="20" ShowPagination="true" ...>`

### ✅ 2. Caché de Catálogos
- **Servicio:** `ICatalogCacheService` / `CatalogCacheService`
- **Ubicación:** `SGRRHH.Local.Infrastructure/Services/`
- **Descripción:** Caché en memoria con expiración deslizante de 10 minutos para:
  - Cargos, Departamentos, Tipos de Permiso
  - Proyectos, Actividades, Empleados Activos
- **Uso:** 
  ```csharp
  @inject ICatalogCacheService CatalogCache
  var cargos = await CatalogCache.GetCargosAsync();
  ```

### ✅ 3. Confirmación de Cambios Sin Guardar
- **Componente:** `UnsavedChangesGuard.razor`
- **Ubicación:** `Components/Shared/`
- **Descripción:** 
  - Previene navegación accidental cuando hay cambios sin guardar
  - Muestra alerta del navegador (beforeunload)
  - Método `ConfirmNavigationAsync()` para confirmación programática
- **Uso:**
  ```razor
  <UnsavedChangesGuard @ref="unsavedChangesGuard" HasChanges="hasUnsavedChanges" />
  ```

### ✅ 4. Componente de Mensajes Reutilizable (Toast)
- **Componente:** `MessageToast.razor`
- **Ubicación:** `Components/Shared/`
- **Descripción:** Sistema de notificaciones tipo toast con:
  - Tipos: Success, Error, Warning, Info
  - Auto-dismiss configurable (default 5 segundos)
  - Posicionamiento fijo en esquina superior derecha
- **Uso:**
  ```razor
  <MessageToast @ref="messageToast" />
  @code {
      messageToast?.ShowSuccess("Guardado exitosamente");
      messageToast?.ShowError("Error al procesar");
  }
  ```

### ✅ 5. Navegación con Teclado en Tablas
- **Ubicación:** `DataTable.razor`, `wwwroot/js/app.js`
- **Descripción:** Navegación completa con teclado:
  - ↑↓: Mover entre filas
  - Enter: Seleccionar fila actual
  - Home: Ir a primera fila
  - End: Ir a última fila
- **Activación:** `ShowKeyboardHints="true"` en DataTable

### ✅ 6. Generación de PDF para Permisos
- **Servicio:** `IReportService.GenerarActaPermisoAsync()`
- **Ubicación:** `SGRRHH.Local.Infrastructure/Services/ReportService.cs`
- **Descripción:** Genera acta de permiso en PDF con QuestPDF incluyendo:
  - Datos del empleado y empresa
  - Detalles del permiso (fechas, tipo, motivo)
  - Firmas y fecha de generación
- **Uso:** Botón "GENERAR PDF" en página de Permisos (visible solo para aprobados)

### ✅ 7. Notificaciones en Tiempo Real
- **Servicio:** `INotificationService` / `NotificationService`
- **Componente:** `NotificationBell.razor`
- **Ubicación:** `Components/Shared/`, `Infrastructure/Services/`
- **Descripción:**
  - Icono de campana en header con contador de pendientes
  - Dropdown con lista de permisos/vacaciones pendientes
  - Notificaciones del navegador (con permiso del usuario)
  - Polling cada 2 minutos en MainLayout
- **Uso:** Integrado automáticamente en MainLayout

### ✅ 8. Timeout de Sesión
- **Servicio:** `ISessionService` / `SessionService`
- **Ubicación:** `Infrastructure/Services/`
- **Descripción:**
  - Timeout configurable (default 30 minutos)
  - Barra de advertencia 5 minutos antes de expirar
  - Eventos `OnSessionExpiring` y `OnSessionExpired`
  - Extensión automática con actividad del usuario
- **Configuración:** Variable `SessionTimeoutMinutes` en `SessionService`

### ✅ 9. Persistencia de Sesión/Caché
- **Servicio:** Integrado en `IMemoryCache`
- **Ubicación:** Registrado en `Program.cs`
- **Descripción:**
  - Los catálogos se mantienen en memoria durante la sesión
  - Funciones JS para localStorage: `saveToLocalStorage`, `getFromLocalStorage`
  - Reducción de llamadas a base de datos

### ✅ 10. Wizard de Onboarding de Empleados
- **Página:** `EmpleadoOnboarding.razor`
- **Ubicación:** `Components/Pages/`
- **Descripción:** Proceso guiado en 4 pasos para ingreso completo de empleados:
  - **PASO 1:** Datos básicos del empleado (validación completa)
  - **PASO 2:** Subir 17 documentos obligatorios según legislación colombiana
  - **PASO 3:** Subir documentos opcionales según el cargo
  - **PASO 4:** Revisar y confirmar antes de guardar
- **Características:**
  - Barra de progreso visual con 4 pasos
  - Validación de campos obligatorios
  - Vista previa de foto del empleado
  - Control de documentos con fechas de emisión y vencimiento
  - No bloquea si faltan documentos (se pueden subir después)
  - Estadísticas de documentos seleccionados
  - Confirmación final con resumen completo
- **Integración:**
  - Botón "WIZARD ONBOARDING" en página de Empleados
  - Mantiene opción "NUEVO RÁPIDO" para creación simple
  - Usa servicios existentes: `IEmpleadoRepository`, `IDocumentoEmpleadoRepository`, `ILocalStorageService`
- **Estilo:** Mantiene consistencia visual Windows 95/98 con toda la aplicación

---

## 📦 NUEVOS SERVICIOS REGISTRADOS

En `Program.cs`:
```csharp
// Memory Cache (requerido para CatalogCacheService)
builder.Services.AddMemoryCache();

// Cache y Session services
builder.Services.AddScoped<ICatalogCacheService, CatalogCacheService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
```

---

## 🆕 NUEVOS COMPONENTES COMPARTIDOS

| Componente | Archivo | Descripción |
|------------|---------|-------------|
| MessageToast | `Components/Shared/MessageToast.razor` | Notificaciones toast |
| UnsavedChangesGuard | `Components/Shared/UnsavedChangesGuard.razor` | Prevención de pérdida de cambios |
| NotificationBell | `Components/Shared/NotificationBell.razor` | Campana de notificaciones |

---

## 📝 NUEVAS INTERFACES

| Interface | Archivo | Descripción |
|-----------|---------|-------------|
| ICatalogCacheService | `Shared/Interfaces/ICatalogCacheService.cs` | Servicio de caché de catálogos |
| ISessionService | `Shared/Interfaces/ISessionService.cs` | Gestión de sesión |
| INotificationService | `Shared/Interfaces/INotificationService.cs` | Servicio de notificaciones |

---

## 🔧 ÁREAS DE MEJORA PENDIENTES

### 1. **Consistencia de UI**
- [ ] Unificar estilos de badges en todos los módulos
- [ ] Estandarizar anchos de modales
- [x] ~~Crear componente reutilizable para mensajes de error/éxito~~ → **MessageToast implementado**

### 2. **Rendimiento**
- [x] ~~Implementar paginación en tablas grandes~~ → **Paginación en DataTable y Empleados**
- [ ] Lazy loading de relaciones
- [x] ~~Caché de catálogos (departamentos, cargos, etc.)~~ → **CatalogCacheService implementado**

### 3. **UX**
- [x] ~~Confirmación antes de salir de formularios con cambios~~ → **UnsavedChangesGuard implementado**
- [ ] Auto-guardado de borradores
- [ ] Indicador de carga global
- [x] ~~Navegación con teclado en tablas (↑↓)~~ → **Implementado en DataTable**

### 4. **Funcionalidad**
- [ ] Completar módulo de Reportes
- [x] ~~Implementar generación de PDF en Permisos~~ → **GenerarActaPermisoAsync implementado**
- [x] ~~Agregar notificaciones en tiempo real~~ → **NotificationService + NotificationBell implementados**
- [ ] Implementar filtros avanzados guardables

### 5. **Seguridad**
- [x] ~~Agregar timeout de sesión~~ → **SessionService implementado**
- [ ] Log de auditoría completo
- [ ] Validación de permisos por endpoint

### 6. **Mobile/Responsive**
- [ ] Tablas responsivas (No priorizado)

---

## 📈 MÉTRICAS DE LA APLICACIÓN

| Métrica | Valor |
|---------|-------|
| Total de Páginas | 16 |
| Componentes Compartidos | 14 |
| Entidades de Dominio | 16 |
| Repositorios | 14 |
| Servicios | 10 |
| Enums | 11 |
| Interfaces (Shared) | 6 |

---

## 🗂️ RESUMEN DE ARCHIVOS

### Páginas (`Components/Pages/`)
1. `Login.razor` - Inicio de sesión
2. `Empleados.razor` - Gestión de empleados (con modo rápido y wizard)
3. `EmpleadoOnboarding.razor` - **[NUEVO]** Wizard de onboarding de empleados en 4 pasos
4. `Documentos.razor` - Gestión de documentos
5. `Permisos.razor` - Gestión de permisos
6. `Vacaciones.razor` - Gestión de vacaciones
7. `Contratos.razor` - Gestión de contratos
8. `ControlDiario.razor` - Control de actividades diarias
9. `ControlDiarioWizard.razor` - Asistente de registro masivo
10. `Catalogos.razor` - Gestión de catálogos (tabs)
11. `Usuarios.razor` - Gestión de usuarios
12. `Configuracion.razor` - Configuración del sistema
13. `Reportes.razor` - Generación de reportes
14. `Auditoria.razor` - Logs de auditoría

### Componentes Compartidos (`Components/Shared/`)
1. `FormModal.razor` - Modal genérico para formularios
2. `KeyboardHandler.razor` - Manejo de atajos de teclado
3. `DataTable.razor` - Tabla con selección, paginación y navegación por teclado
4. `ConfirmDialog.razor` - Diálogo de confirmación
5. `EmpleadoCard.razor` - Tarjeta de empleado
6. `EmpleadoSelector.razor` - Selector de empleado
7. `EstadoBadge.razor` - Badge de estado
8. `CalendarioMini.razor` - Calendario pequeño
9. `ResumenVacacionesPanel.razor` - Resumen de vacaciones
10. `AuthorizeViewLocal.razor` - Vista autorizada
11. `RedirectToLogin.razor` - Redirección a login
12. `MessageToast.razor` - **[NUEVO]** Notificaciones toast
13. `UnsavedChangesGuard.razor` - **[NUEVO]** Protección de cambios sin guardar
14. `NotificationBell.razor` - **[NUEVO]** Campana de notificaciones en header

### Layout (`Components/Layout/`)
1. `MainLayout.razor` - Layout principal
2. `EmptyLayout.razor` - Layout vacío (login)
3. `NavMenu.razor` - Menú de navegación

---

*Documento generado automáticamente para análisis de mejoras del sistema SGRRHH Local*
