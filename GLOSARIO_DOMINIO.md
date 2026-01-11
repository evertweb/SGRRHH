# 📖 Glosario del Dominio - SGRRHH Local

> **Lenguaje Ubicuo**: Este documento define el vocabulario compartido entre el equipo técnico y el área de Recursos Humanos. Todos los términos aquí descritos deben usarse consistentemente en código, documentación y conversaciones.

---

## 🏢 Gestión de Personal

| Término | Definición | En Código | En BD |
|---------|------------|-----------|-------|
| **Empleado** | Persona natural vinculada laboralmente a la empresa | `Empleado` | `empleados` |
| **Trabajador** | Sinónimo de Empleado (usar preferentemente "Empleado") | - | - |
| **Colaborador** | Sinónimo de Empleado (evitar en código) | - | - |
| **Cédula** | Documento de identidad colombiano (CC) | `Empleado.Cedula` | `cedula` |
| **Código Empleado** | Identificador interno único del empleado | `Empleado.Codigo` | `codigo` |
| **Fecha de Ingreso** | Primer día laboral del empleado | `Empleado.FechaIngreso` | `fecha_ingreso` |
| **Fecha de Retiro** | Último día laboral (si aplica) | `Empleado.FechaRetiro` | `fecha_retiro` |
| **Antigüedad** | Años de servicio continuo en la empresa | `Empleado.Antiguedad` | Calculado |
| **Supervisor** | Empleado que tiene a cargo a otros empleados | `Empleado.Supervisor` | `supervisor_id` |

### Estados del Empleado

| Estado | Significado | Valor |
|--------|-------------|-------|
| **Activo** | Empleado laborando normalmente | `EstadoEmpleado.Activo` |
| **Incapacitado** | Empleado con incapacidad médica vigente | `EstadoEmpleado.Incapacitado` |
| **Vacaciones** | Empleado disfrutando período vacacional | `EstadoEmpleado.Vacaciones` |
| **Suspendido** | Contrato suspendido temporalmente | `EstadoEmpleado.Suspendido` |
| **Retirado** | Empleado que ya no labora en la empresa | `EstadoEmpleado.Retirado` |

---

## 📋 Contratos

| Término | Definición | En Código | Referencia Legal |
|---------|------------|-----------|------------------|
| **Contrato** | Acuerdo laboral entre empleado y empresa | `Contrato` | CST Art. 22 |
| **Término Fijo** | Contrato con fecha de finalización definida | `TipoContrato.TerminoFijo` | CST Art. 46 |
| **Término Indefinido** | Contrato sin fecha de finalización | `TipoContrato.Indefinido` | CST Art. 47 |
| **Obra o Labor** | Contrato que termina al finalizar la obra | `TipoContrato.ObraOLabor` | CST Art. 45 |
| **Prestación de Servicios** | Contrato civil, no laboral | `TipoContrato.PrestacionServicios` | Código Civil |
| **Aprendizaje** | Contrato de formación SENA | `TipoContrato.Aprendizaje` | Ley 789/2002 |

### Terminación de Contrato

| Término | Definición | En Código |
|---------|------------|-----------|
| **Renuncia Voluntaria** | El trabajador decide terminar el contrato | `MotivoTerminacionContrato.RenunciaVoluntaria` |
| **Despido con Justa Causa** | Terminación por falta grave del trabajador | `MotivoTerminacionContrato.DespidoJustaCausa` |
| **Despido sin Justa Causa** | Terminación unilateral del empleador (genera indemnización) | `MotivoTerminacionContrato.DespidoSinJustaCausa` |
| **Mutuo Acuerdo** | Ambas partes acuerdan terminar | `MotivoTerminacionContrato.MutuoAcuerdo` |
| **Vencimiento Término** | Contrato a término fijo cumple su fecha | `MotivoTerminacionContrato.VencimientoTerminoFijo` |
| **Liquidación** | Proceso de calcular y pagar prestaciones finales | `Contrato.LiquidacionId` |
| **Indemnización** | Compensación por despido sin justa causa | `Contrato.ValorIndemnizacion` |

