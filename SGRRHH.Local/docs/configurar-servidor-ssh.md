# Configuración SSH en Servidor Windows - SGRRHH
## Ejecutar como Administrador en PowerShell

> **Importante:** Estos comandos requieren permisos de Administrador.  
> Todo lo demás (carpetas, shares, servicios) se configurará remotamente después.

---

## 1. Instalar OpenSSH Server

```powershell
# Verificar si OpenSSH está disponible
Get-WindowsCapability -Online | Where-Object Name -like 'OpenSSH*'

# Instalar OpenSSH Server (si no está instalado)
Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0

# Verificar instalación
Get-WindowsCapability -Online | Where-Object Name -like 'OpenSSH.Server*'
```

---

## 2. Iniciar y Configurar Servicio SSH

```powershell
# Iniciar servicio SSH
Start-Service sshd

# Configurar inicio automático
Set-Service -Name sshd -StartupType 'Automatic'

# Verificar que esté corriendo
Get-Service sshd
```

---

## 3. Configurar Firewall para SSH

```powershell
# Crear regla de firewall para SSH (puerto 22)
New-NetFirewallRule -Name sshd -DisplayName 'OpenSSH Server (sshd)' -Enabled True -Direction Inbound -Protocol TCP -Action Allow -LocalPort 22

# Verificar regla creada
Get-NetFirewallRule -Name sshd
```

---

## 4. Crear Usuario para Deploy

```powershell
# REEMPLAZA <USUARIO> y <PASSWORD> con tus valores
$password = ConvertTo-SecureString "<PASSWORD>" -AsPlainText -Force
New-LocalUser -Name "<USUARIO>" -Password $password -FullName "Usuario Deploy SGRRHH" -Description "Usuario para SSH deploy"

# Verificar creación
Get-LocalUser -Name "<USUARIO>"
```

---

## 5. Agregar Usuario al Grupo de Administradores

```powershell
# Primero, listar grupos para verificar el nombre correcto
Get-LocalGroup

# El nombre del grupo varía según el idioma:
# - Español: "Administradores"  
# - Inglés: "Administrators"

# REEMPLAZA <NOMBRE_GRUPO_ADMIN> y <USUARIO>
Add-LocalGroupMember -Group "<NOMBRE_GRUPO_ADMIN>" -Member "<USUARIO>"

# Verificar que se agregó correctamente
Get-LocalGroupMember -Group "<NOMBRE_GRUPO_ADMIN>"
```

---

## 6. (Opcional) Configurar PowerShell como Shell SSH

```powershell
# Esto permite ejecutar comandos PowerShell directamente via SSH
New-ItemProperty -Path "HKLM:\SOFTWARE\OpenSSH" -Name DefaultShell -Value "C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe" -PropertyType String -Force
```

---

## ✅ Verificación desde tu PC Local

Ejecuta esto desde tu **PC de desarrollo** para verificar que SSH funciona:

```powershell
# REEMPLAZA <USUARIO> y <IP_SERVIDOR>
ssh <USUARIO>@<IP_SERVIDOR> "echo OK"

# Si ves "OK", la configuración SSH está lista
```

---

## 📋 Información a Proporcionar

Una vez completados todos los pasos, proporciona:

```yaml
IP_SERVIDOR: <ejemplo: 192.168.1.XXX>
USUARIO_SSH: <ejemplo: equipo2>
PASSWORD: <ejemplo: ocfb>
NOMBRE_GRUPO_ADMIN: <ejemplo: Administradores o Administrators>
```

**Con esta información, yo me encargaré de:**
- ✅ Crear carpetas remotamente via SSH
- ✅ Configurar share SMB
- ✅ Instalar la aplicación
- ✅ Configurar el servicio Windows (NSSM)
- ✅ Crear certificados SSL
- ✅ Crear script de deploy personalizado

---

## 🔧 Troubleshooting

### Error: "Add-WindowsCapability" no funciona
```powershell
# Alternativa: Usar Settings de Windows
# Settings > Apps > Optional Features > Add a feature > OpenSSH Server
```

### Error: "Access Denied" al conectar por SSH
```powershell
# Verificar membresía del grupo
Get-LocalGroupMember -Group "<NOMBRE_GRUPO_ADMIN>"
```

### Error: SSH conecta pero cierra inmediatamente
```powershell
# Reiniciar servicio SSH
Restart-Service sshd
```
