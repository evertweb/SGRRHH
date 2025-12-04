# 🧪 GUÍA COMPLETA DE PRUEBAS - SGRRHH

> **Objetivo:** Esta guía te ayudará a probar sistemáticamente cada funcionalidad del sistema SGRRHH en el orden correcto para asegurar que todo funcione.

---

## 📋 ÍNDICE

1. [Orden de Pruebas (¿Qué va primero?)](#-orden-de-pruebas-qué-va-primero)
2. [Fase 1: Configuración Inicial](#fase-1-configuración-inicial)
3. [Fase 2: Catálogos Base](#fase-2-catálogos-base)
4. [Fase 3: Empleados](#fase-3-empleados)
5. [Fase 4: Control Diario](#fase-4-control-diario)
6. [Fase 5: Permisos y Licencias](#fase-5-permisos-y-licencias)
7. [Fase 6: Vacaciones](#fase-6-vacaciones)
8. [Fase 7: Contratos](#fase-7-contratos)
9. [Fase 8: Documentos PDF](#fase-8-documentos-pdf)
10. [Fase 9: Reportes](#fase-9-reportes)
11. [Fase 10: Dashboard](#fase-10-dashboard)
12. [Checklist de Verificación Final](#-checklist-de-verificación-final)

---

## 🎯 ORDEN DE PRUEBAS (¿QUÉ VA PRIMERO?)

### Diagrama de Dependencias

```
┌─────────────────────────────────────────────────────────────────────┐
│                        ORDEN DE CREACIÓN                            │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  1. USUARIOS          → Ya vienen por defecto (admin, secretaria,  │
│     (Configuración)      ingeniera)                                 │
│                                                                     │
│  2. DEPARTAMENTOS     → Necesarios para crear CARGOS               │
│     (Catálogo)                                                      │
│           ↓                                                         │
│  3. CARGOS            → Necesarios para crear EMPLEADOS            │
│     (Catálogo)                                                      │
│           ↓                                                         │
│  4. PROYECTOS         → Opcionales, usados en CONTROL DIARIO       │
│     (Catálogo)                                                      │
│           ↓                                                         │
│  5. ACTIVIDADES       → Opcionales, usadas en CONTROL DIARIO       │
│     (Catálogo)                                                      │
│           ↓                                                         │
│  6. TIPOS DE PERMISO  → Ya vienen por defecto (13 tipos colombianos)│
│     (Catálogo)                                                      │
│           ↓                                                         │
│  7. EMPLEADOS         → Necesarios para TODO lo demás:             │
│                         - Control Diario                            │
│                         - Permisos                                  │
│                         - Vacaciones                                │
│                         - Contratos                                 │
│                         - Documentos                                │
│           ↓                                                         │
│  8. CONTRATOS         → Asociados a empleados                       │
│           ↓                                                         │
│  9. CONTROL DIARIO    → Requiere empleados (y opcionalmente         │
│                         proyectos/actividades)                      │
│           ↓                                                         │
│  10. PERMISOS         → Requiere empleados + tipos de permiso      │
│           ↓                                                         │
│  11. VACACIONES       → Calculadas automáticamente desde la fecha  │
│                         de ingreso del empleado                     │
│           ↓                                                         │
│  12. DOCUMENTOS PDF   → Requiere empleados + datos de empresa      │
│           ↓                                                         │
│  13. REPORTES         → Depende de que haya datos en el sistema    │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

### Resumen Simple:
1. **Primero:** Departamentos y Cargos (catálogos base)
2. **Segundo:** Empleados (el centro de todo)
3. **Tercero:** Todo lo demás (permisos, contratos, vacaciones, etc.)

---

## FASE 1: CONFIGURACIÓN INICIAL

### 1.1 Iniciar Sesión
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ejecutar SGRRHH.exe | Abre ventana de login | ☐ |
| 2 | Ingresar usuario: `admin` | Campo se llena | ☐ |
| 3 | Ingresar contraseña: `admin123` | Campo muestra asteriscos | ☐ |
| 4 | Clic en "Iniciar Sesión" | Abre ventana principal | ☐ |
| 5 | Verificar menú lateral | Debe mostrar TODOS los módulos (eres admin) | ☐ |

### 1.2 Configurar Datos de Empresa (⚙️ Configuración)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a ⚙️ Configuración | Abre pantalla de configuración | ☐ |
| 2 | Ir a pestaña "Empresa" | Muestra formulario de empresa | ☐ |
| 3 | Ingresar Nombre de Empresa | Campo se actualiza | ☐ |
| 4 | Ingresar NIT | Campo se actualiza | ☐ |
| 5 | Ingresar Dirección | Campo se actualiza | ☐ |
| 6 | Ingresar Teléfono | Campo se actualiza | ☐ |
| 7 | Clic en "Guardar" | Mensaje de éxito | ☐ |
| 8 | (Opcional) Cargar Logo | El logo aparece en documentos PDF | ☐ |

### 1.3 Verificar Usuarios (👤 Usuarios)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a 👤 Usuarios | Muestra lista de usuarios | ☐ |
| 2 | Verificar 3 usuarios: admin, secretaria, ingeniera | Todos aparecen | ☐ |
| 3 | (Opcional) Crear nuevo usuario | El usuario se crea correctamente | ☐ |
| 4 | (Opcional) Editar usuario existente | Los cambios se guardan | ☐ |

---

## FASE 2: CATÁLOGOS BASE

> ⚠️ **IMPORTANTE:** Debes crear departamentos ANTES de crear cargos, y cargos ANTES de crear empleados.

### 2.1 Departamentos (🏢 Departamentos)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a 🏢 Departamentos | Muestra lista (ya hay algunos por defecto) | ☐ |
| 2 | Verificar departamentos existentes | Administración, Ingeniería, Operaciones | ☐ |
| 3 | Clic en "Nuevo Departamento" | Abre formulario | ☐ |
| 4 | Ingresar nombre: "Recursos Humanos" | Campo se llena | ☐ |
| 5 | Ingresar descripción | Campo se llena | ☐ |
| 6 | Clic en "Guardar" | Nuevo depto aparece en lista | ☐ |
| 7 | Editar un departamento existente | Los cambios se guardan | ☐ |
| 8 | Eliminar departamento (sin empleados) | Se elimina correctamente | ☐ |

### 2.2 Cargos (💼 Cargos)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a 💼 Cargos | Muestra lista (ya hay algunos por defecto) | ☐ |
| 2 | Verificar cargos existentes | Gerente, Secretaria, Ingeniero, etc. | ☐ |
| 3 | Clic en "Nuevo Cargo" | Abre formulario | ☐ |
| 4 | Seleccionar Departamento | Combo muestra los departamentos | ☐ |
| 5 | Ingresar nombre: "Auxiliar Contable" | Campo se llena | ☐ |
| 6 | Ingresar salario base (opcional) | Campo se llena | ☐ |
| 7 | Clic en "Guardar" | Nuevo cargo aparece en lista | ☐ |
| 8 | Editar un cargo existente | Los cambios se guardan | ☐ |

### 2.3 Proyectos (🚀 Proyectos)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a 🚀 Proyectos | Muestra lista de proyectos | ☐ |
| 2 | Clic en "Nuevo Proyecto" | Abre formulario | ☐ |
| 3 | Ingresar nombre: "Proyecto ABC" | Campo se llena | ☐ |
| 4 | Ingresar código: "PRY-001" | Campo se llena | ☐ |
| 5 | Ingresar descripción | Campo se llena | ☐ |
| 6 | Seleccionar estado: "Activo" | Se selecciona | ☐ |
| 7 | Clic en "Guardar" | Proyecto aparece en lista | ☐ |

### 2.4 Actividades (📝 Actividades)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a 📝 Actividades | Muestra lista de actividades | ☐ |
| 2 | Clic en "Nueva Actividad" | Abre formulario | ☐ |
| 3 | Ingresar nombre: "Revisión de documentos" | Campo se llena | ☐ |
| 4 | Seleccionar categoría | Se selecciona | ☐ |
| 5 | Clic en "Guardar" | Actividad aparece en lista | ☐ |

### 2.5 Tipos de Permiso (📋 Tipos de Permiso)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a 📋 Tipos de Permiso | Muestra 13 tipos (normativa colombiana) | ☐ |
| 2 | Verificar tipos existentes | Calamidad, Cita Médica, Luto, etc. | ☐ |
| 3 | Verificar configuración de cada tipo | Remunerado, Requiere Soporte, Días máx | ☐ |
| 4 | (Opcional) Crear tipo personalizado | Se crea correctamente | ☐ |
| 5 | (Opcional) Editar tipo existente | Los cambios se guardan | ☐ |

---

## FASE 3: EMPLEADOS

> ⚠️ **REQUISITO PREVIO:** Debes tener al menos 1 departamento y 1 cargo creados.

### 3.1 Crear Empleado Nuevo (👥 Empleados)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a 👥 Empleados | Muestra lista de empleados | ☐ |
| 2 | Clic en "Nuevo Empleado" | Abre formulario de empleado | ☐ |
| **DATOS PERSONALES** |
| 3 | Ingresar Nombres: "Juan Carlos" | Campo se llena | ☐ |
| 4 | Ingresar Apellidos: "Pérez López" | Campo se llena | ☐ |
| 5 | Ingresar Cédula: "12345678" | Campo se llena | ☐ |
| 6 | Seleccionar Género: "Masculino" | Se selecciona | ☐ |
| 7 | Seleccionar Estado Civil: "Soltero" | Se selecciona | ☐ |
| 8 | Ingresar Fecha Nacimiento | Selector de fecha funciona | ☐ |
| 9 | Ingresar Teléfono | Campo se llena | ☐ |
| 10 | Ingresar Email | Campo se llena | ☐ |
| 11 | Ingresar Dirección | Campo se llena | ☐ |
| **DATOS LABORALES** |
| 12 | Seleccionar Departamento | Combo muestra departamentos | ☐ |
| 13 | Seleccionar Cargo | Combo muestra cargos del depto | ☐ |
| 14 | Ingresar Fecha de Ingreso | ⚡ **MUY IMPORTANTE** para vacaciones | ☐ |
| 15 | Seleccionar Estado: "Activo" | Se selecciona | ☐ |
| 16 | Ingresar Salario | Campo se llena | ☐ |
| **FOTO (OPCIONAL)** |
| 17 | Clic en "Cargar Foto" | Abre selector de archivo | ☐ |
| 18 | Seleccionar imagen | La imagen se previsualiza | ☐ |
| **GUARDAR** |
| 19 | Clic en "Guardar" | Mensaje de éxito | ☐ |
| 20 | Verificar en lista | El empleado aparece | ☐ |

### 3.2 Editar Empleado Existente
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | En lista, clic en empleado | Se selecciona | ☐ |
| 2 | Clic en "Editar" o doble clic | Abre formulario con datos | ☐ |
| 3 | Modificar algún dato | El dato se actualiza | ☐ |
| 4 | Clic en "Guardar" | Cambios guardados | ☐ |

### 3.3 Ver Detalle de Empleado
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Seleccionar empleado | Se selecciona | ☐ |
| 2 | Clic en "Ver Detalle" | Abre ventana de detalle | ☐ |
| 3 | Verificar todas las pestañas | Información completa visible | ☐ |
| 4 | Ver antigüedad calculada | Muestra años/meses de servicio | ☐ |

### 3.4 Buscar y Filtrar Empleados
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Escribir en campo de búsqueda | Lista se filtra en tiempo real | ☐ |
| 2 | Filtrar por departamento | Solo muestra los del depto | ☐ |
| 3 | Filtrar por estado | Solo muestra activos/inactivos | ☐ |
| 4 | Limpiar filtros | Muestra todos | ☐ |

### 3.5 Crear al Menos 3 Empleados de Prueba

> 📝 **RECOMENDACIÓN:** Crea estos empleados para poder probar todos los flujos:

| Empleado | Fecha Ingreso | Propósito |
|----------|---------------|-----------|
| Juan Pérez | Hace 2 años | Probar vacaciones acumuladas |
| María García | Hace 6 meses | Probar vacaciones proporcionales |
| Pedro López | Hoy | Empleado nuevo |

---

## FASE 4: CONTROL DIARIO

> ⚠️ **REQUISITO PREVIO:** Al menos 1 empleado creado. Proyectos/Actividades son opcionales.

### 4.1 Registrar Actividad Diaria (📅 Control Diario)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a 📅 Control Diario | Muestra pantalla de registro | ☐ |
| 2 | Seleccionar fecha (hoy) | Fecha se actualiza | ☐ |
| 3 | Seleccionar empleado | Combo muestra empleados | ☐ |
| 4 | Ingresar hora entrada: "08:00" | Campo se llena | ☐ |
| 5 | Ingresar hora salida: "17:00" | Campo se llena | ☐ |
| 6 | (Opcional) Seleccionar proyecto | Combo muestra proyectos | ☐ |
| 7 | (Opcional) Seleccionar actividad | Combo muestra actividades | ☐ |
| 8 | Ingresar observaciones | Campo se llena | ☐ |
| 9 | Clic en "Guardar Registro" | Registro se guarda | ☐ |
| 10 | Verificar en lista del día | El registro aparece | ☐ |

### 4.2 Ver Registros por Fecha
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Cambiar fecha a día anterior | Lista se actualiza | ☐ |
| 2 | Ver registros de ese día | Muestra registros existentes | ☐ |
| 3 | Navegar entre fechas | Funciona correctamente | ☐ |

### 4.3 Editar Registro Existente
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Seleccionar registro | Se selecciona | ☐ |
| 2 | Clic en "Editar" | Carga datos en formulario | ☐ |
| 3 | Modificar hora salida | Se actualiza | ☐ |
| 4 | Guardar | Cambios guardados | ☐ |

---

## FASE 5: PERMISOS Y LICENCIAS

> ⚠️ **REQUISITO PREVIO:** Al menos 1 empleado y tipos de permiso configurados.

### 5.1 Crear Solicitud de Permiso (📝 Permisos - como Secretaria)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Cerrar sesión (admin) | Regresa a login | ☐ |
| 2 | Iniciar con: `secretaria` / `secretaria123` | Ingresa como Operador | ☐ |
| 3 | Ir a 📝 Permisos | Muestra lista de permisos | ☐ |
| 4 | Clic en "Nueva Solicitud" | Abre formulario | ☐ |
| 5 | Seleccionar Empleado | Combo muestra empleados | ☐ |
| 6 | Seleccionar Tipo de Permiso: "Cita Médica" | Se selecciona | ☐ |
| 7 | Ingresar Fecha Inicio | Selector funciona | ☐ |
| 8 | Ingresar Fecha Fin | Selector funciona | ☐ |
| 9 | Ingresar Motivo/Observaciones | Campo se llena | ☐ |
| 10 | (Si aplica) Adjuntar documento soporte | Se carga archivo | ☐ |
| 11 | Clic en "Guardar" | Solicitud creada | ☐ |
| 12 | Verificar estado: "Pendiente" | Aparece en lista | ☐ |

### 5.2 Aprobar/Rechazar Permiso (📝 Permisos - como Ingeniera)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Cerrar sesión (secretaria) | Regresa a login | ☐ |
| 2 | Iniciar con: `ingeniera` / `ingeniera123` | Ingresa como Aprobador | ☐ |
| 3 | Ir a 📝 Permisos | Muestra **Bandeja de Aprobación** | ☐ |
| 4 | Ver solicitudes pendientes | Aparece la creada por secretaria | ☐ |
| 5 | Seleccionar la solicitud | Se selecciona | ☐ |
| 6 | Clic en "Aprobar" | Pide confirmación | ☐ |
| 7 | Confirmar aprobación | Estado cambia a "Aprobado" | ☐ |
| 8 | Verificar que desaparece de pendientes | Ya no está en bandeja | ☐ |

### 5.3 Rechazar un Permiso
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Crear otra solicitud (como secretaria) | Solicitud creada | ☐ |
| 2 | Iniciar como ingeniera | | ☐ |
| 3 | Seleccionar solicitud | Se selecciona | ☐ |
| 4 | Clic en "Rechazar" | Pide motivo de rechazo | ☐ |
| 5 | Ingresar motivo | Campo se llena | ☐ |
| 6 | Confirmar rechazo | Estado cambia a "Rechazado" | ☐ |

### 5.4 Verificar Flujo Completo (como Admin)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Iniciar como admin | Acceso completo | ☐ |
| 2 | Ir a Permisos | Ve **Bandeja de Aprobación** (es aprobador también) | ☐ |
| 3 | Ver historial de permisos | Muestra aprobados y rechazados | ☐ |

---

## FASE 6: VACACIONES

> ⚠️ **REQUISITO PREVIO:** Empleados con fecha de ingreso configurada.

### 6.1 Ver Resumen de Vacaciones (🏖️ Vacaciones)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a 🏖️ Vacaciones | Muestra lista de empleados | ☐ |
| 2 | Ver columna "Días Acumulados" | Calculados automáticamente (15 días/año) | ☐ |
| 3 | Ver columna "Días Tomados" | Muestra días ya usados | ☐ |
| 4 | Ver columna "Días Disponibles" | Muestra saldo | ☐ |
| 5 | Verificar empleado con 2 años | ~30 días acumulados | ☐ |
| 6 | Verificar empleado con 6 meses | ~7.5 días acumulados | ☐ |

### 6.2 Registrar Vacaciones Tomadas
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Seleccionar empleado | Se selecciona | ☐ |
| 2 | Clic en "Registrar Vacaciones" | Abre formulario | ☐ |
| 3 | Ingresar fecha inicio | Selector funciona | ☐ |
| 4 | Ingresar fecha fin | Selector funciona | ☐ |
| 5 | Ver días calculados automáticamente | Muestra total de días | ☐ |
| 6 | Clic en "Guardar" | Vacación registrada | ☐ |
| 7 | Verificar que "Días Tomados" aumentó | Se actualizó | ☐ |
| 8 | Verificar que "Días Disponibles" bajó | Se actualizó | ☐ |

### 6.3 Ver Historial de Vacaciones
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Seleccionar empleado | Se selecciona | ☐ |
| 2 | Ver detalle/historial | Muestra todas las vacaciones | ☐ |
| 3 | Verificar fechas y días | Información correcta | ☐ |

---

## FASE 7: CONTRATOS

> ⚠️ **REQUISITO PREVIO:** Empleados creados.

### 7.1 Ver Contratos (📄 Contratos)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a 📄 Contratos | Muestra lista de contratos | ☐ |
| 2 | Ver contratos existentes | Muestra tipo, fechas, estado | ☐ |

### 7.2 Crear Nuevo Contrato
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Clic en "Nuevo Contrato" | Abre formulario | ☐ |
| 2 | Seleccionar Empleado | Combo funciona | ☐ |
| 3 | Seleccionar Tipo: "Término Fijo" | Se selecciona | ☐ |
| 4 | Ingresar Fecha Inicio | Selector funciona | ☐ |
| 5 | Ingresar Fecha Fin (6 meses después) | Selector funciona | ☐ |
| 6 | Ingresar Salario | Campo se llena | ☐ |
| 7 | Seleccionar Estado: "Vigente" | Se selecciona | ☐ |
| 8 | Clic en "Guardar" | Contrato creado | ☐ |

### 7.3 Verificar Alertas de Vencimiento
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Crear contrato que venza en 15 días | Contrato creado | ☐ |
| 2 | Ir a Dashboard | Ver alerta de "Contratos por vencer" | ☐ |
| 3 | Verificar contador | Muestra cantidad correcta | ☐ |

### 7.4 Renovar Contrato
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Seleccionar contrato vencido/por vencer | Se selecciona | ☐ |
| 2 | Clic en "Renovar" | Abre formulario de renovación | ☐ |
| 3 | Ingresar nuevas fechas | Campos se llenan | ☐ |
| 4 | Guardar | Nueva versión del contrato | ☐ |

---

## FASE 8: DOCUMENTOS PDF

> ⚠️ **REQUISITO PREVIO:** Datos de empresa configurados, empleados y permisos existentes.

### 8.1 Generar Certificado Laboral (📄 Documentos)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a 📄 Documentos | Muestra opciones de documentos | ☐ |
| 2 | Seleccionar "Certificado Laboral" | Se selecciona | ☐ |
| 3 | Seleccionar Empleado | Combo funciona | ☐ |
| 4 | (Opcional) Configurar opciones | Checkboxes funcionan | ☐ |
| 5 | Clic en "Generar Vista Previa" | PDF se muestra en pantalla | ☐ |
| 6 | Verificar logo de empresa | Aparece en encabezado | ☐ |
| 7 | Verificar datos del empleado | Nombre, cargo, fechas correctos | ☐ |
| 8 | Clic en "Descargar" | Se guarda archivo PDF | ☐ |
| 9 | Clic en "Imprimir" | Abre diálogo de impresión | ☐ |

### 8.2 Generar Constancia de Trabajo
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Seleccionar "Constancia de Trabajo" | Se selecciona | ☐ |
| 2 | Seleccionar Empleado | Combo funciona | ☐ |
| 3 | Ingresar destinatario: "A quien corresponda" | Campo se llena | ☐ |
| 4 | Generar vista previa | PDF se muestra | ☐ |
| 5 | Descargar | Archivo se guarda | ☐ |

### 8.3 Generar Acta de Permiso
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Seleccionar "Acta de Permiso" | Se selecciona | ☐ |
| 2 | Seleccionar permiso aprobado | Combo muestra permisos | ☐ |
| 3 | Generar vista previa | PDF se muestra con todos los datos | ☐ |
| 4 | Verificar número consecutivo | Número único visible | ☐ |
| 5 | Verificar firmas | Espacios para firmas | ☐ |
| 6 | Descargar | Archivo se guarda | ☐ |

---

## FASE 9: REPORTES

### 9.1 Generar Reportes (📈 Reportes)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a 📈 Reportes | Muestra opciones de reportes | ☐ |
| 2 | Seleccionar "Lista de Empleados" | Se genera reporte | ☐ |
| 3 | Ver datos en pantalla | Tabla con empleados | ☐ |
| 4 | (Si hay botón) Exportar/Imprimir | Funciona | ☐ |
| 5 | Seleccionar "Actividades por Empleado" | Se genera reporte | ☐ |
| 6 | Filtrar por fecha | Se actualiza | ☐ |
| 7 | Seleccionar "Horas por Proyecto" | Se genera reporte | ☐ |
| 8 | Verificar totales | Cálculos correctos | ☐ |

---

## FASE 10: DASHBOARD

### 10.1 Verificar Dashboard (📊 Dashboard)
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a 📊 Dashboard | Pantalla principal | ☐ |
| 2 | Ver mensaje de bienvenida | "Buenos días/tardes, [usuario]" | ☐ |
| 3 | Ver contador "Total Empleados" | Número correcto | ☐ |
| 4 | Ver "Permisos Pendientes" | Número correcto | ☐ |
| 5 | Ver "Contratos por Vencer" | Número correcto | ☐ |
| 6 | Clic en botón "Actualizar" | Datos se refrescan | ☐ |
| 7 | Verificar accesos rápidos | Funcionan los enlaces | ☐ |

---

## 🔐 PRUEBAS DE ROLES (MUY IMPORTANTE)

### Probar como OPERADOR (Secretaria)
| Módulo | Acceso Esperado | ✅ |
|--------|-----------------|---|
| Dashboard | ✅ Ver | ☐ |
| Empleados | ✅ Ver, Crear, Editar | ☐ |
| Control Diario | ✅ Ver, Crear, Editar | ☐ |
| Permisos | ✅ Ver propios, Crear solicitudes | ☐ |
| Vacaciones | ✅ Ver | ☐ |
| Contratos | ✅ Ver | ☐ |
| Reportes | ✅ Ver | ☐ |
| Documentos | ✅ Generar | ☐ |
| Configuración | ❌ NO ACCESO | ☐ |
| Usuarios | ❌ NO ACCESO | ☐ |
| Departamentos | ❌ NO ACCESO | ☐ |
| Cargos | ❌ NO ACCESO | ☐ |

### Probar como APROBADOR (Ingeniera)
| Módulo | Acceso Esperado | ✅ |
|--------|-----------------|---|
| Dashboard | ✅ Ver | ☐ |
| Empleados | ✅ Ver | ☐ |
| Control Diario | ✅ Ver | ☐ |
| Permisos | ✅ Ver BANDEJA, Aprobar/Rechazar | ☐ |
| Vacaciones | ✅ Ver | ☐ |
| Contratos | ✅ Ver | ☐ |
| Reportes | ✅ Ver | ☐ |
| Documentos | ✅ Generar | ☐ |
| Proyectos | ✅ Ver | ☐ |
| Configuración | ❌ NO ACCESO | ☐ |
| Usuarios | ❌ NO ACCESO | ☐ |

### Probar como ADMINISTRADOR
| Módulo | Acceso Esperado | ✅ |
|--------|-----------------|---|
| TODOS LOS MÓDULOS | ✅ Acceso completo | ☐ |

---

## ⚙️ PRUEBAS DE CONFIGURACIÓN Y BACKUP

### Backup de Base de Datos
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a Configuración > Backup | Muestra opciones | ☐ |
| 2 | Clic en "Crear Backup" | Se crea archivo .db | ☐ |
| 3 | Verificar en carpeta de backups | Archivo existe | ☐ |
| 4 | Ver lista de backups | Aparece el nuevo | ☐ |

### Restaurar Backup
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Seleccionar backup de lista | Se selecciona | ☐ |
| 2 | Clic en "Restaurar" | Pide confirmación | ☐ |
| 3 | Confirmar | Base de datos restaurada | ☐ |
| 4 | Verificar datos | Datos del backup cargados | ☐ |

### Log de Auditoría
| # | Acción | Resultado Esperado | ✅ |
|---|--------|-------------------|---|
| 1 | Ir a Configuración > Auditoría | Muestra log | ☐ |
| 2 | Ver acciones registradas | Login, creaciones, etc. | ☐ |
| 3 | Filtrar por fecha | Se filtra | ☐ |
| 4 | Filtrar por usuario | Se filtra | ☐ |

---

## ✅ CHECKLIST DE VERIFICACIÓN FINAL

### Sistema Base
- [ ] Login funciona con los 3 usuarios
- [ ] Logout funciona correctamente
- [ ] Cambio de contraseña funciona
- [ ] Menú muestra opciones según rol

### Datos Maestros
- [ ] Departamentos: CRUD completo
- [ ] Cargos: CRUD completo
- [ ] Proyectos: CRUD completo
- [ ] Actividades: CRUD completo
- [ ] Tipos de Permiso: CRUD completo

### Empleados
- [ ] Crear empleado con todos los campos
- [ ] Editar empleado existente
- [ ] Buscar y filtrar empleados
- [ ] Ver detalle completo
- [ ] Foto de empleado se guarda y muestra

### Control Diario
- [ ] Registrar entrada/salida
- [ ] Asociar a proyecto
- [ ] Ver registros por fecha
- [ ] Editar registros

### Permisos
- [ ] Crear solicitud (Operador)
- [ ] Aprobar permiso (Aprobador)
- [ ] Rechazar permiso (Aprobador)
- [ ] Ver historial

### Vacaciones
- [ ] Cálculo automático de días
- [ ] Registrar vacaciones tomadas
- [ ] Ver saldo actualizado

### Contratos
- [ ] Crear contrato
- [ ] Ver contratos por vencer
- [ ] Renovar contrato

### Documentos
- [ ] Certificado Laboral genera correctamente
- [ ] Constancia de Trabajo genera correctamente
- [ ] Acta de Permiso genera correctamente
- [ ] Logo de empresa aparece en PDFs

### Reportes
- [ ] Lista de empleados
- [ ] Actividades por empleado
- [ ] Horas por proyecto

### Dashboard
- [ ] Estadísticas correctas
- [ ] Alertas funcionan
- [ ] Accesos rápidos funcionan

### Configuración
- [ ] Datos de empresa se guardan
- [ ] Backup se crea y restaura
- [ ] Log de auditoría registra acciones

---

## 🚨 ERRORES COMUNES Y SOLUCIONES

| Error | Causa Probable | Solución |
|-------|----------------|----------|
| "No hay departamentos disponibles" | No has creado departamentos | Crear departamentos primero |
| "No hay cargos disponibles" | No has creado cargos | Crear cargos primero |
| "No hay empleados" | Lista vacía | Crear empleados primero |
| Vacaciones muestra 0 días | Empleado sin fecha de ingreso | Editar empleado y agregar fecha |
| Permiso no aparece en bandeja | Ya fue procesado | Revisar historial |
| PDF sin logo | No se ha configurado logo | Ir a Configuración > Empresa |
| Error de conexión | Firebase no configurado | Verificar firebase-credentials.json |

---

## 📞 SOPORTE

Si encuentras un error durante las pruebas:
1. Anota el paso exacto donde ocurrió
2. Toma captura de pantalla del error
3. Revisa el archivo de log en: `data/logs/error_YYYY-MM-DD.log`

---

**Documento creado:** 02/12/2025  
**Versión:** 1.0.0