---

## 🏥 Seguridad Social (Colombia)

| Término | Definición | En Código | Ejemplo |
|---------|------------|-----------|---------|
| **EPS** | Entidad Promotora de Salud | `Empleado.EPS` | Sura, Sanitas, Nueva EPS |
| **ARL** | Administradora de Riesgos Laborales | `Empleado.ARL` | Sura, Positiva, Colmena |
| **AFP** | Administradora de Fondos de Pensiones | `Empleado.AFP` | Porvenir, Protección, Colfondos |
| **Caja de Compensación** | Entidad de bienestar familiar | `Empleado.CajaCompensacion` | Comfama, Cafam, Comfandi |
| **Clase de Riesgo** | Nivel de riesgo laboral (I a V) | `Empleado.ClaseRiesgoARL` | Clase V para trabajos forestales |

---

## 📝 Permisos

| Término | Definición | En Código |
|---------|------------|-----------|
| **Permiso** | Autorización para ausentarse del trabajo | `Permiso` |
| **Número de Acta** | Identificador único del permiso | `Permiso.NumeroActa` |
| **Tipo de Permiso** | Categoría del permiso (médico, personal, etc.) | `TipoPermiso` |
| **Motivo** | Razón por la cual se solicita el permiso | `Permiso.Motivo` |
| **Documento Soporte** | Evidencia que justifica el permiso | `Permiso.DocumentoSoportePath` |

### Estados del Permiso

| Estado | Significado | En Código |
|--------|-------------|-----------|
| **Pendiente** | Esperando aprobación | `EstadoPermiso.Pendiente` |
| **Aprobado** | Autorizado y completamente cerrado | `EstadoPermiso.Aprobado` |
| **Rechazado** | No autorizado | `EstadoPermiso.Rechazado` |
| **Cancelado** | Anulado por el solicitante | `EstadoPermiso.Cancelado` |
| **Aprobado Pendiente Documento** | Autorizado pero falta entregar soporte | `EstadoPermiso.AprobadoPendienteDocumento` |
| **Aprobado en Compensación** | Autorizado, empleado debe compensar horas | `EstadoPermiso.AprobadoEnCompensacion` |
| **Completado** | Cerrado con todos los requisitos cumplidos | `EstadoPermiso.Completado` |

### Resolución del Permiso

| Tipo | Significado | En Código |
|------|-------------|-----------|
| **Remunerado** | Se paga completo, no hay descuento | `TipoResolucionPermiso.Remunerado` |
| **Descontado** | Se descuenta de la nómina | `TipoResolucionPermiso.Descontado` |
| **Compensado** | Empleado trabaja horas extra para compensar | `TipoResolucionPermiso.Compensado` |

---

## 🏨 Incapacidades

| Término | Definición | En Código |
|---------|------------|-----------|
| **Incapacidad** | Documento médico que certifica imposibilidad de trabajar | `Incapacidad` |
| **Número de Incapacidad** | Identificador único (ej: INC-2026-0001) | `Incapacidad.NumeroIncapacidad` |
| **Diagnóstico** | Condición médica que causa la incapacidad | `Incapacidad.DiagnosticoDescripcion` |
| **CIE-10** | Código internacional de enfermedades | `Incapacidad.DiagnosticoCIE10` |
| **Entidad Emisora** | Médico o IPS que expide la incapacidad | `Incapacidad.EntidadEmisora` |
| **Entidad Pagadora** | EPS o ARL que debe pagar la incapacidad | `Incapacidad.EntidadPagadora` |

### Tipos de Incapacidad

