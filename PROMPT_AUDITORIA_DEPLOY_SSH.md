# PROMPT: Auditoría de Despliegue SSH y Limpieza Completa del Servidor

## 🎯 Objetivo

1. **Auditar** la metodología actual de despliegue SSH para identificar problemas potenciales
2. **Limpiar completamente** el servidor de producción
3. **Redesplegar** la aplicación de forma limpia y verificar funcionamiento

---

## 📋 Contexto

El proyecto SGRRHH.Local se despliega a un servidor Windows via SSH. Se han detectado problemas donde:
- Archivos CSS/JS no se copian correctamente
- El servidor no inicia después del despliegue
- La base de datos puede tener inconsistencias
- los scrips que crean la db y los datos seed pueden 

### Información del Servidor
- **Host**: 192.168.1.248
- **Usuario**: equipo1
- **Destino**: C:\SGRRHH
- **Método**: SSH + SCP desde Windows

---

## 🔍 FASE 1: Auditoría de la Metodología Actual

### 1.1 Revisar el Script de Despliegue Existente

Buscar y analizar:
```
SGRRHH.Local/scripts/Deploy-ToServer.ps1
```

Verificar:
- [ ] ¿Detiene correctamente el servidor antes de copiar?
- [ ] ¿Espera suficiente tiempo después de detener el proceso?
- [ ] ¿Copia TODOS los archivos necesarios (DLLs, wwwroot, configs)?
- [ ] ¿Maneja correctamente las subcarpetas (wwwroot/css, wwwroot/js)?
- [ ] ¿Inicia el servidor de forma persistente (no muere con la sesión SSH)?
- [ ] ¿Verifica que el servidor inició correctamente?

### 1.2 Revisar Documentación SSH

Analizar:
```
SGRRHH.Local/docs/SSH_DEPLOY_SETUP.md
```

### 1.3 Identificar Problemas Conocidos

Problemas detectados en sesiones anteriores:
1. **SCP no copia subcarpetas correctamente** - archivos en wwwroot/css quedaban vacíos
2. **Servidor no persiste** - al cerrar SSH el proceso muere
3. **Archivos bloqueados** - a veces el servidor sigue corriendo y bloquea DLLs

---

## 🧹 FASE 2: Limpieza Completa del Servidor

### 2.1 Detener TODOS los Procesos

```powershell
ssh equipo1@192.168.1.248 "taskkill /F /IM SGRRHH.Local.Server.exe 2>nul & taskkill /F /IM dotnet.exe 2>nul & echo Procesos terminados"
```

Esperar 5 segundos y verificar:
```powershell
ssh equipo1@192.168.1.248 "tasklist | findstr -i dotnet & tasklist | findstr -i SGRRHH"
```

### 2.2 Eliminar TODOS los Archivos de la Aplicación

```powershell
# Eliminar ejecutables y DLLs
ssh equipo1@192.168.1.248 "del /F /Q C:\SGRRHH\*.exe C:\SGRRHH\*.dll C:\SGRRHH\*.json C:\SGRRHH\*.config 2>nul"

# Eliminar carpeta wwwroot completa
ssh equipo1@192.168.1.248 "rmdir /S /Q C:\SGRRHH\wwwroot 2>nul"

# Eliminar carpeta runtimes
ssh equipo1@192.168.1.248 "rmdir /S /Q C:\SGRRHH\runtimes 2>nul"

# Eliminar LatoFont
ssh equipo1@192.168.1.248 "rmdir /S /Q C:\SGRRHH\LatoFont 2>nul"
```

### 2.3 Eliminar Base de Datos (para empezar limpio)

```powershell
ssh equipo1@192.168.1.248 "del /F /Q C:\SGRRHH\Data\sgrrhh.db* 2>nul & echo DB eliminada"
```

### 2.4 Verificar que Todo Está Limpio

```powershell
ssh equipo1@192.168.1.248 "dir C:\SGRRHH"
```

Solo debería quedar la carpeta `Data` vacía (o con Fotos).

---

## 📦 FASE 3: Compilar y Publicar Localmente

### 3.1 Compilar Release

```powershell
cd "c:\Users\evert\Documents\rrhh\SGRRHH.Local"
dotnet publish SGRRHH.Local.Server -c Release -o publish-ssh --self-contained false
```

### 3.2 Verificar Contenido de publish-ssh

