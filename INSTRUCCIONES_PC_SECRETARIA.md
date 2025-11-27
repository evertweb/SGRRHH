# 🖥️ INSTRUCCIONES - PC SECRETARIA (Operador)

## Información de acceso
- **Usuario del sistema:** `secretaria`
- **Contraseña:** `secretaria123` (⚠️ CAMBIAR DESPUÉS DEL PRIMER USO)
- **Rol:** Operador
- **Permisos:** Gestión completa de empleados, registros diarios, solicitud de permisos

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

- **Usuario:** `secretaria`
- **Contraseña:** `secretaria123`

Haz clic en **Iniciar Sesión**

---

## 🎯 PASO 6: Funciones principales como Operador

Una vez dentro del sistema, como **Operador (Secretaria)** puedes:

### ✅ Lo que SÍ puedes hacer:

1. **Gestión de Empleados** (tu función principal):
   - ➕ Crear nuevos empleados
   - ✏️ Editar información de empleados
   - 👁️ Ver detalles de empleados
   - 📸 Subir fotos de empleados
   - 📄 Ver contratos

2. **Control Diario**:
   - 📅 Registrar entrada y salida diaria de empleados
   - 📝 Registrar actividades realizadas cada día
   - 🕐 Llevar control de horas trabajadas
   - 📊 Asociar actividades a proyectos

3. **Permisos**:
   - 📋 Solicitar permisos para empleados
   - 📎 Adjuntar documentos de soporte
   - 👀 Ver el estado de permisos solicitados
   - ❌ **NO puedes aprobar permisos** (solo la ingeniera)

4. **Consultar**:
   - Ver catálogos (Departamentos, Cargos, Actividades, Proyectos)
   - Ver vacaciones de empleados
   - Ver tipos de permiso disponibles

5. **Reportes básicos**:
   - Lista de empleados
   - Actividades por empleado
   - Resumen de horas

### ❌ Lo que NO puedes hacer:
- Aprobar o rechazar permisos (solo la ingeniera puede)
- Modificar catálogos del sistema
- Acceder a configuración del sistema
- Gestionar usuarios
- Crear backups

---

## 📚 PASO 7: Tutorial de uso - Tareas comunes

### 🆕 Crear un nuevo empleado:

1. En el menú lateral, haz clic en **Empleados**
2. Haz clic en el botón **+ Nuevo Empleado** (arriba a la derecha)
3. Completa el formulario:
   - **Información Personal**: Cédula, nombres, apellidos, fecha de nacimiento
   - **Contacto**: Dirección, teléfono, email
   - **Laboral**: Departamento, cargo, fecha de ingreso
   - **Foto**: Haz clic en "Seleccionar foto" (opcional)
4. Haz clic en **Guardar**

### 📅 Registrar actividades del día:

1. En el menú lateral, haz clic en **Control Diario**
2. Selecciona la **fecha** (por defecto es hoy)
3. Selecciona el **empleado**
4. Ingresa **hora de entrada** y **hora de salida**
5. En la sección "Actividades del día":
   - Selecciona una **actividad**
   - Selecciona un **proyecto** (si aplica)
   - Ingresa las **horas dedicadas**
   - Agrega **observaciones**
   - Haz clic en **+ Agregar Actividad**
6. Puedes agregar múltiples actividades
7. Haz clic en **Guardar Registro**

### 📋 Solicitar un permiso:

1. En el menú lateral, haz clic en **Permisos**
2. Haz clic en **+ Nueva Solicitud**
3. Completa el formulario:
   - **Empleado**: Selecciona para quién es el permiso
   - **Tipo de permiso**: Selecciona de la lista (Calamidad, Médico, etc.)
   - **Motivo**: Describe el motivo del permiso
   - **Fechas**: Fecha de inicio y fin del permiso
   - **Horario** (si aplica): Hora de salida y regreso
   - **Documento**: Adjunta un documento de soporte (PDF, imagen)
4. Haz clic en **Guardar Solicitud**
5. El permiso quedará en estado **Pendiente**
6. La ingeniera recibirá la solicitud en su bandeja de aprobación

### 🔍 Buscar un empleado:

1. Ve a **Empleados**
2. Usa el **cuadro de búsqueda** (arriba)
3. Puedes buscar por:
   - Nombre
   - Apellido
   - Cédula
   - Departamento
   - Cargo
4. Haz clic en un empleado para ver sus detalles

### ✏️ Editar un empleado:

1. Ve a **Empleados**
2. Busca el empleado
3. Haz clic en el botón **✏️ Editar** (al lado del empleado)
4. Modifica la información necesaria
5. Haz clic en **Guardar Cambios**

