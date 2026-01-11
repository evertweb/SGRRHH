SYSTEM DESIGN
MODELO HOSPITAL-LIKE (LEGACY / CRÍTICO)

1. PROPÓSITO DEL SISTEMA
Ejecutar operaciones críticas de forma predecible, controlada y auditable, en un entorno cerrado o semi-cerrado.
No optimiza:
estética


descubrimiento


emoción


Optimiza:
exactitud


repetibilidad


control


trazabilidad



2. PRINCIPIOS RECTORES (NÚCLEO)
☑ Backend autoritario
 ☑ Transacciones explícitas
 ☑ Estado único y verificable
 ☑ UI determinista
 ☑ Errores visibles
 ☑ Cambios mínimos en el tiempo
 ☑ Usuarios entrenados
 ☑ Operación continua (8–12h/día)

3. TOPOLOGÍA DEL SISTEMA
Modelo lógico
[ Usuario ]
    ↓
[ Terminal (Frontend) ]
    ↓
[ Capa de Aplicación ]
    ↓
[ Motor Transaccional ]
    ↓
[ Persistencia ]

Interpretación moderna (tu caso)
Terminal = Blazor server


Capa de aplicación = Cloud Functions / API


Motor transaccional = lógica de negocio C#


Persistencia = Firestore / DB relacional



4. FRONTEND = TERMINAL GRÁFICA
Responsabilidad del frontend
☐ Mostrar datos
 ☐ Capturar entrada
 ☐ Enviar comandos
 ☐ Renderizar resultado
Prohibido al frontend
☒ Tomar decisiones de negocio
 ☒ Inferir estados
 ☒ Auto-corregir datos
 ☒ Optimizar UX moderna
El frontend NO sabe, solo obedece.

5. MODELO DE INTERACCIÓN
El sistema se basa en COMANDOS, no en vistas.
Ejemplo:
COMANDO: REGISTRAR_CONTRATO
DATOS: { empleadoId, fechaInicio, cargo }
RESULTADO: OK | ERROR

Cada acción:
es explícita


es atómica


tiene respuesta clara



6. PANTALLAS = TRANSACCIONES
Cada pantalla representa:
☑ Una acción concreta
 ☑ Un inicio claro
 ☑ Una validación
 ☑ Un cierre
No existen:
pantallas mixtas
flujos implícitos
navegación libre
7. ESTADOS DEL SISTEMA (OBLIGATORIOS)
Todo el sistema debe poder estar en:
☐ Idle (espera)
 ☐ Captura
 ☐ Validando
 ☐ Ejecutando
 ☐ Confirmado
 ☐ Error
⚠ No hay estados invisibles.

8. MANEJO DE ERRORES (CRÍTICO)
☑ Error siempre visible
 ☑ Error siempre bloqueante
 ☑ Error siempre descriptivo
 ☑ Error siempre auditable
Ejemplo:
ERROR CRÍTICO
Código: HR-004
Descripción: Fecha de ingreso inválida
Acción requerida: Corregir campo FECHA_INGRESO


9. SEGURIDAD OPERATIVA
☑ Autenticación obligatoria
 ☑ Rol visible en pantalla
 ☑ Permisos explícitos
 ☑ Acciones restringidas
 ☑ Confirmación en acciones sensibles
No se confía en el usuario, se lo controla.

10. AUDITORÍA (PARTE DEL DISEÑO)
Toda acción relevante genera:
☑ Usuario
 ☑ Fecha
 ☑ Hora
 ☑ Acción
 ☑ Resultado
Nada es opcional.

11. PERFORMANCE (MENTALIDAD)
☑ Latencia estable > rapidez visual
 ☑ Sin animaciones
 ☑ Sin loaders engañosos
El usuario espera, el sistema responde cuando está listo.

12. CONSISTENCIA TEMPORAL
☑ La UI no cambia sin razón
 ☑ Cambios pequeños y controlados
 ☑ Versionado visible
Un sistema hospitalario no se rediseña cada año.

13. ESCALABILIDAD (REALISTA)
☑ Escala por usuarios entrenados
 ☑ Escala por módulos
 ☑ No escala por “experiencia”
No se optimiza para miles de usuarios casuales.

14. DESARROLLO Y MANTENIMIENTO
☑ Código predecible
 ☑ Pocas abstracciones
 ☑ Lógica clara
 ☑ Documentación operativa

15. PRUEBA DEFINITIVA DE DISEÑO
Responde SÍ o NO:
☐ ¿El sistema podría usarse sin cambios durante 10 años?
 ☐ ¿Un error es imposible de ignorar?
 ☐ ¿Cada acción queda registrada?
 ☐ ¿El usuario siente control, no libertad?
Si alguna es NO → no es hospital-like.

16. FRASE FINAL (GUÍA ABSOLUTA)
Un sistema estilo  hospitalario no se diseña para gustar.
 Se diseña para no fallar.

