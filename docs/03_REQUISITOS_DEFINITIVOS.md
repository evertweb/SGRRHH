# 📋 Documento de Requisitos Definitivos - Sistema RRHH

## 🎯 Resumen Ejecutivo

| Aspecto | Definición |
|---------|------------|
| **Nombre** | SGRRHH - Sistema de Gestión de Recursos Humanos |
| **Tipo** | Aplicación de escritorio nativa Windows |
| **Conexión** | 100% Local / Offline |
| **Usuarios** | 3 (Admin, Secretaria, Ingeniera) |
| **Empleados** | ~20 trabajadores |
| **País** | Colombia 🇨🇴 |
| **Base de datos** | Local (SQLite) |
| **Red** | 3 PCs compartiendo datos en red local |

---

## 👥 USUARIOS DEL SISTEMA

### Roles y Permisos:

| Rol | Usuario | Permisos |
|-----|---------|----------|
| **Administrador** | Tú (Desarrollador) | Todo: configurar, crear usuarios, backup, ver todo |
| **Secretaria** | Secretaria RRHH | Registrar empleados, control diario, solicitar permisos, consultar |
| **Aprobador** | Ingeniera | Aprobar/rechazar permisos, consultar reportes, ver todo |

### Flujo de Trabajo:
```
Secretaria registra → Ingeniera aprueba → Sistema actualiza
         ↓                    ↓
    Control diario      Permisos/Licencias
```

---

## 🇨🇴 NORMATIVA LABORAL COLOMBIA

### Vacaciones:
- **15 días hábiles** por año trabajado
- Se acumulan proporcionalmente
- Máximo acumulable: 2 años (después se deben tomar)

### Licencias Remuneradas (Código Sustantivo del Trabajo):
| Tipo | Días | Artículo |
|------|------|----------|
| Licencia de maternidad | 18 semanas | Art. 236 |
| Licencia de paternidad | 2 semanas | Art. 236 |
| Licencia por luto | 5 días hábiles | Art. 57 |
| Licencia de matrimonio | Según convención/reglamento | - |
| Calamidad doméstica | Según caso | Art. 57 |

### Incapacidades:
- **Días 1-2**: Paga el empleador (66.67% del salario)
- **Días 3-90**: Paga la EPS (66.67%)
- **Días 91-180**: Paga la EPS (50%)
- **Día 181+**: Paga el fondo de pensiones

### Jornada Laboral:
- Máximo: 47 horas semanales (2023+)
- Reducción gradual hasta 42 horas en 2026

---

## 🧩 MÓDULOS CONFIRMADOS

### MÓDULO 1: Gestión de Empleados ✅
- Expediente completo
- Datos personales y laborales
- Foto del empleado
- Documentos adjuntos (contratos, cédula, etc.)
- Cálculo automático de antigüedad
- Estados: Activo, Inactivo, Vacaciones, Licencia, Retirado

### MÓDULO 2: Control Diario ✅
- Registro de hora entrada/salida
- Múltiples actividades por día
- Actividades predefinidas en catálogo
- Asociación a proyectos
- Categorías de actividades
- Observaciones y estado de avance

### MÓDULO 3: Permisos y Licencias ✅
- Flujo: Secretaria solicita → Ingeniera aprueba
- Tipos de permiso colombianos
- Tipos de compensación
- Documentos soporte (certificados médicos, etc.)
- Acta formal imprimible
- Número consecutivo automático

### MÓDULO 4: Contratos y Antigüedad ✅
- Historial de contratos
- Alertas de vencimiento
- Tipos: Indefinido, Fijo, Obra/Labor, Temporal
- Renovaciones

### MÓDULO 5: Vacaciones ✅
- 15 días hábiles/año (Colombia)
- Cálculo automático según antigüedad
- Días tomados vs disponibles
- Programación de vacaciones

### MÓDULO 6: Catálogos ✅
- Departamentos/Áreas
- Cargos
- Actividades (por categoría)
- Proyectos
- Tipos de permiso
- Tipos de contrato

### MÓDULO 7: Reportes y Documentos ✅
- Reportes varios
- Acta de permiso (imprimible)
- Certificado laboral
- Constancia de trabajo

