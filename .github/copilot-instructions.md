# Copilot Instructions

Este archivo define las reglas y comportamientos esperados para agentes de IA (GitHub Copilot, Claude, etc.) al trabajar con este repositorio.

---

## 🧠 Regla Principal: Gestión de Contexto y Delegación

### 🔍 PASO 0: Detectar en qué fase del flujo estamos

**ANTES de hacer cualquier cosa, evaluar el prompt inicial del usuario:**

```
                         ┌─────────────────────────────────┐
                         │   EVALUAR PROMPT DEL USUARIO    │
                         └─────────────────────────────────┘
                                        │
                    ┌───────────────────┴───────────────────┐
                    ▼                                       ▼
        ┌───────────────────────┐             ┌───────────────────────┐
        │  PROMPT AMBIGUO/      │             │  PROMPT ESTRUCTURADO  │
        │  EXPLORATORIO         │             │  CON CONTEXTO         │
        │                       │             │                       │
        │  Ejemplos:            │             │  Ejemplos:            │
        │  • "¿cómo funciona X?"│             │  • Tiene secciones    │
        │  • "¿está preparado?" │             │  • SQL definido       │
        │  • "quiero agregar Y" │             │  • Entidades claras   │
        │  • preguntas abiertas │             │  • Pasos ordenados    │
        └───────────────────────┘             └───────────────────────┘
                    │                                       │
                    ▼                                       ▼
        ┌───────────────────────┐             ┌───────────────────────┐
        │  → FASE INVESTIGAR    │             │  → FASE EJECUTAR      │
        │    (explorar código,  │             │    (implementar       │
        │     entender contexto)│             │     directamente)     │
        └───────────────────────┘             └───────────────────────┘
```

### Indicadores de PROMPT AMBIGUO (→ Investigar primero):
- Preguntas con "¿cómo?", "¿qué pasa si?", "¿está preparado?"
- Solicitudes vagas: "quiero mejorar X", "agregar funcionalidad Y"
- No menciona archivos, tablas o entidades específicas
- Es una exploración o análisis

