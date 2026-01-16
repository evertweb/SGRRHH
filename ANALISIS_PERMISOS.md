# Analisis - Permisos.razor

## Contexto
Componente objetivo: `SGRRHH.Local/SGRRHH.Local.Server/Components/Pages/Permisos.razor`
Tamano actual aproximado: 1,500 lineas
Responsabilidad: gestion completa de permisos (consulta, filtros, creacion, edicion, aprobacion, rechazo, PDF, conversion a incapacidad)

## Mapa estructural (UI)
1. Cabecera
   - Titulo y datos de usuario/rol/fecha
   - Atajos de teclado (F2/F3/F4/F5/F6/F7/F12/Esc)
2. Controles principales
   - Nuevo, Actualizar, Ver detalle, Aprobar/Rechazar (si aprobador), Generar PDF
3. Filtros
   - Busqueda por texto
   - Estado
   - Empleado
4. Tabla de permisos
   - Listado con seleccion, acciones de ver/PDF
5. Modal formulario
   - Creacion/edicion
   - Calculo de dias
   - Descarga/subida de soporte
   - Acciones de aprobacion/rechazo y conversion a incapacidad
6. Dialogos
   - Rechazo
   - Aprobacion con resolucion
   - Conversion a incapacidad

## Lógica de negocio identificada
### Calculo de dias
- Se calcula entre FechaInicio y FechaFin
- Excluye fines de semana
- Actualmente en UI (metodo `CalcularDias`)
- Debe considerar festivos (repositorio de festivos disponible)

### Validaciones de fechas
- FechaInicio no puede ser anterior a hoy
- FechaFin no puede ser anterior a FechaInicio
- TotalDias debe ser > 0
- Solapamiento: consulta a repositorio (`ExisteSolapamientoAsync`)

### Flujo de aprobacion
- Solo roles aprobadores pueden aprobar/rechazar
- Aprobacion puede ser remunerada, descontada o compensada
- Puede quedar pendiente de documento o compensacion

### Conversion a incapacidad
- Solo permisos medicos y aprobados
- Requiere datos adicionales (diagnostico, entidad, fechas)
- Crea incapacidad desde permiso

## Dependencias y servicios usados
- `IAuthService`
- `IPermisoRepository`
- `ISeguimientoPermisoRepository`
- `IIncapacidadRepository`
- `IReportService`
- `ICatalogCacheService`
- `ILocalStorageService`
- `IJSRuntime`

## Redundancias identificadas
1. Formato de fechas
   - `ToString("dd/MM/yyyy")` repetido en tabla, formularios y modales
2. Calculo de dias
   - Metodo `CalcularDias` en UI
3. Validaciones de fechas
   - Validaciones en `Guardar` (posible duplicacion en otros flujos)
4. Solapamiento
   - Verificacion en guardado usando repositorio

## Riesgos actuales
- Logica de calculo en UI sin considerar festivos
- Manejo de atajos via `eval` (no se cambia en refactor)
- Modales acoplados a estado del componente principal