```powershell
Get-ChildItem "c:\Users\evert\Documents\rrhh\SGRRHH.Local\publish-ssh" -Recurse | Measure-Object
Get-ChildItem "c:\Users\evert\Documents\rrhh\SGRRHH.Local\publish-ssh\wwwroot" -Recurse
```

Debe contener:
- [ ] SGRRHH.Local.Server.dll
- [ ] SGRRHH.Local.Server.exe (si es self-contained)
- [ ] wwwroot/css/hospital.css
- [ ] wwwroot/js/app.js
- [ ] appsettings.json

---

## 🚀 FASE 4: Despliegue Limpio

### 4.1 Método Recomendado: Copiar Todo de Una Vez

**IMPORTANTE**: SCP con `-r` puede tener problemas. Usar método alternativo:

```powershell
# Opción A: Comprimir y enviar
Compress-Archive -Path "c:\Users\evert\Documents\rrhh\SGRRHH.Local\publish-ssh\*" -DestinationPath "c:\temp\sgrrhh-deploy.zip" -Force
scp "c:\temp\sgrrhh-deploy.zip" equipo1@192.168.1.248:C:/SGRRHH/
ssh equipo1@192.168.1.248 "powershell Expand-Archive -Path C:\SGRRHH\sgrrhh-deploy.zip -DestinationPath C:\SGRRHH -Force; del C:\SGRRHH\sgrrhh-deploy.zip"
```

**O si SCP funciona bien:**

```powershell
# Copiar archivos raíz
scp "c:\Users\evert\Documents\rrhh\SGRRHH.Local\publish-ssh\*.*" equipo1@192.168.1.248:C:/SGRRHH/

# Copiar wwwroot (recursivo)
scp -r "c:\Users\evert\Documents\rrhh\SGRRHH.Local\publish-ssh\wwwroot" equipo1@192.168.1.248:C:/SGRRHH/

# Copiar runtimes si existe
scp -r "c:\Users\evert\Documents\rrhh\SGRRHH.Local\publish-ssh\runtimes" equipo1@192.168.1.248:C:/SGRRHH/

# Copiar LatoFont
scp -r "c:\Users\evert\Documents\rrhh\SGRRHH.Local\publish-ssh\LatoFont" equipo1@192.168.1.248:C:/SGRRHH/
```

### 4.2 Verificar que los Archivos se Copiaron

```powershell
ssh equipo1@192.168.1.248 "dir C:\SGRRHH\*.dll | findstr SGRRHH"
ssh equipo1@192.168.1.248 "dir C:\SGRRHH\wwwroot\css"
ssh equipo1@192.168.1.248 "dir C:\SGRRHH\wwwroot\js"
```

---

## 🖥️ FASE 5: Iniciar el Servidor de Forma Persistente

### 5.1 Problema con SSH y Procesos

Cuando ejecutas un proceso via SSH, el proceso muere al cerrar la conexión.

### 5.2 Solución: Usar Tarea Programada o Servicio

**Opción A: Crear tarea programada que inicie al login**

```powershell
ssh equipo1@192.168.1.248 "schtasks /create /tn SGRRHH_Server /tr \"dotnet C:\SGRRHH\SGRRHH.Local.Server.dll\" /sc onlogon /ru SYSTEM /f"
ssh equipo1@192.168.1.248 "schtasks /run /tn SGRRHH_Server"
```

**Opción B: Iniciar con Start-Process desde PowerShell**

```powershell
ssh equipo1@192.168.1.248 "powershell -Command Start-Process -FilePath dotnet -ArgumentList 'C:\SGRRHH\SGRRHH.Local.Server.dll' -WorkingDirectory 'C:\SGRRHH' -WindowStyle Hidden"
```

**Opción C: Usar nssm para crear servicio Windows**

### 5.3 Verificar que el Servidor Está Corriendo

```powershell
Start-Sleep -Seconds 5
ssh equipo1@192.168.1.248 "tasklist | findstr dotnet"
ssh equipo1@192.168.1.248 "netstat -an | findstr 5003"
```

---

## ✅ FASE 6: Validación Final

### 6.1 Verificar Logs de Inicio

```powershell
# El servidor debería crear la DB automáticamente
ssh equipo1@192.168.1.248 "if exist C:\SGRRHH\Data\sgrrhh.db (echo DB creada) else (echo DB NO existe)"
```

