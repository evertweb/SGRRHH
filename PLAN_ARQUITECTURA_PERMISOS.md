# Plan de Arquitectura - Permisos

## Objetivo
Reducir complejidad de `Permisos.razor` extrayendo componentes UI y moviendo calculos de negocio a un servicio dedicado, manteniendo funcionalidad completa.

## Arquitectura propuesta
Permisos.razor queda como orquestador de estado y callbacks.

```
Permisos.razor
├─ PermisosHeader
├─ PermisosFilters
├─ PermisosTable
├─ PermisoFormModal
├─ PermisoAprobacionModal
├─ PermisoSeguimientoPanel
└─ PermisoCalculadora
```

## Responsabilidades por componente
1. `PermisosHeader.razor`
   - Cabecera y resumen (usuario, rol, fecha, totales)
2. `PermisosFilters.razor`
   - Busqueda, filtros de estado y empleado
3. `PermisosTable.razor`
   - Tabla de permisos, seleccion y acciones de fila
4. `PermisoFormModal.razor`
   - Crear/editar permisos, adjuntos y validaciones base
5. `PermisoAprobacionModal.razor`
   - Aprobar/rechazar y registrar seguimiento
6. `PermisoSeguimientoPanel.razor`
   - Visualizar historial y compensaciones (modal)
7. `PermisoCalculadora.razor`
   - Calculo de dias laborables usando servicio

## Servicio de calculo
### `IPermisoCalculationService` / `PermisoCalculationService`
Metodos:
- `CalcularDiasLaborablesAsync(fechaInicio, fechaFin)`
- `CalcularMontoDescuentoAsync(empleadoId, dias)`
- `TieneSolapamientoAsync(empleadoId, inicio, fin, permisoIdActual)`
- `ObtenerDiasFestivosEnRangoAsync(inicio, fin)`
- `ContarDiasSemanaEnRango(inicio, fin, diaSemana)`

Ubicacion:
- Interface: `SGRRHH.Local.Domain/Services/IPermisoCalculationService.cs`
- Implementacion: `SGRRHH.Local.Infrastructure/Services/PermisoCalculationService.cs`

## Consolidacion de logica
- Calculo de dias -> Servicio
- Solapamiento -> Servicio (usa repo)
- Descuento -> Servicio (usa empleado)

## Entregables
- Componentes extraidos
- Servicio registrado en DI
- `Permisos.razor` reducido a orquestador

