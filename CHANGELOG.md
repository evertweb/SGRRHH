# Changelog

Todos los cambios notables de este proyecto serán documentados en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/),
y este proyecto adhiere a [Semantic Versioning](https://semver.org/lang/es/).

---

## [1.1.13] - 2025-12-22

### 🧹 Mantenimiento

- **Limpieza de archivos obsoletos**: Eliminados logs de build y archivos temporales del repositorio
- **Mejora de .gitignore**: Agregados patrones para ignorar logs de build y logs de aplicación
- Actualizado texto en pantalla de login: "Ingrese credenciales para acceder"

### 🔧 Cambios Técnicos

- Limpieza de carpetas `bin/obj` de todos los proyectos
- Optimización del repositorio para releases más limpios

---

## [1.1.12] - 2025-12-21

### ✨ Mejoras

- Implementación del estilo Legacy (Brutalismo Funcional) en toda la aplicación
- Sistema de actualizaciones automáticas funcional
- Mejoras de UI/UX en formularios y listas
- Implementación de `LegacyAsyncButton` para feedback visual en operaciones asíncronas

### 🐛 Correcciones

- Corregidos problemas de bloqueo de UI en operaciones asíncronas
- Mejoras de estabilidad general

---

## [1.1.11] y anteriores

### 📋 Funcionalidades Base

- Sistema completo de gestión de empleados
- Gestión de departamentos, cargos y proyectos
- Sistema de permisos y vacaciones
- Gestión de contratos
- Integración con Firebase/Firestore
- Envío de documentos por correo electrónico
- Generación de reportes PDF
- Sistema de chat interno con Sendbird
