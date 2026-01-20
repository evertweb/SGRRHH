---
name: deploy-ssh
description: Proceso de deploy de SGRRHH a servidores remotos vía SSH/SMB. SERVIDORES CONFIGURADOS - Servidor 1 (192.168.1.248:equipo1) Script Deploy-ToServer.ps1 - Servidor 2 (192.168.1.72:fores) Script Deploy-ToServer2.ps1. Usar cuando el usuario pida desplegar, actualizar servidores o verificar producción.
---
# Deploy SSH - SGRRHH

## Script Principal

**Ubicación:** `SGRRHH.Local/scripts/Deploy-ToServer.ps1`

## Configuración del Servidor

| Propiedad | Valor |
|-----------|-------|
| Host | `192.168.1.248` |
| Usuario SSH | `equipo1` |
| Ruta Remota | `C:\SGRRHH` |
| Share SMB | `\\192.168.1.248\SGRRHH` |
| Servicio | `SGRRHH_Local` |

## Archivos PROTEGIDOS

> [!CAUTION]
> Estos archivos/carpetas **NUNCA** se sobrescriben ni eliminan:

| Ruta | Contenido |
|------|-----------|
| `Data/` | Base de datos SQLite (`sgrrhh.db`) |
| `certs/` | Certificados SSL (`localhost+2.p12`) |
| `logs/` | Logs de la aplicación |
| `backups/` | Backups locales |
| `appsettings.json` | Configuración local del servidor |
| `appsettings.Development.json` | Configuración desarrollo |

## Comandos de Deploy

### Servidor 1 (Principal)

**Deploy Incremental:**
```powershell
cd c:\Users\evert\Documents\rrhh\SGRRHH.Local
.\scripts\Deploy-ToServer.ps1
```

**Deploy rápido:**
```powershell
.\scripts\Deploy-ToServer.ps1 -SkipBuild -Force
```

### Servidor 2 (Secundario)

**Deploy Incremental:**
```powershell
cd c:\Users\evert\Documents\rrhh\SGRRHH.Local
.\scripts\Deploy-ToServer2.ps1
```

**Deploy rápido:**
```powershell
.\scripts\Deploy-ToServer2.ps1 -SkipBuild -Force
```

### Opciones Disponibles (Ambos Servidores)

| Parámetro | Función |
|-----------|----------|
| `-SkipBuild` | Usa el build existente (no recompila) |
| `-Force` | No pide confirmación |
| `-FullSync` | Sync completo con /MIR (⚠️ peligroso) |
| `-IncludeDatabase` | Copia la BD local al servidor |

## Otros Scripts Disponibles

| Script | Función |
|--------|---------|
| `Deploy-Production.ps1` | Deploy local (sin SSH) |
| `Backup-Database.ps1` | Backup de la BD |
| `Restore-Backup.ps1` | Restaurar backup |
| `AutoUpdate-Service.ps1` | Actualización automática |

## Verificación Post-Deploy

### Servidor 1

**Conectividad SSH:**
```powershell
ssh equipo1@192.168.1.248 "echo OK"
```

**Estado del servicio:**
```powershell
ssh equipo1@192.168.1.248 "sc query SGRRHH_Local"
```

**Ver logs:**
```powershell
ssh equipo1@192.168.1.248 "type C:\SGRRHH\logs\*.log"
```

### Servidor 2

**Conectividad SSH:**
```powershell
ssh fores@192.168.1.72 "echo OK"
```

**Verificar proceso:**
```powershell
ssh fores@192.168.1.72 "Get-Process | Where-Object {$_.Name -like '*SGRRHH*'}"
```

**Ver logs:**
```powershell
ssh fores@192.168.1.72 "Get-Content C:\SGRRHH\logs\output.log -Tail 50"
```

**Acceso Web:**
- Servidor 1: `http://192.168.1.248:5002` | `https://192.168.1.248:5003`
- Servidor 2: `http://192.168.1.72:5002`

## Servicio Windows (NSSM)

- **Nombre:** `SGRRHH_Local`
- **Gestionado con:** `nssm`
- **Logs:** `C:\SGRRHH\logs` con rotación (1 MB / 24h)
- **Ejecutable:** `C:\SGRRHH\SGRRHH.Local.Server.exe`

## Accesos Directos en Servidor

| Acceso | Función |
|--------|---------|
| `SGRRHH - Consola` | Ejecución con ventana y logs en vivo |
| `SGRRHH - Ver Logs` | Tail del log actual |

## Certificado SSL

**Ruta:** `C:\SGRRHH\certs\localhost+2.p12`

Si falta el certificado, copiar antes de iniciar:
```powershell
scp certs/localhost+2.p12 equipo1@192.168.1.248:C:/SGRRHH/certs/
```

## Flujo de Deploy Completo

```
1. Compilar localmente (Release, self-contained, win-x64)
2. Detener servicio en servidor
3. Sincronizar archivos vía Robocopy/SMB
4. Preservar Data/, certs/, logs/
5. Iniciar servicio
6. Verificar salud de la aplicación
```

## Troubleshooting

### Error de esquema (columnas faltantes)
La DB no se migra automáticamente:
1. Respaldar la DB actual
2. Eliminar archivos `.wal` y `.shm`
3. Copiar la DB válida

### Error de conexión SMB
```powershell
net use \\192.168.1.248\SGRRHH /user:equipo1
```
