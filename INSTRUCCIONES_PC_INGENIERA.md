# 🖥️ INSTRUCCIONES - PC INGENIERA (Aprobador)

## Información de acceso
- **Usuario del sistema:** `ingeniera`
- **Contraseña:** `ingeniera123` (⚠️ CAMBIAR DESPUÉS DEL PRIMER USO)
- **Rol:** Aprobador
- **Permisos:** Aprobar/rechazar permisos, consultar información

---

## 📋 Información del Servidor

Para conectarte al sistema, necesitas saber esta información del PC servidor:

- **Nombre del PC Servidor:** `ELITEBOOK-EVERT`
- **Dirección IP del Servidor:** `192.168.1.76`
- **Carpeta compartida:** `\\ELITEBOOK-EVERT\SGRRHH` o `\\192.168.1.76\SGRRHH`

---

## 🔧 PASO 1: Verificar acceso a la carpeta compartida

Antes de instalar, verifica que puedes acceder a la carpeta del servidor:

1. Abre el **Explorador de archivos** (Windows + E)
2. En la barra de direcciones, escribe:
   ```
   \\ELITEBOOK-EVERT\SGRRHH
   ```
   O usando la IP:
   ```
   \\192.168.1.76\SGRRHH
   ```
3. Deberías ver las carpetas: `backups`, `config`, `documentos`, `fotos`, `logs`

### ❌ Si NO puedes acceder:

**Opción A - Agregar credenciales de red:**
1. En el Explorador, ve a **Este equipo**
2. Clic en **Conectar a unidad de red**
3. Carpeta: `\\ELITEBOOK-EVERT\SGRRHH`
4. Marca: **Conectar con credenciales diferentes**
5. Ingresa las credenciales del PC servidor (si te las pidieron)

**Opción B - Verificar conectividad:**
1. Abre **Símbolo del sistema** (CMD)
2. Ejecuta: `ping ELITEBOOK-EVERT` o `ping 192.168.1.76`
3. Si no responde, verifica que:
   - Ambas PCs están en la misma red WiFi
   - El PC servidor está encendido
   - El Firewall permite compartir archivos

---

## 📝 PASO 2: Instalar SGRRHH

### Opción A - Instalador (Recomendado):
1. Copia `SGRRHH_Setup_1.0.0.exe` desde el servidor o USB
2. Ejecuta el instalador
3. Sigue el asistente de instalación
4. Instala en `C:\Program Files\SGRRHH` (ubicación predeterminada)

### Opción B - Versión Portable:
1. Copia `SGRRHH_Portable_1.0.0.zip` desde el servidor o USB
2. Extrae en `C:\SGRRHH` (o donde prefieras)

---

## 📝 PASO 3: Configurar appsettings.json

Después de instalar, necesitas configurar la conexión al servidor.

**Ubicación del archivo:**
- Si usaste el instalador: `C:\Program Files\SGRRHH\appsettings.json`
- Si usaste la versión portable: `[Carpeta donde extraíste]\appsettings.json`

**Contenido del archivo:**

```json
{
  "Database": {
    "Path": "\\\\ELITEBOOK-EVERT\\SGRRHH\\sgrrhh.db",
    "EnableWalMode": true,
    "BusyTimeout": 30000
  },
  "Network": {
    "IsNetworkMode": true,
    "SharedFolder": "\\\\ELITEBOOK-EVERT\\SGRRHH"
  },
  "Application": {
    "Name": "SGRRHH",
    "Version": "1.0.0",
    "Company": "Mi Empresa"
  }
}
```

**⚠️ IMPORTANTE:**
- Usa **4 barras invertidas** (`\\\\`) en JSON, no 2
- Si prefieres usar la IP en lugar del nombre, reemplaza:
  - `ELITEBOOK-EVERT` por `192.168.1.76`

**Ejemplo usando IP:**
```json
{
  "Database": {
    "Path": "\\\\192.168.1.76\\SGRRHH\\sgrrhh.db",
    "EnableWalMode": true,
    "BusyTimeout": 30000
  },
  "Network": {
    "IsNetworkMode": true,
    "SharedFolder": "\\\\192.168.1.76\\SGRRHH"
  }
}
```

### Editar el archivo:
1. Abre **Bloc de notas** como Administrador
2. Abre el archivo `appsettings.json`
3. Copia el contenido de arriba (eligiendo nombre o IP)
4. Guarda el archivo (Ctrl + S)

---

## 🚀 PASO 4: Ejecutar SGRRHH

