# Configuración de Compilación - SGRRHH

## 📋 Decisión de Arquitectura: Framework-Dependent

**Fecha de decisión**: 28 de noviembre de 2025  
**Estado**: ✅ ACTIVO - Esta es la configuración oficial del proyecto

---

## Regla Establecida

> **TODOS los builds de SGRRHH deben ser Framework-Dependent**

Esto aplica para:
- ✅ GitHub Actions (workflow automático)
- ✅ Scripts de compilación manual
- ✅ Releases publicados
- ✅ Distribuciones a clientes

---

## Configuración Técnica

### Comando de Compilación Estándar

```bash
dotnet publish --configuration Release --runtime win-x64 --self-contained false
```

**Parámetros clave:**
- `--self-contained false` - Framework-Dependent (NO incluir runtime)
- `--runtime win-x64` - Target Windows 64-bit

### Archivos Modificados

1. **`.github/workflows/release.yml`**
   - Línea 26-30: Compilación de SGRRHH.WPF y Updater

2. **`installer/build_manual_package.bat`**
   - Script principal de compilación manual

---

## Requisitos del Sistema Cliente

### Obligatorio en cada máquina cliente:

**Microsoft .NET 8.0 Desktop Runtime (x64)**
- Descarga oficial: https://dotnet.microsoft.com/download/dotnet/8.0
- Buscar: `.NET Desktop Runtime 8.0.x (x64)`
- Tamaño: ~55 MB (instalación una sola vez)

---

## Ventajas de Framework-Dependent

| Aspecto | Self-Contained | Framework-Dependent (ACTUAL) |
|---------|----------------|------------------------------|
| **Tamaño del paquete** | ~100-120 MB | ~20-30 MB ✅ |
| **Número de archivos** | ~187 archivos | ~40-60 archivos ✅ |
| **Velocidad de descarga** | Lenta | Rápida ✅ |
| **Actualizaciones futuras** | Pesadas | Ligeras ✅ |
| **Seguridad** | Runtime fijo | Runtime actualizable ✅ |
| **Rendimiento** | Estándar | Optimizado por OS ✅ |
| **Requisito previo** | Ninguno | .NET Runtime (una vez) ⚠️ |

---

## Proceso de Instalación en Cliente

### Primera Instalación

1. **Instalar .NET 8 Desktop Runtime** (si no está instalado)
   - Verificar ejecutando: `dotnet --list-runtimes`
   - Debe aparecer: `Microsoft.WindowsDesktop.App 8.0.x`

2. **Instalar SGRRHH**
   - Descomprimir `SGRRHH-Install.zip`
   - Copiar archivos a la ubicación deseada
   - Ejecutar `SGRRHH.exe`

### Actualizaciones Futuras

- Las actualizaciones automáticas desde GitHub serán más rápidas (~20-30 MB)
- No requieren reinstalar .NET Runtime
- Proceso completamente automático

---

## Scripts Disponibles

### Script Principal (Recomendado)
```batch
installer\build_manual_package.bat
```
Genera: `installer\manual-package\SGRRHH-Install.zip` (Framework-Dependent)

### Script Legacy (Solo para casos especiales)
```batch
installer\build_manual_package_self_contained.bat
```
⚠️ **NO USAR** excepto en casos muy específicos donde el cliente no pueda instalar .NET Runtime

---

## Verificación de Cumplimiento

Para verificar que un build es Framework-Dependent:

1. **Revisar tamaño del paquete**
   - Framework-Dependent: ~20-30 MB
   - Self-Contained: ~100+ MB

2. **Revisar archivos DLL del runtime**
   - Framework-Dependent: NO incluye `coreclr.dll`, `clrjit.dll` propios
   - Self-Contained: Incluye todas las DLLs del runtime

3. **Ejecutar sin .NET instalado**
   - Framework-Dependent: Mostrará error pidiendo instalar .NET
   - Self-Contained: Se ejecutará sin problemas

---

## Excepciones

Esta regla NO aplica para:
- Proyectos de prueba internos
- Builds locales de desarrollo (Debug)
- Herramientas auxiliares en la carpeta `tools/`

---

## Historial de Cambios

| Fecha | Cambio | Razón |
|-------|--------|-------|
| 2025-11-28 | Establecida regla Framework-Dependent | Reducir tamaño de paquetes y mejorar velocidad de actualizaciones |

---

## Contacto

Para preguntas sobre esta configuración, consultar con el equipo de desarrollo.
