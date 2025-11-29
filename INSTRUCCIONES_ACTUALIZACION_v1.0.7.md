# ACTUALIZACIÓN A SGRRHH v1.0.7

## ⚠️ IMPORTANTE: Esta es la última actualización manual

A partir de v1.0.7, las actualizaciones serán **completamente automáticas**.

---

## 📦 Pasos para Actualizar

### 1. Cerrar la Aplicación
- Asegúrate de cerrar completamente `SGRRHH.exe`
- Guarda cualquier trabajo pendiente

### 2. Descomprimir el Archivo
- Descomprime `SGRRHH_v1.0.7.zip` en una carpeta temporal
- Deberías ver archivos como: `SGRRHH.exe`, `SGRRHH.dll`, etc.

### 3. Reemplazar Archivos
- Copia **todos** los archivos descomprimidos
- Pégalos en tu carpeta de instalación de SGRRHH
  - Ejemplo: `C:\Program Files\SGRRHH\`
  - O donde tengas instalada la aplicación
- Cuando pregunte "¿Reemplazar archivos?", haz clic en **"Sí a todo"**

### 4. Verificar la Actualización
- Abre `SGRRHH.exe`
- Ve a **Configuración** (o menú principal)
- Verifica que diga **Versión 1.0.7**

---

## ✅ ¿Qué cambia en v1.0.7?

### Mejoras en el Sistema de Actualizaciones

1. **Actualización Automática**
   - A partir de ahora, la app detecta nuevas versiones automáticamente
   - Ya NO necesitarás actualizar manualmente nunca más

2. **Tres Opciones para Actualizar**
   - **Actualizar ahora**: Descarga e instala inmediatamente
   - **Instalar al cerrar**: Descarga ahora, instala cuando cierres la app
   - **Recordar después**: Te pregunta en el próximo inicio

3. **Seguridad Mejorada**
   - Validación SHA256 de archivos descargados
   - Backup automático antes de actualizar
   - Rollback si falla la instalación

---

## 🚀 Actualizaciones Futuras (v1.0.8 en adelante)

### Cómo Funcionará

1. **Abres SGRRHH.exe**
2. Si hay nueva versión, verás este diálogo:

   ```
   ┌─────────────────────────────────────┐
   │ 🚀 Nueva Versión Disponible         │
   │                                      │
   │ Versión actual: 1.0.7               │
   │ Nueva versión: 1.0.8                │
   │                                      │
   │ [Actualizar ahora]                  │
   │ [📥 Instalar al cerrar]             │
   │ [Recordar después]                  │
   └─────────────────────────────────────┘
   ```

3. **Opción Recomendada**: Haz clic en **"Instalar al cerrar"**
   - La actualización se descarga en segundo plano
   - Puedes seguir trabajando normalmente
   - Cuando cierres la app, se actualiza automáticamente
   - Al abrirla de nuevo, ya tendrás la nueva versión

---

## ❓ Preguntas Frecuentes

### ¿Perderé mis datos al actualizar?
No. Tus datos están en la carpeta `data/` que NO se modifica durante la actualización.

### ¿Qué pasa si falla la actualización?
El sistema crea un backup automático antes de actualizar. Si algo falla, restaura los archivos anteriores automáticamente.

### ¿Necesito conexión a internet?
Solo para descargar la actualización. Una vez descargada, la instalación NO requiere internet.

### ¿Cuánto tarda la actualización?
- Descarga: 30 segundos - 2 minutos (depende de tu internet)
- Instalación: 10-15 segundos

### ¿Puedo seguir usando la versión anterior?
Sí, pero NO recibirás nuevas funcionalidades ni correcciones de errores.

---

## 📞 Soporte

Si tienes problemas con la actualización:

1. Revisa el archivo de log: `data/logs/error_YYYY-MM-DD.log`
2. Contacta al administrador del sistema
3. Reporta el problema con capturas de pantalla

---

**Fecha de publicación**: Enero 2025
**Versión**: 1.0.7
**Autor**: Forestech - Sistema SGRRHH
