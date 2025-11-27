# 📦 Guía de Instalación - SGRRHH

## Sistema de Gestión de Recursos Humanos v1.0.0

---

## 📋 Requisitos del Sistema

### Requisitos mínimos:
- **Sistema Operativo:** Windows 10 (versión 1809 o posterior) o Windows 11
- **Arquitectura:** 64 bits (x64)
- **RAM:** 4 GB mínimo (8 GB recomendado)
- **Espacio en disco:** 500 MB para la instalación + espacio para datos
- **Pantalla:** Resolución mínima 1366x768

### Requisitos adicionales:
- **Microsoft Edge WebView2 Runtime** (para vista previa de PDFs)
  - Se instala automáticamente en Windows 10/11 modernos
  - Si no está instalado, descargue desde: https://developer.microsoft.com/microsoft-edge/webview2/

---

## 🚀 Instalación

### Opción 1: Usando el instalador (Recomendado)

1. **Descargue el instalador:**
   - Archivo: `SGRRHH_Setup_1.0.0.exe`

2. **Ejecute el instalador:**
   - Haga doble clic en el archivo descargado
   - Si Windows SmartScreen muestra una advertencia, haga clic en "Más información" → "Ejecutar de todos modos"

3. **Siga el asistente de instalación:**
   - Seleccione el idioma (Español o English)
   - Acepte los términos de uso (si aplica)
   - Seleccione la carpeta de instalación (predeterminado: `C:\Program Files\SGRRHH`)
   - Seleccione si desea crear acceso directo en el escritorio
   - Haga clic en "Instalar"

4. **Finalice la instalación:**
   - Opcionalmente, marque "Ejecutar SGRRHH" para iniciar la aplicación
   - Haga clic en "Finalizar"

### Opción 2: Instalación portable (sin instalador)

1. **Descargue el archivo comprimido:**
   - Archivo: `SGRRHH_Portable_1.0.0.zip`

2. **Extraiga el contenido:**
   - Extraiga en la ubicación deseada (ej: `C:\SGRRHH`)

3. **Ejecute la aplicación:**
   - Navegue a la carpeta extraída
   - Ejecute `SGRRHH.exe`

---

## 🔧 Primer Inicio

Al ejecutar SGRRHH por primera vez:

1. **Creación de base de datos:**
   - La base de datos SQLite se creará automáticamente
   - Se crearán las carpetas necesarias para datos

2. **Datos iniciales:**
   - Se crean usuarios predeterminados
   - Se configuran departamentos y cargos de ejemplo
   - Se establecen tipos de permiso según normativa colombiana

3. **Usuarios predeterminados:**

   | Usuario | Contraseña | Rol | Permisos |
   |---------|------------|-----|----------|
   | admin | admin123 | Administrador | Acceso total |
   | secretaria | secretaria123 | Operador | Registrar datos |
   | ingeniera | ingeniera123 | Aprobador | Aprobar permisos |

   > ⚠️ **IMPORTANTE:** Cambie las contraseñas predeterminadas después del primer inicio.

---

## 📂 Estructura de Carpetas

Después de la instalación:

```
C:\Program Files\SGRRHH\
├── SGRRHH.exe              # Ejecutable principal
├── *.dll                   # Bibliotecas del sistema
├── data\
│   ├── sgrrhh.db           # Base de datos SQLite
│   ├── config\
│   │   └── logo.png        # Logo de empresa (opcional)
│   ├── backups\            # Copias de seguridad
│   ├── logs\               # Archivos de log
│   ├── fotos\              # Fotos de empleados
│   └── documentos\         # Documentos generados
└── [carpetas de idiomas]
```

---

## 🔄 Actualización

Para actualizar a una nueva versión:

1. **Realice una copia de seguridad:**
   - Use la función de backup integrada en la aplicación
   - O copie manualmente la carpeta `data`

2. **Ejecute el nuevo instalador:**
   - El instalador detectará la versión anterior
   - Se conservarán los datos de la aplicación

3. **Verifique la actualización:**
   - Inicie la aplicación
   - Verifique que los datos se mantienen correctamente

---

## 🗑️ Desinstalación

### Desde el instalador:
1. Panel de Control → Programas y características
2. Busque "Sistema de Gestión de Recursos Humanos"
3. Haga clic en "Desinstalar"
4. Se le preguntará si desea conservar los datos

### Manual:
1. Elimine la carpeta de instalación
2. Opcionalmente, elimine la carpeta `data` si no desea conservar los datos

---

## ❓ Solución de Problemas

### La aplicación no inicia:

1. **Verifique los requisitos:**
   - Asegúrese de tener Windows 10/11 de 64 bits
   
2. **Verifique WebView2:**
   - Descargue e instale desde: https://developer.microsoft.com/microsoft-edge/webview2/

3. **Ejecute como administrador:**
   - Haga clic derecho en SGRRHH.exe → "Ejecutar como administrador"

### Error de base de datos:

1. **Verifique permisos:**
   - Asegúrese de que la carpeta `data` tiene permisos de escritura

2. **Restaure desde backup:**
   - Use la función de restauración en Configuración → Backup

### La aplicación está lenta:

1. **Verifique recursos del sistema:**
   - Cierre aplicaciones innecesarias
   - Asegúrese de tener suficiente RAM disponible

2. **Compacte la base de datos:**
   - Use la función de backup para crear una copia limpia

---

## 📞 Soporte

Para reportar problemas o solicitar ayuda:

1. **Revise los logs:**
   - Ubicación: `[InstallDir]\data\logs\`
   - Busque archivos `error_YYYY-MM-DD.log`

2. **Información a proporcionar:**
   - Versión de Windows
   - Versión de SGRRHH
   - Descripción del problema
   - Pasos para reproducir el error
   - Archivos de log relevantes

---

## 📄 Licencia

SGRRHH - Sistema de Gestión de Recursos Humanos
Copyright © 2025

Este software es de uso interno para la gestión de recursos humanos.

---

*Última actualización: Noviembre 2025*
