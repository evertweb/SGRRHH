# 📖 CONTEXTO RÁPIDO PARA LA IA - SGRRHH

> **LEE ESTO PRIMERO** - Resumen ejecutivo para retomar el proyecto rápidamente.

---

## 🎯 ¿QUÉ ES ESTE PROYECTO?

**SGRRHH** = Sistema de Gestión de Recursos Humanos

Una aplicación de escritorio **nativa de Windows** para un departamento de RRHH en Colombia que necesita:

1. **Gestionar empleados** - Expedientes completos, antigüedad, contratos
2. **Control diario** - Qué hizo cada empleado cada día (hora entrada/salida, actividades)
3. **Permisos/Licencias** - Solicitudes con flujo de aprobación (Secretaria solicita → Ingeniera aprueba)
4. **Vacaciones** - 15 días/año según ley colombiana
5. **Reportes y PDFs** - Actas de permiso, certificados laborales

---

## 🔧 TECNOLOGÍA

```
C# .NET 8 + WPF + SQLite + Entity Framework Core
```

- **100% Local** - Sin internet, sin servidor
- **3 PCs en red** - Carpeta compartida con la BD
- **~20 empleados** - Empresa pequeña

---

## 👥 USUARIOS

| Rol | Quién | Qué hace |
|-----|-------|----------|
| **Admin** | El desarrollador | Todo + configuración |
| **Operador** | Secretaria | Registra empleados, solicita permisos |
| **Aprobador** | Ingeniera | Aprueba/rechaza permisos |

---

## 📁 ESTRUCTURA DE ARCHIVOS

```
c:\Users\evert\Documents\rrhh\
├── docs/
│   ├── 03_REQUISITOS_DEFINITIVOS.md  ← Qué debe hacer el sistema
│   ├── 04_ARQUITECTURA_TECNICA.md    ← Cómo se construye
│   ├── 05_ROADMAP.md                 ← Plan de fases detallado
│   ├── 06_ESTADO_ACTUAL.md           ← Progreso actual
│   └── 00_CONTEXTO_IA.md             ← Este archivo
└── src/                              ← Código (aún no creado)
```

---

## 📊 ESTADO ACTUAL

| Fase | Estado |
|------|--------|
| 0 - Planificación | ✅ COMPLETADA |
| 1 - Fundación | ⬜ PENDIENTE ← **PRÓXIMA** |
| 2-10 | ⬜ PENDIENTES |

---

## 🚀 PARA CONTINUAR

### Lee estos archivos en orden:
1. `06_ESTADO_ACTUAL.md` - Ver exactamente dónde quedamos
2. `05_ROADMAP.md` - Ver tareas de la fase actual
3. `04_ARQUITECTURA_TECNICA.md` - Si necesitas detalles técnicos

### Luego:
- Continúa con las tareas pendientes de la fase actual
- Al terminar la sesión, actualiza `06_ESTADO_ACTUAL.md`

---

## ⚡ DECISIONES CLAVE YA TOMADAS

- ✅ WPF (no WinForms, no Electron)
- ✅ SQLite (no SQL Server, no archivos JSON)
- ✅ MVVM + Clean Architecture
- ✅ 3 roles: Admin, Operador, Aprobador
- ✅ Normativa laboral de Colombia
- ✅ QuestPDF para generar documentos
