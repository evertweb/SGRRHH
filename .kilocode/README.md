# Kilo Code Configuration - SGRRHH

Esta carpeta contiene la configuración de skills y rules para Kilo Code en el proyecto SGRRHH, utilizando **symlinks** para mantener sincronización automática con la estructura principal `.agent/`.

## Estructura con Symlinks

```
.kilocode/
├── rules/                      # → symlink a .agent/rules/
│   ├── architecture.md         # Clean Architecture + Dapper
│   ├── build.md                # Comandos de build obligatorios
│   ├── git.md                  # Flujo Git y Conventional Commits
│   ├── language.md             # Todo en español
│   └── ui-style-strict.md      # Estilos UI estrictos
├── skills/                     # Skills generales (todos los modos)
│   └── git-workflow/ → .agent/skills/git-workflow/
├── skills-code/                # Skills específicos del modo código
│   ├── blazor-component/ → .agent/skills/blazor-component/
│   ├── build-and-verify/ → .agent/skills/build-and-verify/
│   ├── dapper-repository/ → .agent/skills/dapper-repository/
│   └── hospital-ui-style/ → .agent/skills/hospital-ui-style/
└── skills-architect/           # Skills específicos del modo arquitecto
    ├── deploy-ssh/ → .agent/skills/deploy-ssh/
    ├── metacognitive-reasoning/ → .agent/skills/metacognitive-reasoning/
    └── playwright-e2e/ → .agent/skills/playwright-e2e/
```

## Ventajas de los Symlinks

### ✅ Sincronización Automática
- Cualquier cambio en `.agent/` se refleja inmediatamente en `.kilocode/`
- Un solo lugar para mantener el contenido (`.agent/`)
- Sin duplicación de archivos

### ✅ Compatibilidad Completa
- **`.agent/`** sigue funcionando con OpenCode, Cursor, Claude
- **`.kilocode/`** funciona específicamente con Kilo Code
- **Ambos** sistemas usan el mismo contenido

### ✅ Organización por Modo
- Kilo Code ve los skills organizados por modo (code, architect)
- Mejor experiencia contextual según el modo activo

## Rules (Reglas del Proyecto)

Las rules se aplican automáticamente y definen:

- **Arquitectura**: Clean Architecture obligatoria (.NET 8 + Blazor + Dapper)
- **Build**: Comando específico con binary logging
- **Git**: Conventional Commits con scopes del proyecto
- **Idioma**: Todo en español (código, UI, comentarios)
- **UI**: Prohibición estricta de estilos inline

## Skills por Modo

### Skills Generales (Todos los modos)
- **git-workflow**: Flujo Git, branches y Conventional Commits

### Skills para Modo Código
- **blazor-component**: Patrones Blazor Server, componentes y páginas
- **build-and-verify**: Compilación con dotnet build y resolución de errores
- **dapper-repository**: Repositorios con Dapper + SQLite
- **hospital-ui-style**: Clases CSS disponibles y reglas de estilo

### Skills para Modo Arquitecto
- **deploy-ssh**: Scripts de deployment a servidores de producción
- **metacognitive-reasoning**: Framework de razonamiento estructurado
- **playwright-e2e**: Testing automatizado end-to-end

## Fuente de Verdad: .agent/

**Importante:** La estructura `.agent/` sigue siendo la **fuente de verdad**:

- Para **agregar/modificar** skills → editar en `.agent/skills/`
- Para **agregar/modificar** rules → editar en `.agent/rules/`
- Los cambios se reflejan automáticamente en `.kilocode/` vía symlinks

## Uso con Diferentes Herramientas

### Con Kilo Code
```bash
# Ve la estructura organizada por modo
.kilocode/skills-code/         # Skills específicos de desarrollo
.kilocode/skills-architect/    # Skills específicos de arquitectura
```

### Con OpenCode/Cursor/Claude
```bash
# Ve la estructura unificada tradicional
.agent/skills/                 # Todos los skills juntos
.agent/rules/                  # Todas las reglas
```

### Ambos sistemas
- **Rules**: Aplicadas automáticamente en ambos casos
- **Skills**: Disponibles según el contexto de cada herramienta
- **Contenido**: Idéntico en ambos (gracias a symlinks)

## Mantenimiento

Para agregar un nuevo skill:
1. Crear en `.agent/skills/nuevo-skill/SKILL.md`
2. Crear symlink en la categoría apropiada de `.kilocode/`
3. El skill estará disponible en ambos sistemas

---

**Actualizado**: 2026-01-31  
**Enfoque**: Symlinks para sincronización automática