1. Ejecuta `SGRRHH.exe` (desde el menú inicio o la carpeta de instalación)
2. Deberías ver la ventana de **Login**

### Primera ejecución:
Si la base de datos aún no existe, se creará automáticamente en el servidor (`\\ELITEBOOK-EVERT\SGRRHH\sgrrhh.db`)

---

## 🔐 PASO 5: Iniciar sesión

En la ventana de Login:

- **Usuario:** `ingeniera`
- **Contraseña:** `ingeniera123`

Haz clic en **Iniciar Sesión**

---

## 🎯 PASO 6: Funciones principales como Aprobador

Una vez dentro del sistema, como **Aprobador (Ingeniera)** puedes:

### ✅ Lo que SÍ puedes hacer:

1. **Bandeja de Aprobación** (menú lateral):
   - Ver permisos pendientes de aprobación
   - Aprobar o rechazar solicitudes de permisos
   - Agregar observaciones

2. **Consultar información**:
   - Ver lista de empleados
   - Ver registros diarios
   - Ver permisos (todos)
   - Ver vacaciones
   - Ver contratos

3. **Generar reportes**:
   - Reportes de empleados
   - Reportes de actividades
   - Documentos PDF (certificados, constancias)

### ❌ Lo que NO puedes hacer:
- Crear o editar empleados
- Crear nuevos registros diarios
- Solicitar permisos (solo aprobarlos)
- Modificar catálogos
- Acceder a configuración del sistema
- Gestionar usuarios

---

## 🔒 PASO 7: Cambiar tu contraseña (RECOMENDADO)

Para mayor seguridad:

1. En el menú superior derecho, haz clic en tu nombre de usuario
2. Selecciona **Cambiar contraseña**
3. Ingresa:
   - Contraseña actual: `ingeniera123`
   - Nueva contraseña: [tu contraseña segura]
   - Confirmar contraseña: [repetir la nueva contraseña]
4. Haz clic en **Guardar**

---

## 🧪 PASO 8: Probar funcionalidades

### Prueba 1 - Aprobar un permiso:
1. Pídele a la secretaria que cree una solicitud de permiso
2. En tu PC, ve a **Bandeja de Aprobación**
3. Deberías ver la solicitud pendiente
4. Selecciónala y haz clic en **Aprobar** o **Rechazar**
5. Agrega una observación (opcional)
6. Confirma la acción

### Prueba 2 - Generar un documento:
1. Ve a **Documentos** (menú lateral)
2. Selecciona un tipo de documento (ej: Certificado Laboral)
3. Selecciona un empleado
4. Haz clic en **Generar**
5. Previsualiza el PDF
6. Descárgalo o imprímelo

---

## 📞 Solución de problemas

### "No se puede conectar a la base de datos"
- Verifica que el PC servidor está encendido
- Verifica que puedes acceder a `\\ELITEBOOK-EVERT\SGRRHH` desde el Explorador
- Verifica el archivo `appsettings.json` (rutas correctas, 4 barras invertidas)
- Intenta usar la IP en lugar del nombre del PC

### "La base de datos está bloqueada"
- Espera unos segundos e intenta de nuevo
- Es normal si otro usuario está guardando cambios
- Si persiste, pídele al administrador que revise los logs

### "Credenciales inválidas"
- Verifica que usas:
  - Usuario: `ingeniera` (todo en minúsculas)
  - Contraseña: `ingeniera123`
- Si cambiaste la contraseña y la olvidaste, pídele al administrador que la restablezca

### La aplicación está lenta
- Verifica tu conexión WiFi
- Si es posible, usa cable de red en lugar de WiFi
- Pídele al administrador que verifique la configuración del servidor

### No aparece la opción "Bandeja de Aprobación"
- Verifica que iniciaste sesión con el usuario `ingeniera`
- Esta opción solo está disponible para el rol **Aprobador**

---

## ℹ️ Información adicional

### Red WiFi:
Este sistema funciona en red WiFi local. Para mejor rendimiento:
- Mantente cerca del router WiFi
- Evita descargar archivos grandes mientras usas el sistema
- Si experimentas lentitud, considera usar cable de red

### Backups:
- Los backups los maneja el administrador
- NO intentes hacer backups desde tu PC
- Los datos están centralizados en el servidor

### Soporte:
Si tienes problemas técnicos:
1. Revisa esta guía primero
2. Verifica tu conexión de red
3. Contacta al administrador del sistema
4. El administrador puede revisar los logs en `\\ELITEBOOK-EVERT\SGRRHH\logs\`

---

**¡Listo!** Ya puedes empezar a usar SGRRHH como Aprobador.