| Tipo | Definición | Quién Paga | En Código |
|------|------------|------------|-----------|
| **Enfermedad General** | Enfermedad no relacionada con trabajo | EPS (desde día 3) | `TipoIncapacidad.EnfermedadGeneral` |
| **Accidente de Trabajo** | Lesión ocurrida en el trabajo | ARL (desde día 1) | `TipoIncapacidad.AccidenteTrabajo` |
| **Enfermedad Laboral** | Enfermedad causada por el trabajo | ARL (desde día 1) | `TipoIncapacidad.EnfermedadLaboral` |
| **Licencia de Maternidad** | 18 semanas por parto | EPS (100%) | `TipoIncapacidad.LicenciaMaternidad` |
| **Licencia de Paternidad** | 2 semanas por nacimiento de hijo | EPS (100%) | `TipoIncapacidad.LicenciaPaternidad` |

### Proceso de Incapacidad

| Término | Definición | En Código |
|---------|------------|-----------|
| **Transcripción** | Registro de la incapacidad ante la EPS | `Incapacidad.Transcrita`, `RegistrarTranscripcionAsync()` |
| **Radicado** | Número asignado por EPS al transcribir | `Incapacidad.NumeroRadicadoEps` |
| **Prórroga** | Extensión de una incapacidad existente | `Incapacidad.EsProrroga`, `CrearProrrogaAsync()` |
| **Cobro** | Proceso de reclamar pago a EPS/ARL | `Incapacidad.Cobrada`, `RegistrarCobroAsync()` |
| **Días Empresa** | Días que paga el empleador (1-2 en enf. general) | `Incapacidad.DiasEmpresa` |
| **Días EPS/ARL** | Días que paga la entidad de salud | `Incapacidad.DiasEpsArl` |

### Estados de Incapacidad

| Estado | Significado | En Código |
|--------|-------------|-----------|
| **Activa** | Incapacidad vigente, empleado sin trabajar | `EstadoIncapacidad.Activa` |
| **Finalizada** | Período de incapacidad terminó | `EstadoIncapacidad.Finalizada` |
| **Transcrita** | Ya se registró ante EPS | `EstadoIncapacidad.Transcrita` |
| **Cobrada** | Ya se recibió pago de EPS/ARL | `EstadoIncapacidad.Cobrada` |
| **Cancelada** | Incapacidad anulada | `EstadoIncapacidad.Cancelada` |

---

## 🏖️ Vacaciones

| Término | Definición | En Código | Referencia Legal |
|---------|------------|-----------|------------------|
| **Vacaciones** | Descanso remunerado anual | `Vacacion` | CST Art. 186 |
| **Días Hábiles** | 15 días laborables por año trabajado | `Vacacion.DiasHabiles` | CST Art. 186 |
| **Días Calendario** | Total de días incluyendo fines de semana | `Vacacion.DiasCalendario` | - |
| **Período** | Año de causación de las vacaciones | `Vacacion.PeriodoCorrespondiente` | - |
| **Días Disponibles** | Días de vacaciones acumulados sin disfrutar | `Vacacion.DiasDisponibles` | - |

---

## 🌲 Silvicultura (Proyectos Forestales)

| Término | Definición | En Código |
|---------|------------|-----------|
| **Proyecto** | Unidad de trabajo forestal en un área geográfica | `Proyecto` |
| **Predio** | Finca o terreno donde se ubica el proyecto | `Proyecto.Predio` |
| **Lote** | Subdivisión del predio, también llamado "Rodal" | `Proyecto.Lote` |
| **Rodal** | Sinónimo de Lote (área homogénea de plantación) | - |
| **Especie Forestal** | Tipo de árbol plantado | `EspecieForestal` |
| **Hectárea (ha)** | Unidad de medida de área (10,000 m²) | `Proyecto.AreaHectareas` |

### Tipos de Proyecto Forestal

| Tipo | Definición | En Código |
|------|------------|-----------|
| **Plantación Nueva** | Establecimiento inicial de árboles | `TipoProyectoForestal.PlantacionNueva` |
| **Mantenimiento** | Cuidado de plantación existente | `TipoProyectoForestal.Mantenimiento` |
| **Raleo** | Reducción de densidad para favorecer crecimiento | `TipoProyectoForestal.Raleo` |
| **Cosecha** | Tala y extracción de madera | `TipoProyectoForestal.Cosecha` |

### Actividades Silviculturales

