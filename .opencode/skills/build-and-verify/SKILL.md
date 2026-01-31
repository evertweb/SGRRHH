---
name: build-and-verify
description: Compila el proyecto SGRRHH con el comando específico y verifica errores. Usar siempre después de modificar código C# o Razor.
license: MIT
compatibility: opencode
metadata:
  project: SGRRHH
  type: build-verification
  command: dotnet-build
---
# Compilación y Verificación - SGRRHH

## Comando de Build OBLIGATORIO

**SIEMPRE** usar este comando exacto para compilar:

```powershell
dotnet build -v:m /bl:build.binlog 2>&1 | Tee-Object build.log
```

> [!CAUTION]
> Esta regla es **INVIOLABLE**. No usar otro comando de build.

## Directorio de Trabajo

Ejecutar desde: `c:\Users\evert\Documents\rrhh\SGRRHH.Local`

## Interpretación de Resultados

| Resultado | Acción |
|-----------|--------|
| `Build succeeded` | ✅ Continuar con la tarea |
| `Build FAILED` | ❌ Leer errores y corregir ANTES de continuar |
| Warnings CS8019 | ⚠️ Usings innecesarios, corregir solo si afectan |

## Cuándo Ejecutar Build

1. Después de modificar cualquier archivo `.cs` o `.razor`
2. Después de agregar/modificar dependencias en `.csproj`
3. Antes de hacer deploy
4. Cuando el usuario pida verificar compilación

## Post-Build Exitoso

Si el build es exitoso y hay cambios significativos:
- Si el servidor está corriendo → Sugerir hot reload o restart
- Si se modificaron migraciones → Recordar aplicarlas

## Errores Comunes

### Error de conexión SQLite
```
Unable to open database file
```
→ Verificar que la ruta `C:\SGRRHH\Data\sgrrhh.db` existe

### Error de tipo nullable
```
CS8600: Converting null literal or possible null value to non-nullable type
```
→ Usar `?` en el tipo o inicializar con `= default!`

### Error de referencia circular
```
CS0246: The type or namespace name 'X' could not be found
```
→ Verificar referencias entre proyectos en `.csproj`