### Indicadores de PROMPT ESTRUCTURADO (→ Ejecutar directo):
- Tiene secciones con headers (##, ###)
- Incluye código SQL, C#, o pseudocódigo
- Define entidades, DTOs, interfaces
- Lista pasos numerados o fases
- Referencia archivos específicos a crear/modificar
- Viene de un archivo `PROMPT_*.md` del proyecto

---

### Cuando los cambios son grandes o complejos:

**PROBLEMA:** En sesiones largas de investigación + implementación, la calidad puede degradarse porque:
1. La investigación consume gran parte de la ventana de contexto
2. La planificación y ejecución compiten por el contexto restante
3. Detalles importantes pueden perderse o pasarse por alto

**SOLUCIÓN:** Seguir el patrón **Investigar → Documentar → Delegar**

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────────┐
│   INVESTIGAR    │────▶│   DOCUMENTAR     │────▶│   DELEGAR/EJECUTAR  │
│   (esta sesión) │     │   (prompt.md)    │     │   (nueva sesión)    │
└─────────────────┘     └──────────────────┘     └─────────────────────┘
```

**IMPORTANTE:** Si el usuario ya proporciona un prompt estructurado, SALTAR directamente a ejecutar.

### Criterios para aplicar esta regla:

| Señal | Acción |
|-------|--------|
| Cambio requiere 3+ archivos nuevos | Crear prompt detallado |
| Se necesitan nuevas tablas en BD | Documentar migración SQL |
| Cambio afecta múltiples capas (Domain, Infrastructure, UI) | Separar en fases |
| La investigación tomó más del 30% del contexto | Documentar hallazgos |
| Usuario hace pregunta exploratoria ("¿cómo funciona X?") | Responder + ofrecer documentar solución |

### Formato del prompt de delegación:

Los prompts para delegar deben incluir:
1. **Contexto del problema** - Por qué se necesita el cambio
2. **Objetivos claros** - Qué debe lograr la implementación
3. **Cambios en BD** - Scripts SQL completos
4. **Entidades/DTOs** - Código C# listo para usar
5. **Interfaces** - Contratos de repositorios/servicios
6. **Componentes UI** - Estructura de páginas Blazor
7. **Casos de prueba** - Escenarios para validar
8. **Orden de implementación** - Fases sugeridas

### Ubicación de prompts:
- Raíz del proyecto: `PROMPT_[NOMBRE_MODULO].md`
- O en carpeta dedicada: `PROMPTS/PROMPT_[NOMBRE].md`

---

## 📋 Otras Reglas de Comportamiento

### 1. Preguntar antes de asumir
Cuando la solicitud es ambigua, preguntar:
> "¿Quieres que documente la solución para implementar después, o prefieres que implemente directamente?"

### 2. Investigar antes de implementar
- Revisar archivos existentes antes de crear nuevos
- Verificar patrones ya establecidos en el proyecto
- Consultar la estructura de BD actual

### 3. Respetar la arquitectura existente
Este proyecto usa:
- **Clean Architecture** (Domain → Infrastructure → Server)
- **Dapper** como ORM (no Entity Framework)
- **SQLite** como base de datos
- **Blazor Server** para UI
- **Estilo "hospitalario"** (Courier New, diseño terminal)

### 4. Validar compilación
Después de cambios significativos:
```powershell
dotnet build 2>&1 | Select-String -Pattern "error|Build succeeded|Build FAILED"
```

### 5. No crear documentación innecesaria
- NO crear archivos .md para documentar cada cambio
- SÍ crear prompts cuando el cambio es grande y delegable
- SÍ actualizar CHANGELOG.md para features importantes

---

## 🗂️ Estructura del Proyecto

```
SGRRHH.Local/
├── SGRRHH.Local.Domain/        # Entidades, Enums, DTOs, Interfaces
├── SGRRHH.Local.Infrastructure/ # Repositorios, Servicios, Data
├── SGRRHH.Local.Server/        # Blazor Server, Components, Pages
├── SGRRHH.Local.Shared/        # Código compartido
├── scripts/                    # Migraciones SQL, PowerShell
└── docs/                       # Documentación técnica
```

---

## 🔧 Comandos Útiles

```powershell
# Build
cd SGRRHH.Local
dotnet build

# Run en desarrollo
dotnet watch --project SGRRHH.Local.Server

# Consultar BD
sqlite3 "C:\SGRRHH\Data\sgrrhh.db" ".tables"
sqlite3 "C:\SGRRHH\Data\sgrrhh.db" ".schema nombre_tabla"

# Detener servidor
Stop-Process -Name "SGRRHH.Local.Server" -Force -ErrorAction SilentlyContinue
```

---

## 📝 Notas para el Agente

1. **Contexto colombiano:** El sistema maneja normativa laboral colombiana (EPS, ARL, prestaciones sociales, etc.)

2. **Usuarios target:** ~20 empleados de empresa forestal. Ingenieros de campo usan el sistema.

3. **Prioridad UX:** Interfaz simple, sin animaciones excesivas, funcional en equipos modestos.

4. **Idioma:** Todo en español (código, comentarios, UI, documentación).

5. **Deploy SSH actual:**
        - Build Release self-contained `win-x64`, empaquetado ZIP → `C:\SGRRHH`.
        - Limpieza en servidor preserva `Data`, `certs`, `logs`; nunca sobrescribir DB salvo petición explícita.
        - Servicio `nssm` (`SGRRHH_Local`) obligatorio; logs en `C:\SGRRHH\logs` con rotación (1 MB / 24h).
        - Accesos directos en escritorio del servidor: `SGRRHH - Consola` (ejecución con ventana y logs en vivo) y `SGRRHH - Ver Logs` (tail del log).
        - Certificado requerido: `C:\SGRRHH\certs\localhost+2.p12`; si falta, copiar antes de iniciar.
        - Si hay errores de esquema (columnas faltantes), la DB no se migra automáticamente: respaldar, eliminar wal/shm y copiar la DB válida.

---

*Última actualización: Enero 2026*
