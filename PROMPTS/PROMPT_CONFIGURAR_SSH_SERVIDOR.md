# PROMPT: Configuración Completa de SSH en Servidor Windows

## Contexto
Necesito configurar un servidor Windows para permitir acceso SSH sin contraseña usando el usuario administrador existente del sistema.

## Información del Entorno

**Servidor a Configurar:**
- Sistema: Windows 10/11 o Windows Server
- Usuario: El usuario administrador existente del sistema (único usuario)
- Acceso actual: Físico o RDP

**Mi PC (Cliente):**
- Usuario: `evert@ELITEBOOK-EVERT`
- Llave SSH pública: `ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIHFmmlCze8n99Uu7z2CULXOiVAnrhrK7pX7QFeVmwtRT evert@ELITEBOOK-EVERT`

## Objetivos

1. ✅ Instalar y configurar OpenSSH Server en el servidor Windows
2. ✅ Usar el usuario administrador existente para SSH (NO crear usuario nuevo)
3. ✅ Configurar autenticación basada en llave pública (sin contraseña)
4. ✅ Testear la conexión SSH desde mi PC
5. ✅ Documentar la configuración final (IP, usuario, etc.)

## Pasos a Ejecutar

### 1. Instalación de OpenSSH Server
Ejecutar en PowerShell como Administrador en el servidor:
- Verificar si OpenSSH está disponible
- Instalar OpenSSH.Server si no está instalado
- Iniciar el servicio sshd
- Configurar inicio automático del servicio
- Configurar firewall para permitir puerto 22

### 2. Identificar Usuario Administrador
- Obtener el nombre del usuario administrador actual del sistema
- Verificar que tiene permisos de administrador
- Usar este usuario para SSH (no crear uno nuevo)

### 3. Configurar Autenticación SSH sin Contraseña
- Crear el directorio `C:\ProgramData\ssh` si no existe
- Crear archivo `administrators_authorized_keys` con mi llave pública
- Configurar permisos correctos del archivo (solo SYSTEM y Administrators)
- Reiniciar servicio SSH

### 4. Obtener Información del Servidor
- IP del servidor (usar `ipconfig`)
- Nombre del usuario administrador
- Verificar grupo de administradores (nombre en español o inglés)

### 5. Testear Conexión
Desde mi PC (opcional si el agente puede):
- Intentar `ssh <usuario>@<ip> "echo OK"`
- Verificar que NO pida contraseña

## Comandos de Referencia

### Instalación SSH (Servidor)
```powershell
# Verificar disponibilidad
Get-WindowsCapability -Online | Where-Object Name -like 'OpenSSH*'

# Instalar
Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0

# Iniciar y configurar
Start-Service sshd
Set-Service -Name sshd -StartupType 'Automatic'

# Firewall
New-NetFirewallRule -Name sshd -DisplayName 'OpenSSH Server (sshd)' -Enabled True -Direction Inbound -Protocol TCP -Action Allow -LocalPort 22
```

### Configurar Llave Pública (Servidor)
```powershell
# Crear directorio
New-Item -Path "C:\ProgramData\ssh" -ItemType Directory -Force

# Agregar llave pública
$publicKey = "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIHFmmlCze8n99Uu7z2CULXOiVAnrhrK7pX7QFeVmwtRT evert@ELITEBOOK-EVERT"
Set-Content -Path "C:\ProgramData\ssh\administrators_authorized_keys" -Value $publicKey

# Configurar permisos
icacls "C:\ProgramData\ssh\administrators_authorized_keys" /inheritance:r
icacls "C:\ProgramData\ssh\administrators_authorized_keys" /grant "SYSTEM:(F)"
icacls "C:\ProgramData\ssh\administrators_authorized_keys" /grant "Administrators:(F)"

# Reiniciar SSH
Restart-Service sshd
```

### Obtener Información (Servidor)
```powershell
# IP del servidor
ipconfig | Select-String "IPv4"

# Usuario actual
$env:USERNAME

# Grupos del usuario
whoami /groups

# Verificar nombre del grupo de administradores
Get-LocalGroup | Where-Object {$_.SID -like "*-544"}
```

## Resultado Esperado

Al finalizar, proporcionarme:

```yaml
# Configuración del Servidor SSH
IP_SERVIDOR: <dirección IP>
USUARIO_SSH: <nombre del usuario administrador>
NOMBRE_GRUPO_ADMIN: <nombre del grupo en español o inglés>
PUERTO_SSH: 22
AUTENTICACION: Llave pública (sin contraseña)
```

Y confirmar que este comando funciona **sin pedir contraseña**:
```powershell
ssh <usuario>@<ip> "echo OK"
```

## Notas Importantes

1. **Usar usuario existente**: NO crear un usuario nuevo, usar el administrador actual
2. **Archivo especial**: Como es administrador, usar `C:\ProgramData\ssh\administrators_authorized_keys` (no `~/.ssh/authorized_keys`)
3. **Permisos estrictos**: SSH no funcionará si los permisos del archivo de llaves son incorrectos
4. **PowerShell como shell**: Opcionalmente configurar PowerShell como shell SSH por defecto
5. **Firewall**: Asegurar que puerto 22 esté abierto

## Troubleshooting

Si SSH pide contraseña después de configurar:
- Verificar que el archivo `administrators_authorized_keys` existe
- Verificar los permisos del archivo con `icacls`
- Revisar logs: `Get-Content C:\ProgramData\ssh\logs\sshd.log -Tail 50`
- Reiniciar servicio SSH

Si no puede conectar:
- Verificar firewall local del servidor
- Verificar firewall de red
- Ping al servidor para confirmar conectividad

## Contexto Adicional

Este servidor será usado para:
- Deploy de aplicación SGRRHH (ASP.NET Blazor Server)
- Sincronización de archivos via SSH/SMB
- Ejecución remota de comandos PowerShell
- Instalación de servicio Windows con NSSM

La configuración SSH es el primer paso. Después se configurarán:
- Share SMB para robocopy
- Instalación de la aplicación
- Servicio Windows
- Certificados SSL