### 6.2 Probar Conectividad

Desde el equipo local, abrir navegador:
```
https://192.168.1.248:5003
```

### 6.3 Verificar que No Hay Errores SQLite

Si tienes acceso a logs:
```powershell
ssh equipo1@192.168.1.248 "type C:\SGRRHH\logs\*.log 2>nul | findstr -i error"
```

---

## 📝 Recomendaciones para Mejorar el Script de Despliegue

### Script Mejorado Propuesto

```powershell
# Deploy-ToServer-Improved.ps1

param(
    [string]$Server = "192.168.1.248",
    [string]$User = "equipo1",
    [string]$DestPath = "C:/SGRRHH"
)

$ErrorActionPreference = "Stop"

Write-Host "=== DESPLIEGUE SGRRHH ===" -ForegroundColor Cyan

# 1. Detener servidor
Write-Host "1. Deteniendo servidor..." -ForegroundColor Yellow
ssh $User@$Server "taskkill /F /IM SGRRHH.Local.Server.exe 2>nul; taskkill /F /IM dotnet.exe 2>nul"
Start-Sleep -Seconds 3

# 2. Compilar
Write-Host "2. Compilando..." -ForegroundColor Yellow
dotnet publish SGRRHH.Local.Server -c Release -o publish-ssh --self-contained false
if ($LASTEXITCODE -ne 0) { throw "Error en compilación" }

# 3. Comprimir
Write-Host "3. Comprimiendo..." -ForegroundColor Yellow
$zipPath = "$env:TEMP\sgrrhh-deploy.zip"
Remove-Item $zipPath -ErrorAction SilentlyContinue
Compress-Archive -Path "publish-ssh\*" -DestinationPath $zipPath -Force

# 4. Copiar ZIP
Write-Host "4. Copiando al servidor..." -ForegroundColor Yellow
scp $zipPath "${User}@${Server}:${DestPath}/"

# 5. Descomprimir en servidor
Write-Host "5. Descomprimiendo en servidor..." -ForegroundColor Yellow
ssh $User@$Server "powershell Expand-Archive -Path $DestPath/sgrrhh-deploy.zip -DestinationPath $DestPath -Force; Remove-Item $DestPath/sgrrhh-deploy.zip"

# 6. Iniciar servidor
Write-Host "6. Iniciando servidor..." -ForegroundColor Yellow
ssh $User@$Server "powershell Start-Process -FilePath dotnet -ArgumentList '$DestPath/SGRRHH.Local.Server.dll' -WorkingDirectory '$DestPath' -WindowStyle Hidden"

# 7. Verificar
Start-Sleep -Seconds 5
Write-Host "7. Verificando..." -ForegroundColor Yellow
$result = ssh $User@$Server "tasklist | findstr dotnet"
if ($result) {
    Write-Host "✓ Servidor iniciado correctamente" -ForegroundColor Green
} else {
    Write-Host "✗ El servidor no está corriendo" -ForegroundColor Red
}

Write-Host "=== DESPLIEGUE COMPLETADO ===" -ForegroundColor Cyan
```

---

## 🏁 Criterios de Éxito

La tarea está completa cuando:

1. ✅ El servidor está completamente limpio
2. ✅ La nueva compilación se copió sin errores
3. ✅ wwwroot contiene CSS y JS
4. ✅ El servidor inicia y permanece corriendo
5. ✅ La DB se crea automáticamente con seed data
6. ✅ La aplicación es accesible desde https://192.168.1.248:5003
7. ✅ No hay errores SQLite en los logs

---

## ⚠️ Troubleshooting

### El servidor no inicia
- Verificar que .NET 8 Runtime está instalado: `ssh equipo1@192.168.1.248 "dotnet --list-runtimes"`
- Revisar si hay error ejecutando directamente: `ssh equipo1@192.168.1.248 "cd C:\SGRRHH && dotnet SGRRHH.Local.Server.dll"`

### SCP no copia archivos
- Verificar permisos en C:\SGRRHH
- Intentar con método ZIP

### CSS/JS no cargan
- Verificar que wwwroot se copió: `ssh equipo1@192.168.1.248 "dir C:\SGRRHH\wwwroot\css\hospital.css"`
- Si está vacío, copiar wwwroot por separado

---

*Prompt creado: Enero 2026*