---

## 🔒 PASO 8: Cambiar tu contraseña (RECOMENDADO)

Para mayor seguridad:

1. En el menú superior derecho, haz clic en tu nombre de usuario
2. Selecciona **Cambiar contraseña**
3. Ingresa:
   - Contraseña actual: `secretaria123`
   - Nueva contraseña: [tu contraseña segura]
   - Confirmar contraseña: [repetir la nueva contraseña]
4. Haz clic en **Guardar**

---

## 🧪 PASO 9: Pruebas recomendadas

### Prueba 1 - Crear un empleado de prueba:
1. Ve a **Empleados** → **+ Nuevo Empleado**
2. Crea un empleado ficticio con datos de prueba
3. Sube una foto (puede ser cualquier imagen)
4. Guarda y verifica que aparece en la lista

### Prueba 2 - Registrar actividad del día:
1. Ve a **Control Diario**
2. Crea un registro para el empleado de prueba
3. Agrega 2-3 actividades diferentes
4. Guarda y verifica que se guardó correctamente

### Prueba 3 - Solicitar un permiso:
1. Ve a **Permisos** → **+ Nueva Solicitud**
2. Crea una solicitud de permiso médico para el empleado de prueba
3. Guarda y verifica que aparece con estado "Pendiente"
4. Pídele a la ingeniera que lo apruebe desde su PC

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
  - Usuario: `secretaria` (todo en minúsculas)
  - Contraseña: `secretaria123`
- Si cambiaste la contraseña y la olvidaste, pídele al administrador que la restablezca

### "No puedo subir una foto de empleado"
- Verifica que la imagen es JPG o PNG
- Verifica que el tamaño no sea muy grande (máximo 5 MB recomendado)
- Verifica que tienes permisos de escritura en la carpeta compartida

### La aplicación está lenta
- Verifica tu conexión WiFi
- Si es posible, usa cable de red en lugar de WiFi
- Pídele al administrador que verifique la configuración del servidor

### "No puedo aprobar permisos"
- Eso es correcto, como **Operador** solo puedes SOLICITAR permisos
- Solo la ingeniera (Aprobador) puede aprobarlos
- Puedes ver el estado de tus solicitudes en **Permisos**

---

## 💡 Consejos y buenas prácticas

### Al crear empleados:
- ✅ Siempre completa todos los campos obligatorios
- ✅ Verifica bien la cédula (sin puntos ni espacios)
- ✅ Sube una foto de buena calidad
- ✅ Asigna el departamento y cargo correcto

### Al registrar actividades:
- ✅ Hazlo todos los días al final de la jornada
- ✅ Sé específica en las observaciones
- ✅ Asegúrate de que las horas sumen correctamente
- ✅ Asocia las actividades al proyecto correspondiente

### Al solicitar permisos:
- ✅ Selecciona el tipo de permiso correcto
- ✅ Describe claramente el motivo
- ✅ Adjunta documentos de soporte cuando sea necesario
- ✅ Verifica las fechas antes de guardar

### Para mejor rendimiento:
- 🚀 Mantén la aplicación abierta durante tu jornada laboral
- 🚀 Cierra la aplicación al terminar el día
- 🚀 No abras múltiples instancias de la aplicación
- 🚀 Guarda tus cambios regularmente

---

## ℹ️ Información adicional

### Red WiFi:
Este sistema funciona en red WiFi local. Para mejor rendimiento:
- Mantente cerca del router WiFi
- Evita descargar archivos grandes mientras usas el sistema
- Si experimentas lentitud, considera usar cable de red

### Documentos adjuntos:
- Los documentos de permisos se guardan en: `\\ELITEBOOK-EVERT\SGRRHH\documentos\`
- Las fotos de empleados se guardan en: `\\ELITEBOOK-EVERT\SGRRHH\fotos\`
- NO intentes acceder directamente a estas carpetas, usa la aplicación

### Tipos de permiso disponibles (Colombia):
1. Calamidad doméstica
2. Cita médica
3. Luto (muerte de familiar)
4. Licencia de maternidad
5. Licencia de paternidad
6. Permiso sindical
7. Lactancia
8. Diligencias personales
9. Estudio
10. Matrimonio
11. Mudanza
12. Trámites legales
13. Otros

### Soporte:
Si tienes problemas técnicos:
1. Revisa esta guía primero
2. Verifica tu conexión de red
3. Contacta al administrador del sistema
4. El administrador puede revisar los logs en `\\ELITEBOOK-EVERT\SGRRHH\logs\`

---

**¡Listo!** Ya puedes empezar a usar SGRRHH como Operador. ¡Bienvenida! 🎉
