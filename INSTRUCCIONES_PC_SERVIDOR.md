# 🖥️ INSTRUCCIONES - PC SERVIDOR (Administrador)

## Información del Servidor
- **Nombre del PC:** `ELITEBOOK-EVERT`
- **Dirección IP WiFi:** `192.168.1.76`
- **Usuario del sistema:** `admin`
- **Contraseña:** `admin123` (⚠️ CAMBIAR DESPUÉS DEL PRIMER USO)

---

## ✅ Pasos ya completados

- [x] Carpetas creadas en `C:\SGRRHH_Data`
- [ ] Carpeta compartida en red (PENDIENTE - ver abajo)

---

## 🔧 PASO 1: Compartir la carpeta (DEBES HACERLO MANUALMENTE)

### Método recomendado - Explorador de Windows:

1. Abre el **Explorador de archivos** (Windows + E)
2. Ve a `C:\SGRRHH_Data`
3. **Clic derecho** en la carpeta → **Propiedades**
4. Ve a la pestaña **Compartir**
5. Haz clic en **Uso compartido avanzado...**
6. Marca **Compartir esta carpeta**
7. Nombre del recurso: `SGRRHH` (debe ser exactamente este nombre)
8. Haz clic en **Permisos**
9. Selecciona **Todos** y marca:
   - ✅ Control total
   - ✅ Cambiar
   - ✅ Leer
10. **Aplicar** → **Aceptar**

### Verificar que funciona:

Abre **PowerShell** y ejecuta:
```powershell
Get-SmbShare -Name "SGRRHH"
```

Deberías ver información de la carpeta compartida.

---

## 📝 PASO 2: Configurar el archivo appsettings.json

Cuando instales SGRRHH en tu PC, el archivo `appsettings.json` debe estar junto al ejecutable `SGRRHH.exe`.

**Contenido del archivo para TU PC (servidor):**

```json
{
  "Database": {
    "Path": "C:\\SGRRHH_Data\\sgrrhh.db",
    "EnableWalMode": true,
    "BusyTimeout": 30000
  },
  "Network": {
    "IsNetworkMode": true,
    "SharedFolder": "C:\\SGRRHH_Data"
  },
  "Application": {
    "Name": "SGRRHH",
    "Version": "1.0.0",
    "Company": "Mi Empresa"
  }
}
```

**Nota:** En tu PC usamos la ruta local `C:\SGRRHH_Data` porque TÚ eres el servidor.

---

## 🚀 PASO 3: Instalar SGRRHH

1. Ejecuta el instalador: `SGRRHH_Setup_1.0.0.exe` (si lo tienes)
2. O usa la versión portable desde: `installer\output\SGRRHH_Portable_1.0.0.zip`
3. Si usas la portable:
   - Extrae en `C:\Program Files\SGRRHH`
   - Copia el archivo `appsettings.json` (del paso 2) junto a `SGRRHH.exe`

---

## 🧪 PASO 4: Probar que funciona

1. Ejecuta `SGRRHH.exe`
2. Inicia sesión con:
   - **Usuario:** `admin`
   - **Contraseña:** `admin123`
3. Deberías ver el Dashboard
4. Ve a **Configuración** → **Backup** y crea un backup de prueba
5. Verifica que se creó en `C:\SGRRHH_Data\backups\`

---

## 🔒 PASO 5: Cambiar contraseñas (IMPORTANTE)

Una vez que todo funcione:

1. En SGRRHH, ve a **Configuración** → **Usuarios**
2. Edita tu usuario `admin`
3. Haz clic en **Cambiar Contraseña**
4. Pon una contraseña segura

---

## 📡 PASO 6: Verificar acceso desde otra PC

Desde otra PC en la misma red WiFi:

1. Abre el **Explorador de archivos**
2. En la barra de direcciones, escribe:
   ```
   \\ELITEBOOK-EVERT\SGRRHH
   ```
   O usando la IP:
   ```
   \\192.168.1.76\SGRRHH
   ```
3. Deberías ver las carpetas: `backups`, `config`, `documentos`, `fotos`, `logs`
4. Si NO puedes acceder, revisa:
   - Que compartiste correctamente la carpeta
   - Que el Firewall de Windows permite compartir archivos
   - Que ambas PCs están en la misma red WiFi

---

## 🛡️ Firewall - Permitir compartir archivos

Si tienes problemas de acceso desde otras PCs:

1. Abre **Panel de Control** → **Sistema y seguridad** → **Firewall de Windows Defender**
2. En el panel izquierdo: **Permitir una aplicación a través de Firewall**
3. Busca **Compartir archivos e impresoras**
4. Marca las casillas para **Privado** y **Público** (o solo Privado si tu red WiFi es privada)
5. Haz clic en **Aceptar**

---

## 📞 Solución de problemas

### La base de datos no se crea
- Verifica que la carpeta `C:\SGRRHH_Data` existe
- Verifica que tienes permisos de escritura en esa carpeta

### Otras PCs no pueden acceder
- Verifica que ejecutaste el PASO 1 correctamente
- Verifica el Firewall (ver arriba)
- Prueba hacer ping desde la otra PC: `ping ELITEBOOK-EVERT` o `ping 192.168.1.76`

---

**¡Listo!** Tu PC está configurado como servidor. Ahora configura las PCs de la ingeniera y secretaria siguiendo sus respectivos archivos de instrucciones.
