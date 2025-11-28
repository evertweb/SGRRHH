# Guía de Distribución Manual - SGRRHH v1.0.5

## ⚠️ IMPORTANTE
Esta es la **primera versión con el nuevo sistema de actualizaciones**. Debe instalarse manualmente en todos los clientes. Las versiones futuras se actualizarán automáticamente.

---

## Paso 1: Compilar el Paquete de Distribución

### Opción Recomendada: Script Automático

1. Abra una terminal en la carpeta del proyecto
2. Ejecute:
   ```batch
   cd C:\Users\evert\Documents\rrhh\installer
   .\build_manual_package.bat
   ```

3. Esto creará:
   - **Carpeta completa**: `C:\Users\evert\Documents\rrhh\src\publish\SGRRHH-Manual\`
   - **Archivo ZIP**: `C:\Users\evert\Documents\rrhh\installer\manual-package\SGRRHH-Manual-Install.zip`

---

## Paso 2: Distribuir a los Clientes

### Archivos Necesarios

El paquete incluye aproximadamente **187 archivos** (runtime completo de .NET):

**Archivos Principales:**
- `SGRRHH.exe` - Aplicación principal
- `SGRRHH.Updater.exe` - **NUEVO** - Actualizador automático
- `appsettings.json` - Configuración
- `firebase-credentials.json` - Credenciales de Firebase
- Todas las DLLs del runtime .NET 8

**Distribución:**
1. Envíe el archivo `SGRRHH-Manual-Install.zip` a cada cliente
2. O copie toda la carpeta `SGRRHH-Manual` completa

---

## Paso 3: Instalación en el Cliente

### Método 1: Sobre instalación existente (Recomendado)

1. **Cerrar la aplicación actual** (si está ejecutándose)
2. Ir a la carpeta de instalación existente:
   - Normalmente: `C:\Program Files\SGRRHH\`
   - O donde se haya instalado originalmente
3. **Descomprimir** `SGRRHH-Manual-Install.zip` 
4. **Copiar TODOS los archivos** sobre la instalación existente (**reemplazar archivos**)
5. Ejecutar `SGRRHH.exe`

### Método 2: Instalación limpia

1. **Desinstalar** la versión antigua (si existe):
   - Panel de Control → Programas → Desinstalar SGRRHH
2. **Crear carpeta nueva**: `C:\Program Files\SGRRHH\`
3. **Descomprimir** `SGRRHH-Manual-Install.zip` en esa carpeta
4. Crear acceso directo de `SGRRHH.exe` en el escritorio
5. Ejecutar `SGRRHH.exe`

---

## Paso 4: Verificar la Instalación

1. Abrir SGRRHH
2. Ir a **Configuración → Acerca de** (o donde muestre la versión)
3. Verificar que la versión sea **1.0.5** (o la versión nueva)
4. **IMPORTANTE**: Verificar que existe el archivo `SGRRHH.Updater.exe` en la carpeta de instalación

---

## Paso 5: Probar el Sistema de Actualizaciones

### Preparar una Versión de Prueba

1. **Actualizar la versión** en el proyecto:
   ```xml
   <!-- src/SGRRHH.WPF/SGRRHH.WPF.csproj -->
   <Version>1.0.6</Version>
   ```

2. **Hacer un cambio visible** (ejemplo: agregar un texto en la ventana principal)

3. **Commit y Push**:
   ```bash
   git add .
   git commit -m "test: Version 1.0.6 para probar actualizaciones"
   git push
   ```

4. **Crear el tag** para activar GitHub Actions:
   ```bash
   git tag v1.0.6
   git push origin v1.0.6
   ```

5. **Esperar** a que GitHub Actions compile y publique (5-10 minutos)
   - Ir a: https://github.com/evertweb/SGRRHH/actions
   - Verificar que el workflow "Release SGRRHH" se complete exitosamente
   - Verificar que se creó el Release: https://github.com/evertweb/SGRRHH/releases

### Probar la Actualización Automática

1. Abrir SGRRHH (versión 1.0.5)
2. La aplicación debería detectar automáticamente la versión 1.0.6
3. Mostrar una notificación de actualización disponible
4. Al hacer clic en "Actualizar":
   - Se descarga el ZIP
   - Se cierra la aplicación
   - `SGRRHH.Updater.exe` se ejecuta
   - Hace backup
   - Reemplaza archivos
   - Reinicia SGRRHH
5. Verificar que la nueva versión (1.0.6) esté instalada

---

## Troubleshooting

### El updater no se ejecuta
- **Verificar** que `SGRRHH.Updater.exe` existe en la carpeta de instalación
- **Permisos**: Ejecutar SGRRHH como administrador

### La actualización falla
- **Revisar logs** (si se implementaron)
- **Verificar conexión** a internet
- **Permisos** de escritura en la carpeta de instalación

### La aplicación no reinicia después de actualizar
- Reiniciar manualmente `SGRRHH.exe`
- Verificar que no haya procesos zombies de SGRRHH en el Administrador de Tareas

---

## Resumen del Flujo

```
┌─────────────────────────────────────────────────────────┐
│  VERSIÓN ACTUAL (1.0.4 o anterior)                      │
│  - No tiene SGRRHH.Updater.exe                          │
│  - Sistema de actualizaciones antiguo (puede fallar)    │
└─────────────────────────────────────────────────────────┘
                        ↓
                 [INSTALACIÓN MANUAL]
                        ↓
┌─────────────────────────────────────────────────────────┐
│  NUEVA VERSIÓN (1.0.5+)                                 │
│  ✓ Tiene SGRRHH.Updater.exe                             │
│  ✓ Sistema de actualizaciones robusto                   │
│  ✓ Actualizaciones automáticas desde GitHub Releases    │
└─────────────────────────────────────────────────────────┘
                        ↓
         [ACTUALIZACIONES AUTOMÁTICAS FUTURAS]
                        ↓
┌─────────────────────────────────────────────────────────┐
│  VERSIONES FUTURAS (1.0.6+)                             │
│  • Se descargan automáticamente                         │
│  • Se aplican sin intervención manual                   │
│  • Reinician la aplicación correctamente                │
└─────────────────────────────────────────────────────────┘
```

---

## Checklist de Distribución

- [ ] Compilar paquete con `build_manual_package.bat`
- [ ] Verificar que el ZIP contiene `SGRRHH.exe` y `SGRRHH.Updater.exe`
- [ ] Distribuir a todos los clientes
- [ ] Instalar en cada cliente (reemplazando versión anterior)
- [ ] Verificar versión instalada en cada cliente
- [ ] Crear versión de prueba (v1.0.6)
- [ ] Crear tag y publicar en GitHub
- [ ] Probar actualización automática en un cliente
- [ ] Confirmar que funciona correctamente
- [ ] Informar a todos los usuarios que las actualizaciones futuras serán automáticas
