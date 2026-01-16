# Test Plan ControlDiario

## Funcionalidad basica
- [ ] Compilacion sin errores
- [ ] Cargar empleados del dia
- [ ] Navegar fechas (anterior/siguiente/hoy)
- [ ] Busqueda por empleado

## Registro individual
- [ ] Marcar entrada rapida
- [ ] Marcar salida rapida
- [ ] Editar hora entrada/salida inline
- [ ] Guardar observaciones
- [ ] Completar registro

## Actividades
- [ ] Abrir selector de actividades
- [ ] Crear actividad con horas
- [ ] Editar actividad existente
- [ ] Eliminar actividad con confirmacion
- [ ] Validar maximo 24 horas

## Resumen y reportes
- [ ] Resumen muestra totales correctos
- [ ] Contador de completados correcto
- [ ] Contador de pendientes correcto
- [ ] Total horas correcto
- [ ] Navegacion a Reportes/Wizard/Productividad

## Performance (critico)
- [ ] Carga inicial < 2s (100 empleados)
- [ ] Cambio de fecha < 1s
- [ ] Acciones masivas sin lag visible
