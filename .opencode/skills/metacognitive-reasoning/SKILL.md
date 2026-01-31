---
name: metacognitive-reasoning
description: Apply when facing architecture changes, large refactors, design decisions, or complex debugging. 5-step structured reasoning framework.
license: MIT
compatibility: opencode
metadata:
  project: SGRRHH
  type: reasoning-framework
  complexity-threshold: 3-plus-files
---
# Razonamiento Metacognitivo - SGRRHH

## Cuándo Aplicar Este Framework

> [!IMPORTANT]
> Este framework es **OBLIGATORIO** para:
> - Cambios que afecten 3+ archivos
> - Modificaciones a entidades de dominio o BD
> - Debugging de errores que no son obvios
> - Decisiones de arquitectura o diseño
> - Refactorings que afecten múltiples capas

Para preguntas simples o cambios triviales → **responder directamente**.

---

## Proceso de 5 Pasos

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│ DESCOMPONER │────▶│   RESOLVER  │────▶│  VERIFICAR  │────▶│ SINTETIZAR  │────▶│ REFLEXIONAR │
│  (Análisis) │     │  (Solución) │     │ (Validación)│     │ (Integrar)  │     │  (Evaluar)  │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
```

---

### PASO 1: DESCOMPONER
Dividir el problema en subproblemas manejables.

**Preguntas guía:**
- ¿Qué capas del proyecto afecta? (Domain, Infrastructure, Server)
- ¿Qué archivos necesitan modificarse?
- ¿Hay dependencias entre los cambios?
- ¿Se requieren migraciones de BD?

**Formato de salida:**
```
📋 SUBPROBLEMAS IDENTIFICADOS:
1. [Subproblema A] - Capa: Domain
2. [Subproblema B] - Capa: Infrastructure  
3. [Subproblema C] - Capa: Server/UI
```

---

### PASO 2: RESOLVER
Abordar cada subproblema con **nivel de confianza explícito**.

**Escala de confianza:**
| Valor | Significado | Acción |
|-------|-------------|--------|
| 0.9-1.0 | Alta confianza | Proceder sin validación extra |
| 0.7-0.8 | Confianza moderada | Proceder pero verificar |
| 0.5-0.6 | Confianza baja | Investigar más antes de proceder |
| < 0.5 | Muy baja confianza | Preguntar al usuario |

**Formato de salida:**
```
🔧 SOLUCIÓN SUBPROBLEMA 1:
   Enfoque: [descripción]
   Confianza: 0.85
   Razón: [por qué este nivel]
```

---

### PASO 3: VERIFICAR
Revisar cada solución antes de implementar.

**Checklist de verificación:**

| Aspecto | Pregunta |
|---------|----------|
| **Lógica** | ¿El código hace lo que debe hacer? |
| **Hechos** | ¿Los tipos, métodos y rutas existen? |
| **Completitud** | ¿Faltan casos edge o validaciones? |
| **Sesgos** | ¿Estoy asumiendo algo sin verificar? |
| **Estilo** | ¿Cumple con `hospital-ui-style`? |
| **Arquitectura** | ¿Respeta la estructura Clean Architecture? |

**Bandera roja - Detenerse si:**
- El archivo no existe donde supuse
- El método tiene firma diferente
- La entidad no tiene el campo asumido
- El servicio no está registrado en DI

---

### PASO 4: SINTETIZAR
Combinar soluciones usando confianza ponderada.

**Cálculo:**
```
Confianza Total = Σ(confianza_i × peso_i) / Σ(peso_i)

Pesos sugeridos:
- Cambios en Domain: 1.5x
- Cambios en BD: 2.0x
- Cambios en UI: 1.0x
- Cambios en Servicios: 1.3x
```

**Formato de salida:**
```
📊 SÍNTESIS:
   Confianza ponderada: 0.82
   Orden de implementación:
   1. [Paso más seguro primero]
   2. [Paso dependiente]
   3. [Paso final]
```

---

### PASO 5: REFLEXIONAR
Si confianza total < 0.8 → **iterar**.

**Proceso de reflexión:**
1. Identificar el subproblema con menor confianza
2. Documentar la debilidad específica
3. Buscar información adicional (código, docs, usuario)
4. Volver al PASO 2 con nuevo conocimiento

**Formato de salida:**
```
🔄 REFLEXIÓN:
   Debilidad: [área de incertidumbre]
   Acción: [qué investigar]
   Nueva confianza esperada: 0.85
```

---

## Formato de Entrega Final

Toda respuesta compleja debe incluir:

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅ RESPUESTA
[Solución clara y concisa]

📊 CONFIANZA: 0.XX

⚠️ ADVERTENCIAS CLAVE:
• [Riesgo o consideración 1]
• [Dependencia o requisito 2]
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

---

## Ejemplos por Contexto

### Ejemplo: Debugging de Error

```
📋 DESCOMPONER:
1. Error en runtime: NullReferenceException en línea X
2. Posibles causas: servicio no inyectado, dato null de BD

🔧 RESOLVER:
   Hipótesis 1: Servicio no registrado → Confianza: 0.4
   Hipótesis 2: Query retorna null → Confianza: 0.7
   
✓ VERIFICAR:
   Program.cs revisado → Servicio registrado ✓
   Query revisada → No tiene manejo de null ✗

📊 SÍNTESIS:
   Causa: Query sin manejo de null
   Confianza: 0.85
   
✅ RESPUESTA: Agregar verificación null después del await
📊 CONFIANZA: 0.85
⚠️ ADVERTENCIAS: Verificar otros usos similares del query
```

### Ejemplo: Agregar Campo a Entidad

```
📋 DESCOMPONER:
1. Migración SQL (BD)
2. Actualizar entidad (Domain)
3. Actualizar DTO (Shared)
4. Actualizar Repository (Infrastructure)
5. Actualizar UI (Server)

🔧 RESOLVER:
   Cada paso con confianza 0.9 (patrón conocido)

✓ VERIFICAR:
   - Campo nuevo no conflicta con existentes ✓
   - Tipo de dato correcto ✓
   - Nullable definido correctamente ✓

📊 SÍNTESIS:
   Confianza ponderada: 0.88
   Orden: BD → Domain → Shared → Infrastructure → Server

✅ RESPUESTA: [Lista de archivos a modificar con código]
📊 CONFIANZA: 0.88
⚠️ ADVERTENCIAS: Ejecutar migración antes de probar
```

---

## Integración con Otras Skills

| Skill | Cuándo Combinar |
|-------|-----------------|
| `build-and-verify` | Después de PASO 4 (Sintetizar) |
| `blazor-component` | Durante PASO 2 si afecta UI |
| `dapper-repository` | Durante PASO 2 si afecta datos |
| `hospital-ui-style` | Durante PASO 3 en Verificar estilo |

---

## Atajos para Problemas Simples

Si el problema es simple (< 2 archivos, patrón conocido):

```
✅ RESPUESTA RÁPIDA: [Solución]
📊 CONFIANZA: 0.95
```

Sin necesidad de documentar todos los pasos.