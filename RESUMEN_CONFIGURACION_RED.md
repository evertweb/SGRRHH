# 📋 RESUMEN - Configuración de Red SGRRHH

## ✅ Trabajo Completado Automáticamente

He ejecutado los siguientes pasos en tu PC:

- ✅ Creadas carpetas del sistema en `C:\SGRRHH_Data\`:
  - `C:\SGRRHH_Data\fotos`
  - `C:\SGRRHH_Data\documentos`
  - `C:\SGRRHH_Data\backups`
  - `C:\SGRRHH_Data\config`
  - `C:\SGRRHH_Data\logs`

- ✅ Obtenida información de red:
  - Nombre del PC: `ELITEBOOK-EVERT`
  - IP WiFi: `192.168.1.76`

- ✅ Creados archivos de configuración de ejemplo:
  - `appsettings_SERVIDOR.json` - Para TU PC (servidor)
  - `appsettings_CLIENTES.json` - Para PCs de ingeniera y secretaria (usa nombre del PC)
  - `appsettings_CLIENTES_IP.json` - Alternativa usando IP

- ✅ Creadas guías detalladas:
  - `INSTRUCCIONES_PC_SERVIDOR.md` - Para ti (administrador)
  - `INSTRUCCIONES_PC_INGENIERA.md` - Para la ingeniera
  - `INSTRUCCIONES_PC_SECRETARIA.md` - Para la secretaria

---

## ⚠️ SIGUIENTE PASO IMPORTANTE (DEBES HACERLO TÚ)

### 🔴 PASO CRÍTICO: Compartir la carpeta en red

**No puedo hacer esto automáticamente, debes hacerlo manualmente:**

### Método 1 - PowerShell como Administrador (MÁS RÁPIDO):

1. Presiona `Windows + X` → Selecciona **"Windows PowerShell (Administrador)"** o **"Terminal (Administrador)"**
2. Copia y pega este comando:
   ```powershell
   New-SmbShare -Name "SGRRHH" -Path "C:\SGRRHH_Data" -FullAccess "Todos"
   ```
3. Presiona Enter
4. Verifica que funcionó con:
   ```powershell
   Get-SmbShare -Name "SGRRHH"
   ```

### Método 2 - Explorador de Windows (MÁS VISUAL):

1. Abre el **Explorador de archivos** (Windows + E)
2. Ve a `C:\SGRRHH_Data`
3. **Clic derecho** → **Propiedades**
4. Pestaña **Compartir** → **Uso compartido avanzado...**
5. Marca **"Compartir esta carpeta"**
6. Nombre: `SGRRHH`
7. Clic en **Permisos** → Marca **"Control total"** para **"Todos"**
8. **Aplicar** → **Aceptar**

---

## 📁 Archivos Creados

En la carpeta `C:\Users\evert\Documents\rrhh\` encontrarás:

| Archivo | Para quién | Descripción |
|---------|------------|-------------|
| `INSTRUCCIONES_PC_SERVIDOR.md` | **Para TI** | Pasos completos para tu PC |
| `INSTRUCCIONES_PC_INGENIERA.md` | **Para la ingeniera** | Guía completa para su PC |
| `INSTRUCCIONES_PC_SECRETARIA.md` | **Para la secretaria** | Guía completa para su PC |
| `appsettings_SERVIDOR.json` | **Para TI** | Configuración para tu PC |
| `appsettings_CLIENTES.json` | **Para ingeniera y secretaria** | Configuración usando nombre del PC |
| `appsettings_CLIENTES_IP.json` | **Alternativa** | Configuración usando IP |

---

## 🚀 Plan de Implementación Recomendado

### Día 1 - Configurar tu PC (Servidor)

1. ✅ **HECHO**: Carpetas creadas
2. ⚠️ **PENDIENTE**: Compartir carpeta `C:\SGRRHH_Data` (ver arriba)
3. **Instalar SGRRHH** en tu PC:
   - Usa `installer\output\SGRRHH_Portable_1.0.0.zip` o el instalador si lo tienes
4. **Copiar** el archivo `appsettings_SERVIDOR.json` como `appsettings.json` junto a `SGRRHH.exe`
5. **Ejecutar** SGRRHH y hacer login con:
   - Usuario: `admin`
   - Contraseña: `admin123`
6. **Probar** que funciona:
   - Ver el Dashboard
   - Crear un empleado de prueba
   - Ir a Configuración → Backup → Crear backup
   - Verificar que el backup se guardó en `C:\SGRRHH_Data\backups\`
7. **Cambiar** la contraseña del admin

### Día 2 - Configurar PC de la Ingeniera

1. **Llevar** el instalador o ZIP portable a su PC (USB o red)
2. **Darle** el archivo `INSTRUCCIONES_PC_INGENIERA.md`
3. **Ella debe** seguir la guía paso a paso
4. **Verificar** que puede:
   - Acceder a `\\ELITEBOOK-EVERT\SGRRHH` desde el Explorador
   - Iniciar sesión en SGRRHH con usuario `ingeniera`
   - Ver la "Bandeja de Aprobación"

### Día 3 - Configurar PC de la Secretaria

1. **Llevar** el instalador o ZIP portable a su PC (USB o red)
2. **Darle** el archivo `INSTRUCCIONES_PC_SECRETARIA.md`
3. **Ella debe** seguir la guía paso a paso
4. **Verificar** que puede:
   - Acceder a `\\ELITEBOOK-EVERT\SGRRHH` desde el Explorador
   - Iniciar sesión en SGRRHH con usuario `secretaria`
   - Crear un empleado de prueba
   - Solicitar un permiso de prueba

### Día 4 - Pruebas de integración

1. **Secretaria** crea una solicitud de permiso
2. **Ingeniera** la ve en su bandeja y la aprueba
3. **Tú** verificas que todo se guardó correctamente
4. **Todos** cambian sus contraseñas predeterminadas

---

## 🔍 Verificación Rápida

### Desde tu PC (Servidor):

```powershell
# Verificar que la carpeta está compartida
Get-SmbShare -Name "SGRRHH"

