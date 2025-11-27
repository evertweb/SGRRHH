# 🌐 Guía de Configuración en Red Local - SGRRHH

## Arquitectura Multi-PC

El sistema SGRRHH está diseñado para funcionar en una red local con múltiples usuarios accediendo simultáneamente a la misma base de datos.

```
┌─────────────────────────────────────────────────────────────┐
│                    RED LOCAL EMPRESA                        │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐        │
│  │  PC Admin   │  │PC Secretaria│  │PC Ingeniera │        │
│  │  (Servidor) │  │             │  │             │        │
│  │             │  │             │  │             │        │
│  │ [SGRRHH.exe]│  │ [SGRRHH.exe]│  │ [SGRRHH.exe]│        │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘        │
│         │                │                │                │
│         └────────────────┼────────────────┘                │
│                          │                                  │
│                          ▼                                  │
│              ┌───────────────────────┐                     │
│              │   CARPETA COMPARTIDA  │                     │
│              │   (En PC Servidor)    │                     │
│              │                       │                     │
│              │  \\SERVIDOR\SGRRHH\   │                     │
│              │  ├── sgrrhh.db        │                     │
│              │  ├── fotos\           │                     │
│              │  ├── documentos\      │                     │
│              │  └── backups\         │                     │
│              └───────────────────────┘                     │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 📋 Requisitos Previos

1. **Red Local funcionando** - Todas las PCs deben poder comunicarse entre sí
2. **Carpeta compartida** - Crear una carpeta compartida en el PC servidor
3. **Permisos** - Configurar permisos de lectura/escritura para todos los usuarios

---

## 🔧 Paso a Paso: Configuración del Servidor

### 1. Crear la carpeta compartida

En el PC que actuará como servidor (generalmente el PC del Administrador):

```powershell
# Crear estructura de carpetas
mkdir C:\SGRRHH_Data
mkdir C:\SGRRHH_Data\fotos
mkdir C:\SGRRHH_Data\documentos
mkdir C:\SGRRHH_Data\backups
mkdir C:\SGRRHH_Data\config
mkdir C:\SGRRHH_Data\logs
```

### 2. Compartir la carpeta

1. **Clic derecho** en `C:\SGRRHH_Data` → **Propiedades**
2. Ir a la pestaña **Compartir**
3. Clic en **Uso compartido avanzado...**
4. Marcar **Compartir esta carpeta**
5. Nombre del recurso: `SGRRHH`
6. Clic en **Permisos**:
   - **Todos** → Permisos: **Control total** ✅
   - O crear un grupo específico de usuarios
7. Clic en **Aplicar** y **Aceptar**

### 3. Configurar permisos NTFS

1. En **Propiedades** de la carpeta, ir a **Seguridad**
2. Clic en **Editar**
3. Agregar los usuarios que accederán al sistema
4. Dar permisos de **Modificar** y **Lectura y ejecución**

### 4. Verificar acceso desde otra PC

Desde otra PC en la red, abra el Explorador de archivos y escriba:
```
\\NOMBRE_PC_SERVIDOR\SGRRHH
```
O usando la IP:
```
\\192.168.1.100\SGRRHH
```

---

## ⚙️ Configuración de la Aplicación

### 1. Editar `appsettings.json`

En **CADA PC** donde se instale SGRRHH, editar el archivo `appsettings.json`:

**Ubicación:** Junto al ejecutable `SGRRHH.exe`

```json
{
  "Database": {
    "Path": "\\\\NOMBRE_SERVIDOR\\SGRRHH\\sgrrhh.db",
    "EnableWalMode": true,
    "BusyTimeout": 30000
  },
  "Network": {
    "IsNetworkMode": true,
    "SharedFolder": "\\\\NOMBRE_SERVIDOR\\SGRRHH"
  },
  "Application": {
    "Name": "SGRRHH",
    "Version": "1.0.0",
    "Company": "Mi Empresa"
  }
}
```

### Ejemplos de rutas:

| Escenario | Ruta de Base de Datos |
|-----------|----------------------|
| Por nombre de PC | `\\\\SERVIDOR\\SGRRHH\\sgrrhh.db` |
| Por dirección IP | `\\\\192.168.1.100\\SGRRHH\\sgrrhh.db` |
| Local (sin red) | `data/sgrrhh.db` |

> ⚠️ **Importante:** Use doble barra invertida (`\\\\`) en el JSON porque es un carácter de escape.

### 2. Configuración del PC Servidor

En el PC servidor, puede usar la ruta local directa:

```json
{
  "Database": {
    "Path": "C:\\SGRRHH_Data\\sgrrhh.db",
    "EnableWalMode": true,
    "BusyTimeout": 30000
  }
}
```

---

## 🔒 Modo WAL - Explicación

El sistema usa **SQLite con modo WAL (Write-Ahead Logging)** para permitir:

- ✅ **Lecturas concurrentes** - Múltiples usuarios pueden leer al mismo tiempo
- ✅ **Escrituras no bloqueantes** - Una escritura no bloquea las lecturas
- ✅ **Mayor rendimiento en red** - Optimizado para acceso remoto

### Parámetros configurables:

| Parámetro | Valor por defecto | Descripción |
|-----------|-------------------|-------------|
| `EnableWalMode` | `true` | Habilita el modo WAL |
| `BusyTimeout` | `30000` | Tiempo de espera en ms si la BD está bloqueada |

---

## 👥 Usuarios del Sistema

| Usuario | Contraseña | Rol | Permisos |
|---------|------------|-----|----------|
| admin | admin123 | Administrador | Acceso total |
| secretaria | secretaria123 | Operador | Registrar, solicitar permisos |
| ingeniera | ingeniera123 | Aprobador | Aprobar permisos, consultar |

> 🔐 **Cambie las contraseñas** después de la primera instalación.

---

## 🛠️ Solución de Problemas

### Error: "La base de datos está bloqueada"

**Causa:** Otro usuario tiene un bloqueo activo.

**Solución:**
1. Espere unos segundos e intente nuevamente
2. Si persiste, verifique que ningún proceso tenga el archivo abierto
3. Aumente el `BusyTimeout` en `appsettings.json`

### Error: "No se puede abrir la base de datos"

**Causas posibles:**
1. La carpeta compartida no está accesible
2. Permisos insuficientes
3. El firewall bloquea el acceso

**Soluciones:**
1. Verifique que puede acceder a `\\SERVIDOR\SGRRHH` desde el Explorador
2. Verifique permisos de la carpeta compartida
3. Agregue excepción en el firewall para compartir archivos

### Error: "Ruta de red no encontrada"

**Soluciones:**
1. Verifique que el PC servidor está encendido
2. Verifique el nombre del PC o IP
3. Use `ping NOMBRE_SERVIDOR` para verificar conectividad

### Rendimiento lento en red

**Optimizaciones:**
1. Asegúrese de que `EnableWalMode` esté en `true`
2. Use cable de red en lugar de WiFi
3. Verifique que no hay saturación en la red

---

## 📁 Estructura de la Carpeta Compartida

```
\\SERVIDOR\SGRRHH\
├── sgrrhh.db           # Base de datos principal
├── sgrrhh.db-wal       # Archivo WAL (generado automáticamente)
├── sgrrhh.db-shm       # Archivo de memoria compartida
├── fotos\              # Fotos de empleados
│   └── [empleado_id]\
├── documentos\         # Documentos adjuntos
│   ├── permisos\
│   └── contratos\
├── backups\            # Copias de seguridad
│   └── sgrrhh_YYYYMMDD_HHMMSS.db
├── config\             # Archivos de configuración
│   └── logo.png
└── logs\               # Logs de errores
    └── error_YYYY-MM-DD.log
