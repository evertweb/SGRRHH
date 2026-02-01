---
name: build-and-verify
description: Compila el proyecto SGRRHH con el comando específico y verifica errores. Usar siempre después de modificar código C# o Razor.
---

# Build y Compilación - SGRRHH

## Comando de Build OBLIGATORIO

> [!CAUTION]
> Esta regla es **INVIOLABLE**. El siguiente comando es el ÚNICO permitido para compilar el proyecto.

```powershell
dotnet build -v:m /bl:build.binlog 2>&1 | Tee-Object build.log
```

### Componentes del Comando

| Parte | Propósito |
|-------|-----------|
| `dotnet build` | Comando de compilación base |
| `-v:m` | Verbosity minimal (reduce ruido) |
| `/bl:build.binlog` | Binary log para diagnóstico detallado |
| `2>&1` | Redirige stderr a stdout |
| `\| Tee-Object build.log` | Guarda output en archivo y muestra en consola |

## Directorio de Trabajo

**SIEMPRE ejecutar desde:**
```
C:\Users\evert\Documents\rrhh\SGRRHH.Local
```

## Cuándo Ejecutar Build

1. ✅ Después de modificar cualquier archivo `.cs` o `.razor`
2. ✅ Después de agregar/modificar dependencias en `.csproj`
3. ✅ Antes de hacer deploy
4. ✅ Cuando el usuario pida verificar compilación
5. ✅ Después de merge de feature branch

## Interpretación de Resultados

| Resultado | Significado | Acción |
|-----------|-------------|--------|
| `Build succeeded` | ✅ Compilación exitosa | Continuar con la tarea |
| `Build FAILED` | ❌ Errores de compilación | **DETENER** - Leer errores y corregir ANTES de continuar |
| `Warning(s)` | ⚠️ Advertencias | Revisar si son críticas |
| `CS8019` | Usings innecesarios | Corregir solo si afectan funcionalidad |

## Errores Comunes y Soluciones

### Error de Conexión SQLite
```
Unable to open database file
```
**Solución:** Verificar que la ruta `C:\SGRRHH\Data\sgrrhh.db` existe en producción o la configurada en `appsettings.json` en desarrollo.

### Error de Tipo Nullable
```
CS8600: Converting null literal or possible null value to non-nullable type
```
**Solución:** Usar `?` en el tipo o inicializar con `= default!`

### Error de Referencia Circular
```
CS0246: The type or namespace name 'X' could not be found
```
**Solución:** Verificar referencias entre proyectos en `.csproj`

### Error de Namespace
```
CS0234: The type or namespace name 'Y' does not exist in the namespace 'X'
```
**Solución:** Verificar imports y referencias de paquetes NuGet

## Post-Build Exitoso

Si el build es exitoso y hay cambios significativos:

1. **Servidor corriendo:** Sugerir hot reload o restart
   ```bash
   # Hot reload automático si usas dotnet watch
   # O restart manual del servidor
   ```

2. **Migraciones modificadas:** Recordar aplicarlas
   ```bash
   sqlite3 "C:\SGRRHH\Data\sgrrhh.db" < scripts/migration_xxx_v1.sql
   ```

## NO Delegar al Usuario

> [!IMPORTANT]
> Este comando debe ejecutarse **automáticamente** por el agente.
> NO pedir al usuario que compile manualmente.

## Archivo de Log

El archivo `build.log` se genera en el directorio actual y contiene:
- Warnings completos
- Errores detallados
- Información de restauración de paquetes

Útil para diagnóstico cuando la salida de consola es insuficiente.

## Captura de Output Completo (Anti-Truncamiento)

> [!IMPORTANT]
> Si el output de compilación aparece truncado, usar esta estrategia para ver TODOS los errores.

### Estrategia Recomendada: Script Python + view_file

La forma más confiable de ver errores completos:

```powershell
# Compilar guardando output completo
dotnet build 2>&1 | Out-File build_errors.txt -Encoding UTF8

# Filtrar solo errores con script Python
python scripts\parse_build_errors.py build_errors.txt 2>&1 | Out-File errors_summary.txt -Encoding ASCII
```

Luego usar `view_file` en `errors_summary.txt` para ver el resumen.

**Archivos involucrados:**
- `scripts/parse_build_errors.py` - Script que extrae errores CS