# Debería mostrar:
# Name    ScopeName Path              Description
# ----    --------- ----              -----------
# SGRRHH  *         C:\SGRRHH_Data
```

### Desde las otras PCs (Ingeniera y Secretaria):

1. Abre el Explorador de archivos
2. Escribe en la barra de direcciones:
   ```
   \\ELITEBOOK-EVERT\SGRRHH
   ```
   O con IP:
   ```
   \\192.168.1.76\SGRRHH
   ```
3. Deberías ver las carpetas: `backups`, `config`, `documentos`, `fotos`, `logs`

---

## 🛡️ Firewall - Si hay problemas de conexión

Si las otras PCs no pueden acceder, configura el Firewall:

### En tu PC (Servidor):

1. Abre **Panel de Control** → **Sistema y seguridad** → **Firewall de Windows Defender**
2. Clic en **Permitir una aplicación a través de Firewall**
3. Busca **"Compartir archivos e impresoras"**
4. Marca las casillas para **Privado** (al menos)
5. **Aceptar**

O ejecuta en PowerShell (como Administrador):
```powershell
# Habilitar compartir archivos en el Firewall
Set-NetFirewallRule -DisplayGroup "Compartir archivos e impresoras" -Enabled True -Profile Private
```

---

## 📊 Información de Red

| Elemento | Valor |
|----------|-------|
| **Nombre del PC Servidor** | `ELITEBOOK-EVERT` |
| **IP del Servidor (WiFi)** | `192.168.1.76` |
| **Carpeta compartida (nombre)** | `\\ELITEBOOK-EVERT\SGRRHH` |
| **Carpeta compartida (IP)** | `\\192.168.1.76\SGRRHH` |
| **Carpeta local en servidor** | `C:\SGRRHH_Data` |
| **Tipo de red** | WiFi local |

---

## 👥 Usuarios del Sistema

| Usuario | Contraseña | Rol | PC |
|---------|------------|-----|-----|
| admin | admin123 | Administrador | Tu PC (servidor) |
| ingeniera | ingeniera123 | Aprobador | PC de la ingeniera |
| secretaria | secretaria123 | Operador | PC de la secretaria |

⚠️ **IMPORTANTE**: Todos deben cambiar sus contraseñas después del primer inicio de sesión.

---

## 📞 Solución de Problemas Comunes

### "No se puede compartir la carpeta"
- Ejecuta PowerShell **como Administrador**
- Verifica que la carpeta `C:\SGRRHH_Data` existe

### "Otras PCs no pueden acceder"
- Verifica que compartiste la carpeta (Paso crítico arriba)
- Verifica el Firewall (ver sección Firewall arriba)
- Verifica que todas las PCs están en la misma red WiFi
- Intenta hacer ping: `ping ELITEBOOK-EVERT` o `ping 192.168.1.76`

### "La base de datos está bloqueada"
- Normal si varios usuarios guardan al mismo tiempo
- Espera unos segundos
- Si persiste, aumenta `BusyTimeout` en `appsettings.json`

### "Rendimiento lento"
- Usa cable de red en lugar de WiFi si es posible
- Acércate al router WiFi
- Verifica que el modo WAL está habilitado (`EnableWalMode: true`)

---

## ✅ Lista de Verificación Final

**En tu PC (Servidor):**
- [ ] Carpetas creadas en `C:\SGRRHH_Data` ✅ (YA HECHO)
- [ ] Carpeta compartida como `SGRRHH` ⚠️ (PENDIENTE)
- [ ] Firewall configurado para permitir compartir archivos
- [ ] SGRRHH instalado
- [ ] Archivo `appsettings.json` configurado (usar `appsettings_SERVIDOR.json`)
- [ ] Primera ejecución exitosa
- [ ] Login con usuario `admin` funciona
- [ ] Contraseña del admin cambiada

**En PC de la Ingeniera:**
- [ ] SGRRHH instalado
- [ ] Archivo `appsettings.json` configurado (usar `appsettings_CLIENTES.json` o `_IP.json`)
- [ ] Puede acceder a `\\ELITEBOOK-EVERT\SGRRHH`
- [ ] Login con usuario `ingeniera` funciona
- [ ] Puede ver "Bandeja de Aprobación"
- [ ] Contraseña cambiada

**En PC de la Secretaria:**
- [ ] SGRRHH instalado
- [ ] Archivo `appsettings.json` configurado (usar `appsettings_CLIENTES.json` o `_IP.json`)
- [ ] Puede acceder a `\\ELITEBOOK-EVERT\SGRRHH`
- [ ] Login con usuario `secretaria` funciona
- [ ] Puede crear empleados
- [ ] Puede solicitar permisos
- [ ] Contraseña cambiada

**Pruebas de integración:**
- [ ] Secretaria crea solicitud de permiso
- [ ] Ingeniera ve la solicitud en su bandeja
- [ ] Ingeniera aprueba la solicitud
- [ ] El sistema genera el PDF del acta de permiso
- [ ] Todos pueden trabajar simultáneamente

---

## 📚 Recursos Adicionales

- **Guía de red completa:** `docs\07_CONFIGURACION_RED.md`
- **Guía de instalación:** `installer\README_INSTALACION.md`
- **Documentación del proyecto:** `docs\00_CONTEXTO_IA.md`

---

## 🎯 Próximos Pasos

1. **AHORA**: Comparte la carpeta `C:\SGRRHH_Data` (Paso crítico arriba)
2. **HOY**: Instala SGRRHH en tu PC y prueba que funciona
3. **MAÑANA**: Configura PC de la ingeniera
4. **PASADO MAÑANA**: Configura PC de la secretaria
5. **DÍA 4**: Pruebas de integración con todos los usuarios

---

**¿Necesitas ayuda?**

Si tienes problemas en algún paso:
1. Revisa la guía correspondiente (`INSTRUCCIONES_PC_*.md`)
2. Verifica la conexión de red
3. Revisa los logs en `C:\SGRRHH_Data\logs\`
4. Contáctame y te ayudo a resolver el problema

---

**¡Éxito con la instalación!** 🎉
