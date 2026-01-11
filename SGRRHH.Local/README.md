# SGRRHH Local

Aplicación local para gestión de RRHH con base de datos SQLite y backend .NET 8.

## Requisitos
- .NET 8 SDK/Runtime
- Windows (probado en entorno local)

## Ejecución en Desarrollo
1. Restaurar dependencias y compilar: `dotnet build`
2. Iniciar servidor local: `dotnet run --project SGRRHH.Local.Server`
3. Abrir `https://localhost:5003` o la URL indicada en consola.

## Tests
- Proyecto de pruebas: `SGRRHH.Local.Tests`
- Ejecutar: `dotnet test SGRRHH.Local.Tests/SGRRHH.Local.Tests.csproj`
- Las pruebas usan una base SQLite temporal; no tocan la base de producción.

## Deploy

### Deploy a Servidor Remoto (SSH) - Recomendado
Despliega automáticamente al servidor de oficina (`192.168.1.248`) via SSH.

**Desde VS Code:**
- Presionar `Ctrl+Shift+B` → Seleccionar "0. Deploy a Servidor (SSH)"

**Desde Terminal:**
```powershell
powershell -ExecutionPolicy Bypass -File scripts\Deploy-ToServer.ps1
```

Documentación completa: [docs/SSH_DEPLOY_SETUP.md](docs/SSH_DEPLOY_SETUP.md)

### Deploy Local (C:\SGRRHH)
Para desplegar en este mismo equipo:
```powershell
powershell -ExecutionPolicy Bypass -File scripts\Deploy-Production.ps1
```

### Características del Sistema de Deploy
- ✅ Compilación automática Release, self-contained `win-x64`
- ✅ Empaquetado en ZIP y copia íntegra (incluye wwwroot completo)
- ✅ Limpieza de destino preservando `Data`, `certs` y `logs` (DB intacta)
- ✅ Servicio persistente con `nssm` (`SGRRHH_Local`), arranque automático (stdout/stderr en `C:\SGRRHH\logs` con rotación)
- ✅ Verificación de conexión SSH antes de compilar

### Consola y acceso directo en servidor
- En el escritorio del servidor existen:
	- `SGRRHH - Consola`: abre PowerShell en `C:\SGRRHH` y ejecuta la app con consola visible (para ver logs en vivo).
	- `SGRRHH - Ver Logs`: tail de `C:\SGRRHH\logs\nssm_stdout.log` en tiempo real.
- El servicio puede controlarse con `nssm status/start/stop SGRRHH_Local`.

## Documentación de usuario
El manual se encuentra en `SGRRHH.Local.Server/wwwroot/docs/manual-usuario.md` y se publica junto a la app.
