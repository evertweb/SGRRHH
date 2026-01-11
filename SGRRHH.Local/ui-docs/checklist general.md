CHECKLIST MAESTRO GENERAL 
UI / UX ESTILO SISTEMA HOSPITALARIO LEGACY
(Aplicable a SGRRHH web moderno)

1. FILOSOFÍA FUNDAMENTAL (NO NEGOCIABLE)
☐ El sistema se opera, no se explora
 ☐ El usuario no decide caminos, sigue flujos
 ☐ La información es prioritaria sobre la estética
 ☐ La interfaz no entretiene, informa y ejecuta
 ☐ El sistema asume usuarios entrenados
 ☐ El error debe ser explícito, nunca oculto
 ☐ El sistema prefiere bloquear antes que permitir ambigüedad
 ☐ El frontend se comporta como terminal gráfica

2. ARQUITECTURA MENTAL DEL FRONTEND
☐ El frontend NO contiene lógica de negocio
 ☐ El frontend NO “infiera” estados
 ☐ El frontend NO optimiza UX moderna
 ☐ Cada pantalla representa una transacción
 ☐ Cada transacción tiene inicio, validación y cierre
 ☐ El backend es la autoridad final
 ☐ No existe “estado feliz por defecto”
 ☐ El error es un estado válido y visible

3. LAYOUT (ESTRUCTURA VISUAL)
☐ Ancho fijo (1024–1280 px)
 ☐ Layout estable, sin reflow
 ☐ Sin diseño responsive fluido
 ☐ Sin tarjetas
 ☐ Sin grid dinámico
 ☐ Sin sombras
 ☐ Sin glassmorphism
 ☐ Sin blur
 ☐ Bordes visibles y funcionales
 ☐ Separadores claros (líneas, no espacios decorativos)
Estructura obligatoria:
☐ Encabezado del sistema (nombre + versión + usuario)
 ☐ Barra de navegación textual
 ☐ Ruta (breadcrumb) visible
 ☐ Área central de trabajo
 ☐ Zona de acciones fija (inferior o lateral)

4. COLOR (DISCIPLINA EXTREMA)
☐ Fondo blanco
 ☐ Texto negro
 ☐ Bordes grises
 ☐ Gris claro para campos deshabilitados
Uso exclusivo de color:
☐ Rojo → error bloqueante
 ☐ Amarillo → advertencia
 ☐ Verde → confirmación explícita
☐ Prohibido usar color para decoración
 ☐ Prohibido degradados
 ☐ Prohibido color por “branding”

5. TIPOGRAFÍA
☐ Tipografía legible, técnica
 ☐ Preferible monoespaciada o semi-mono
 ☐ Tamaños constantes
 ☐ Sin jerarquías exageradas
 ☐ Alineación perfecta de etiquetas y campos
 ☐ Cero fuentes decorativas

6. COMPONENTES DE FORMULARIO
☐ Etiquetas SIEMPRE visibles (no placeholders)
 ☐ Campos alineados verticalmente
 ☐ Longitudes de campo coherentes con el dato
 ☐ Campos obligatorios claramente marcados
 ☐ Campos bloqueados claramente diferenciados
 ☐ Sin autoformateo silencioso
 ☐ Sin autocompletado mágico

7. VALIDACIÓN (CRUDA Y EXPLÍCITA)
☐ Validación al enviar
 ☐ Validación backend obligatoria
 ☐ Error bloquea flujo
 ☐ Mensaje claro y técnico
Formato de error correcto:
ERROR: El campo FECHA_INGRESO no puede ser mayor a la fecha actual.
☐ Nunca “algo salió mal”
 ☐ Nunca mensajes emocionales
 ☐ Nunca errores silenciosos
 ☐ Nunca permitir continuar con datos inválidos

8. MENSAJES DEL SISTEMA
☐ Mensajes impersonales
 ☐ Lenguaje técnico
 ☐ Sin emojis
 ☐ Sin tono amable
Ejemplos válidos:
“Registro guardado correctamente.”


“Operación cancelada por el usuario.”


“Acción no permitida.”



9. NAVEGACIÓN (CLAVE ABSOLUTA)
☐ Navegación por teclado prioritaria
 ☐ Enter confirma
 ☐ ESC cancela
 ☐ Tab navega campos
 ☐ Teclas de función visibles (aunque sean simuladas)
Ejemplo:
F5 Guardar   F8 Cancelar   F2 Buscar   ESC Salir

☐ El mouse es secundario
 ☐ No gestos
 ☐ No drag & drop

10. FLUJOS TRANSACCIONALES
☐ Una pantalla = una acción
 ☐ Confirmación explícita antes de guardar
 ☐ Resultado explícito después de guardar
 ☐ Retorno controlado
 ☐ No navegación libre después de acciones críticas

11. ESTADOS DEL SISTEMA
☐ Estado cargando visible y sobrio
 ☐ Estado error visible y dominante
 ☐ Estado vacío explícito
 ☐ Estado bloqueado claramente marcado
☐ Nunca esconder estados
 ☐ Nunca animar estados

12. SEGURIDAD VISUAL Y OPERATIVA
☐ Usuario visible en pantalla
 ☐ Rol visible
 ☐ Acciones restringidas por rol
 ☐ Acciones no permitidas visibles pero bloqueadas
 ☐ Confirmación en acciones críticas

13. AUDITORÍA (MENTALIDAD HOSPITALARIA)
☐ Toda acción importante es auditable
 ☐ Mostrar fecha, hora, usuario
 ☐ Mostrar resultado de acción
 ☐ No permitir acciones sin registro

14. PERFORMANCE (PERCEPCIÓN)
☐ Prefiere estabilidad a velocidad aparente
 ☐ No loaders modernos
 ☐ No skeletons
 ☐ No transiciones
El usuario espera.

15. QUÉ ESTÁ PROHIBIDO (SIN EXCEPCIONES)
☐ Animaciones
 ☐ Transiciones suaves
 ☐ Tooltips decorativos
 ☐ Íconos innecesarios
 ☐ Sonidos
 ☐ Feedback emocional
 ☐ UX “amigable”
 ☐ Diseño tipo app móvil

16. PRUEBA DEFINITIVA (SI FALLA, NO ES HOSPITALARIO)
Responde SÍ o NO:
☐ ¿Un usuario entrenado puede usar el sistema sin mirar el mouse?
 ☐ ¿Dos personas ven exactamente la misma UI en diferentes PCs?
 ☐ ¿El sistema se siente serio, frío y confiable?
 ☐ ¿Los errores son imposibles de ignorar?
 ☐ ¿La UI no ha cambiado en meses?
Si alguna respuesta es NO → no cumple.

17. PRINCIPIO FINAL (GRÁBALO)
Un sistema estilo  hospitalario no seduce, no explica y no perdona errores.
 Informa, controla y registra.

