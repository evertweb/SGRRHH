# Analisis ControlDiario

## Mapa estructural actual

1. **Encabezado principal**
   - Titulo "CONTROL DIARIO".
   - Fecha destacada con formato largo.
   - Mensajes de error y exito.
2. **Barra principal de controles**
   - Navegacion de fecha (anterior/siguiente/hoy + input date).
   - Busqueda rapida por empleado.
   - Acciones: refrescar y crear registros faltantes.
3. **Resumen compacto**
   - Total registros filtrados vs totales.
   - Conteo por estado (completados/pendientes).
   - Total de horas acumuladas.
4. **Tabla principal**
   - Columnas: codigo, empleado, departamento, fecha, horas, estado, observaciones, acciones.
   - Edicion inline de hora entrada y salida.
   - Acciones rapidas por registro: entrada, salida, actividad, completar, ver detalle.
5. **Modal de detalle**
   - Informacion completa del registro.
   - Observaciones editables.
   - Tabla de actividades con indicadores de productividad.
6. **Modal de actividad**
   - Formulario para agregar/editar actividad.
   - Campos condicionales por categoria/proyecto/cantidad.
   - Preview de rendimiento y calculo de horas.
7. **Modal de confirmacion**
   - Confirmacion para eliminar actividad.
8. **Enlaces secundarios**
   - Wizard masivo, reportes y dashboard de productividad.

## Flujo de datos

1. **Inicializacion**
   - Validar autenticacion.
   - Parseo de fecha desde ruta.
   - Cargar maestros (empleados, departamentos, actividades, categorias, proyectos).
2. **Carga del dia**
   - Consultar registros por fecha.
   - Cargar detalles de actividades.
   - Vincular empleados y calcular empleados sin registro.
3. **Interaccion**
   - Filtrar por busqueda/estado/departamento.
   - Edicion inline de horarios y observaciones.
   - CRUD de actividades y recalculo de totales.
   - Cambios de estado a completado.

## Problemas de performance identificados

1. **N+1 en detalles de actividades**
   - Se consultan detalles por registro dentro de un foreach.
   - Impacto: una consulta por cada registro del dia.
2. **N+1 en empleados**
   - Si Empleado es null, se consulta por registro.
   - Impacto: consultas adicionales por cada registro.
3. **StateHasChanged frecuente**
   - Llamadas repetidas en operaciones masivas y guardados.
   - Impacto: re-render completo innecesario.

## Reglas de negocio criticas

| Regla | Ubicacion actual | Ubicacion propuesta |
|-------|------------------|---------------------|
| Total horas (sumatoria detalles) | Entidad `RegistroDiario` | Mantener en entidad |
| Validacion max 24 horas | UI (manual) | `RegistroDiarioService.ValidarHorasTotalesAsync` |
| Completar registro | UI | Servicio + UI |
| CRUD de detalles | UI + repositorio | `RegistroDiarioService` |
| Productividad (clasificacion) | UI | Componentes `RegistroAsistenciaModal` / `ActividadSelector` |

## Redundancias detectadas

1. **Calculo de horas por detalle**
   - Se calcula en varias vistas (tabla y modal).
2. **Parseo de TimeSpan**
   - Repetido en edicion de entrada/salida y modal de actividades.
3. **Mensajes de exito con auto-hide**
   - Patron repetido en varias operaciones.

## Estados criticos a preservar

1. Registro seleccionado y su detalle visible.
2. Edicion inline de hora entrada/salida (una fila a la vez).
3. Modal de actividad con datos persistidos durante la edicion.
4. Filtros de busqueda activos y resumen actualizado.
5. Mensajes de error/exito con auto-hide.