| Término | Definición | Unidad Típica |
|---------|------------|---------------|
| **Siembra** | Plantar árboles nuevos | Árboles/hora |
| **Plateo** | Limpiar maleza alrededor del árbol | Hectáreas/hora |
| **Poda** | Cortar ramas bajas del árbol | Árboles/hora |
| **Fertilización** | Aplicar nutrientes al suelo | Hectáreas/hora |
| **Control Fitosanitario** | Aplicar tratamientos contra plagas | Hectáreas/hora |
| **Rocería** | Cortar maleza entre hileras | Hectáreas/hora |

### Métricas Forestales

| Término | Definición | En Código |
|---------|------------|-----------|
| **Densidad** | Árboles por hectárea | `Proyecto.DensidadActual` |
| **Turno de Cosecha** | Años hasta la cosecha final | `Proyecto.TurnoCosechaAnios` |
| **Edad del Cultivo** | Años desde la siembra | `Proyecto.EdadCultivoAnios` |
| **Rendimiento** | Cantidad de trabajo por hora | `Actividad.RendimientoEsperado` |
| **Jornal** | Día de trabajo de un empleado | `Proyecto.TotalJornales` |

---

## 👤 Usuarios y Roles

| Término | Definición | En Código |
|---------|------------|-----------|
| **Usuario** | Persona con acceso al sistema | `Usuario` |
| **Aprobador** | Usuario con permisos para aprobar solicitudes | `AuthService.IsAprobador` |
| **Operador** | Usuario básico del sistema | `RolUsuario.Operador` |
| **Administrador** | Usuario con acceso completo | `RolUsuario.Administrador` |

> **Nota**: Actualmente todos los usuarios funcionan como Administrador. El sistema de roles será rediseñado.

---

## 💰 Nómina y Compensación

| Término | Definición | En Código |
|---------|------------|-----------|
| **Nómina** | Cálculo de pagos mensuales | `Nomina` |
| **Salario Base** | Remuneración mensual fija | `Empleado.SalarioBase` |
| **Prestaciones** | Beneficios legales (prima, cesantías, etc.) | `Prestacion` |
| **Descuento** | Deducción del salario | `Permiso.MontoDescuento` |
| **Compensación de Horas** | Trabajo adicional para compensar ausencias | `CompensacionHoras` |

---

## 📊 Estados Generales

Estos estados se usan en múltiples entidades:

| Estado | Significado General |
|--------|---------------------|
| **Pendiente** | Esperando acción o aprobación |
| **Aprobado** | Autorizado por persona competente |
| **Rechazado** | No autorizado |
| **Cancelado** | Anulado, ya no tiene efecto |
| **Completado** | Proceso finalizado exitosamente |
| **Activo** | Vigente, en uso |
| **Inactivo** | No vigente, pero no eliminado |

---

## 🔧 Términos Técnicos (Solo para Desarrollo)

| Término Técnico | Equivalente en Negocio | Uso |
|-----------------|------------------------|-----|
| `Id` | Identificador | Clave primaria en BD |
| `FechaCreacion` | Fecha de Registro | Auditoría |
| `FechaModificacion` | Última Actualización | Control de concurrencia |
| `Activo` (bool) | No Eliminado | Borrado lógico |
| `Path` | Ruta del Archivo | Ubicación de documentos |

---

## 📚 Referencias Legales

| Referencia | Descripción |
|------------|-------------|
| **CST** | Código Sustantivo del Trabajo (Colombia) |
| **Ley 100/1993** | Sistema de Seguridad Social |
| **Ley 789/2002** | Reforma laboral, contratos de aprendizaje |
| **Decreto 1072/2015** | Decreto Único Reglamentario del Sector Trabajo |

---

## 🔄 Historial de Cambios

| Fecha | Cambio | Autor |
|-------|--------|-------|
| 2026-01-10 | Creación inicial del glosario | Sistema |

---

*Este glosario debe actualizarse cada vez que se agregue un nuevo concepto al dominio o se identifique una inconsistencia en el lenguaje.*