```

---

## 🔄 Backup en Red

### Recomendaciones:

1. **Realizar backups diarios** desde el menú Configuración → Backup
2. **Guardar backups en otra ubicación** además de la carpeta compartida
3. **Verificar periódicamente** que los backups se están creando

### Backup automático (opcional):

Crear una tarea programada en Windows para copiar el backup:

```powershell
# Script de backup programado
$origen = "\\SERVIDOR\SGRRHH\sgrrhh.db"
$destino = "D:\Backups\SGRRHH\sgrrhh_$(Get-Date -Format 'yyyyMMdd').db"
Copy-Item $origen $destino -Force
```

---

## ✅ Lista de Verificación de Instalación en Red

- [ ] Carpeta compartida creada en el servidor
- [ ] Permisos de red configurados correctamente
- [ ] SGRRHH instalado en cada PC
- [ ] `appsettings.json` configurado con la ruta de red
- [ ] Primera ejecución exitosa (crea la base de datos)
- [ ] Los 3 usuarios pueden iniciar sesión
- [ ] Prueba de edición simultánea exitosa
- [ ] Backup configurado

---

## 📞 Soporte

Si tiene problemas con la configuración en red:

1. Revise los logs en: `[Carpeta SGRRHH]\data\logs\`
2. Verifique el archivo `appsettings.json`
3. Compruebe conectividad de red con `ping`
4. Verifique permisos de la carpeta compartida

---

*Última actualización: Noviembre 2025*