### MÓDULO 8: Dashboard ✅
- Panel principal con alertas
- Gráficos de ausentismo
- Contratos por vencer
- Cumpleaños y aniversarios

### MÓDULO 9: Configuración ✅
- Gestión de usuarios
- Backup de base de datos
- Datos de la empresa
- Parámetros del sistema

---

## 📊 REPORTES CONFIRMADOS

### Empleados:
- ✅ Listado general
- ✅ Ficha individual/Expediente
- ✅ Por departamento
- ✅ Por tipo de contrato
- ✅ Contratos próximos a vencer
- ✅ Cumpleaños del mes
- ✅ Aniversarios laborales

### Control Diario:
- ✅ Registro por fecha
- ✅ Actividades por empleado
- ✅ Empleados por actividad
- ✅ Horas por proyecto
- ✅ Resumen mensual

### Permisos:
- ✅ Por empleado
- ✅ Por tipo
- ✅ Pendientes de aprobar
- ✅ Estadísticas de ausentismo
- ✅ Días compensatorios pendientes

### Vacaciones:
- ✅ Estado por empleado
- ✅ Programadas
- ✅ Pendientes por tomar

---

## 📄 DOCUMENTOS A GENERAR

### 1. Acta de Permiso
```
╔════════════════════════════════════════════════════════════╗
║              [LOGO EMPRESA]                                ║
║         ACTA DE PERMISO No. 2025-0001                     ║
╠════════════════════════════════════════════════════════════╣
║ Fecha: 26/11/2025                                         ║
║ Empleado: Pedro Pérez - C.C. 12.345.678                   ║
║ Cargo: Auxiliar Administrativo                            ║
║ Departamento: Administración                              ║
║                                                            ║
║ TIPO DE PERMISO: Diligencias personales                   ║
║ MOTIVO: Cita en notaría                                   ║
║ FECHA(S): 27/11/2025                                      ║
║ HORARIO: 08:00 - 12:00 (4 horas)                         ║
║                                                            ║
║ COMPENSACIÓN: Compensatorio                               ║
║ FECHA A COMPENSAR: 30/11/2025                            ║
║                                                            ║
║ ESTADO: ☑ APROBADO  ☐ RECHAZADO                          ║
║ Aprobado por: Ing. María García                          ║
║ Fecha aprobación: 25/11/2025                             ║
║                                                            ║
║ ____________________    ____________________              ║
║ Firma Empleado          Firma Aprobador                   ║
╚════════════════════════════════════════════════════════════╝
```

### 2. Certificado Laboral
### 3. Constancia de Trabajo

---

## 🔔 ALERTAS DEL SISTEMA

| Alerta | Anticipación | Para quién |
|--------|--------------|------------|
| Contrato por vencer | 30, 15, 7 días | Admin, Ingeniera |
| Permiso pendiente de aprobar | Inmediato | Ingeniera |
| Vacaciones acumuladas (+30 días) | Semanal | Admin |
| Cumpleaños próximo | 7 días | Todos |
| Aniversario laboral | 7 días | Todos |
| Día compensatorio pendiente | Semanal | Secretaria |
| Incapacidad por vencer | 3 días | Admin |

---

## ✅ REQUISITOS TÉCNICOS CONFIRMADOS

| Requisito | Especificación |
|-----------|----------------|
| Plataforma | Windows 10/11 |
| Instalación | Local, sin internet |
| Base de datos | SQLite (archivo local) |
| Red | Carpeta compartida entre 3 PCs |
| Backup | Manual/Automático a carpeta |
| Documentos | Almacenados en carpeta local |
| Impresión | Soporte para impresoras locales |
| Idioma | Español |

---

## 📝 DATOS DE LA EMPRESA (A configurar)

```
Nombre de la empresa: _______________
NIT: _______________
Dirección: _______________
Ciudad: _______________
Teléfono: _______________
Logo: [Imagen]
```

---

**Documento aprobado:** ⬜ Pendiente de revisión del usuario

**Siguiente paso:** Arquitectura técnica y selección de tecnología
