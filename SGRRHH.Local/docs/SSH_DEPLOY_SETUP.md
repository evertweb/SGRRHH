# Deploy SSH - Configuración y Uso

Este documento describe el sistema de deploy automatizado via SSH para SGRRHH.

## Configuración del Servidor

| Campo | Valor |
|-------|-------|
| **IP del Servidor** | `192.168.1.248` |
| **Usuario SSH** | `equipo1` |
| **Hostname** | `OFICINA` |
| **Carpeta Destino** | `C:\SGRRHH` |
| **Puerto HTTP** | 5002 |
| **Puerto HTTPS** | 5003 |

## Requisitos Previos

1. **Clave SSH configurada**: La autenticación SSH debe estar configurada sin contraseña
2. **OpenSSH**: Debe estar instalado en el equipo de desarrollo (viene con Windows 10+)
3. **nssm instalado en el servidor**: El ejecutable `nssm` debe estar en PATH (servicio `SGRRHH_Local` se recrea en cada deploy)
4. **Servidor encendido**: El servidor debe estar accesible en la red

## Uso del Deploy

### Desde VS Code (Recomendado)

1. Presionar `Ctrl+Shift+B`
2. Seleccionar "0. Deploy a Servidor (SSH)"

O usar la paleta de comandos:
- `Ctrl+Shift+P` → "Tasks: Run Task" → "0. Deploy a Servidor (SSH)"

### Desde Terminal

```powershell
powershell -ExecutionPolicy Bypass -File SGRRHH.Local\scripts\Deploy-ToServer.ps1
```

### Parámetros Opcionales

| Parámetro | Descripción |
|-----------|-------------|
| `-SkipBuild` | Usa el último build sin recompilar |
| `-Force` | No pide confirmación |
| `-IncludeDatabase` | Copia la base de datos (¡CUIDADO!) |

Ejemplo:
```powershell
.\Deploy-ToServer.ps1 -SkipBuild
```

### Qué hace el script
- Compila self-contained `win-x64` a `publish-ssh`
- Empaqueta en ZIP y copia a `C:\SGRRHH`
- Limpia destino excepto `Data`, `certs` y `logs` (no toca la base de datos)
- Descomprime ZIP en destino
- Crea/actualiza servicio `nssm` (`SGRRHH_Local`) apuntando a `SGRRHH.Local.Server.exe` y lo inicia
- Registra stdout/stderr en `C:\SGRRHH\logs` con rotación (1 MB / 24h)

## Verificar Estado del Servidor

```powershell
# Ver archivos en servidor
ssh equipo1@192.168.1.248 "dir C:\SGRRHH"

# Ver si la app está corriendo
ssh equipo1@192.168.1.248 "tasklist | findstr SGRRHH"

# Detener la app
ssh equipo1@192.168.1.248 "taskkill /F /IM SGRRHH.Local.Server.exe"
```

## Iniciar la Aplicación

### Servicio (recomendado)
- El deploy deja configurado el servicio `SGRRHH_Local` con nssm y lo arranca.
- Para controlar el servicio:
```powershell
ssh equipo1@192.168.1.248 "nssm restart SGRRHH_Local"
ssh equipo1@192.168.1.248 "nssm status SGRRHH_Local"
```

### Consola en vivo (para depurar)
- Detén primero el servicio si está corriendo: `ssh equipo1@192.168.1.248 "nssm stop SGRRHH_Local"`
- Usa el acceso directo en el escritorio `SGRRHH - Consola` (abre PowerShell en `C:\SGRRHH` y ejecuta la app con ventana visible).
- Para solo ver logs en vivo sin parar el servicio, usa el acceso directo `SGRRHH - Ver Logs` (tail de `C:\SGRRHH\logs\nssm_stdout.log`).
- Al terminar depuración, vuelve a iniciar el servicio: `ssh equipo1@192.168.1.248 "nssm start SGRRHH_Local"`.

### Fallback temporal (solo si falta nssm)
```powershell
ssh equipo1@192.168.1.248 "powershell -Command Start-Process -FilePath 'C:\SGRRHH\SGRRHH.Local.Server.exe' -WorkingDirectory 'C:\SGRRHH' -WindowStyle Hidden"
```

## Base de Datos

⚠️ **IMPORTANTE**: La base de datos **NUNCA** se sobrescribe durante el deploy.

### Primera vez (copiar base de datos inicial)
```powershell
scp "C:\SGRRHH\Data\sgrrhh.db" equipo1@192.168.1.248:C:/SGRRHH/Data/
```

### Backup de la base de datos del servidor
```powershell
scp equipo1@192.168.1.248:C:/SGRRHH/Data/sgrrhh.db "C:\Backups\sgrrhh_servidor.db"
```

## Certificados HTTPS

Los certificados deben copiarse manualmente la primera vez:
```powershell
ssh equipo1@192.168.1.248 "mkdir C:\SGRRHH\certs"
scp "C:\SGRRHH\certs\localhost+2.p12" equipo1@192.168.1.248:C:/SGRRHH/certs/
```

## Troubleshooting

### Error: "No se puede conectar al servidor"
```powershell
# Verificar red
ping 192.168.1.248

# Verificar SSH
ssh equipo1@192.168.1.248 "echo OK"
```

### Error: "address already in use"
La aplicación incluye detección automática de puertos. Si el puerto está ocupado, usará el siguiente disponible.

Para forzar el cierre:
```powershell
ssh equipo1@192.168.1.248 "taskkill /F /IM SGRRHH.Local.Server.exe"
```

### Error de certificados
- Si el servicio no arranca por `localhost+2.p12` faltante, copia el certificado: `scp C:\SGRRHH\certs\localhost+2.p12 equipo1@192.168.1.248:C:/SGRRHH/certs/`
- Verifica con `dir C:\SGRRHH\certs` en el servidor.

### Inconsistencias de base de datos (esquema)
- El deploy no toca la DB; si aparecen errores por columnas faltantes, sincroniza la DB local validada con el servidor: renombra la remota a `.bak`, elimina `.wal/.shm`, y copia la DB buena a `C:\SGRRHH\Data`. Haz backup antes.

### La app no abre el navegador automáticamente
- Verificar que no esté en modo Development
- El navegador solo se abre automáticamente en producción

## Estructura de Archivos en el Servidor

```
C:\SGRRHH\
├── SGRRHH.Local.Server.exe    # Ejecutable principal
├── appsettings.json           # Configuración
├── Data\
│   ├── sgrrhh.db             # Base de datos (NO se sobrescribe)
│   └── Backups\              # Backups locales
├── certs\
│   └── localhost+2.p12       # Certificado HTTPS
├── wwwroot\                   # Archivos estáticos
└── [otros DLLs y recursos]
```

## Características del Deploy

1. **Verificación de conexión**: Falla temprano si no hay SSH
2. **Detención automática**: Cierra el proceso remoto antes de copiar
3. **Exclusión de archivos**: No copia DB, logs ni appsettings.Development.json
4. **Puerto automático**: La app busca puertos disponibles si hay conflicto
5. **Navegador automático**: Abre el navegador al iniciar (solo producción)

---

*Última actualización: Enero 2026*
