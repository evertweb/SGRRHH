# Agregar Llave SSH Pública al Servidor

## Tu Llave Pública SSH
```
ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIHFmmlCze8n99Uu7z2CULXOiVAnrhrK7pX7QFeVmwtRT evert@ELITEBOOK-EVERT
```

---

## Opción 1: Automático con ssh-copy-id (Recomendado)

Si tienes `ssh-copy-id` disponible (Git Bash en Windows):

```bash
ssh-copy-id -i ~/.ssh/id_ed25519.pub <USUARIO>@<IP_SERVIDOR>
```

---

## Opción 2: Manual - Ejecutar en el Servidor Remoto

**Importante:** Como el usuario es **administrador**, Windows requiere una configuración especial.

### Paso 1: Crear archivo administrators_authorized_keys

```powershell
# Ejecutar en el SERVIDOR como Administrador

# Crear directorio ssh si no existe
New-Item -Path "C:\ProgramData\ssh" -ItemType Directory -Force

# Crear archivo administrators_authorized_keys
$publicKey = "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIHFmmlCze8n99Uu7z2CULXOiVAnrhrK7pX7QFeVmwtRT evert@ELITEBOOK-EVERT"
Set-Content -Path "C:\ProgramData\ssh\administrators_authorized_keys" -Value $publicKey

# Verificar contenido
Get-Content "C:\ProgramData\ssh\administrators_authorized_keys"
```

### Paso 2: Configurar Permisos Correctos

```powershell
# Ejecutar en el SERVIDOR como Administrador

# El archivo debe ser propiedad de SYSTEM y Administrators
icacls "C:\ProgramData\ssh\administrators_authorized_keys" /inheritance:r
icacls "C:\ProgramData\ssh\administrators_authorized_keys" /grant "SYSTEM:(F)"
icacls "C:\ProgramData\ssh\administrators_authorized_keys" /grant "Administrators:(F)"

# Verificar permisos
icacls "C:\ProgramData\ssh\administrators_authorized_keys"
```

### Paso 3: Reiniciar Servicio SSH

```powershell
# Ejecutar en el SERVIDOR como Administrador
Restart-Service sshd
```

---

## Opción 3: Desde tu PC (Más Fácil)

Ejecuta esto desde **tu PC local** (PowerShell):

```powershell
# REEMPLAZA <USUARIO> e <IP_SERVIDOR>
$serverUser = "<USUARIO>"
$serverIp = "<IP_SERVIDOR>"
$publicKey = "ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIHFmmlCze8n99Uu7z2CULXOiVAnrhrK7pX7QFeVmwtRT evert@ELITEBOOK-EVERT"

# Comando completo que se ejecutará remotamente
$remoteCommand = @"
New-Item -Path 'C:\ProgramData\ssh' -ItemType Directory -Force | Out-Null
Set-Content -Path 'C:\ProgramData\ssh\administrators_authorized_keys' -Value '$publicKey'
icacls 'C:\ProgramData\ssh\administrators_authorized_keys' /inheritance:r
icacls 'C:\ProgramData\ssh\administrators_authorized_keys' /grant 'SYSTEM:(F)'
icacls 'C:\ProgramData\ssh\administrators_authorized_keys' /grant 'Administrators:(F)'
Restart-Service sshd
Write-Host 'Llave SSH configurada correctamente'
"@

# Ejecutar en el servidor remoto
ssh "$serverUser@$serverIp" $remoteCommand
```

---

## ✅ Verificación

Después de configurar, prueba la conexión **sin contraseña**:

```powershell
ssh <USUARIO>@<IP_SERVIDOR> "echo 'SSH sin password funciona!'"
```

Si pide contraseña todavía:

1. **Verifica la ruta del archivo:**
   ```powershell
   ssh <USUARIO>@<IP_SERVIDOR> "Get-Content C:\ProgramData\ssh\administrators_authorized_keys"
   ```

2. **Verifica permisos:**
   ```powershell
   ssh <USUARIO>@<IP_SERVIDOR> "icacls C:\ProgramData\ssh\administrators_authorized_keys"
   ```

3. **Ver logs de SSH:**
   ```powershell
   ssh <USUARIO>@<IP_SERVIDOR> "Get-Content C:\ProgramData\ssh\logs\sshd.log -Tail 50"
   ```

---

## Notas Importantes

- **Para usuarios NO administradores:** Usa `C:\Users\<usuario>\.ssh\authorized_keys` en vez de `administrators_authorized_keys`
- **Múltiples llaves:** Puedes agregar múltiples llaves, una por línea
- **Seguridad:** Asegúrate que los permisos sean correctos, SSH es muy estricto con esto
