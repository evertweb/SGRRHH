CHECKLIST EXTENDIDA
APLICACIÓN POR COMPONENTES (HOSPITAL-LIKE)

1. PANTALLAS (VIEWS / PAGES)
Se aplica a:
Páginas Blazor


Rutas principales


Vistas de módulo (Empleados, Contratos, Nómina, etc.)


☐ Una pantalla = una operación
 ☐ Pantallas con propósito único (no dashboards modernos)
 ☐ Layout idéntico entre pantallas
 ☐ No scroll infinito
 ☐ Scroll solo vertical y controlado
 ☐ Encabezado siempre visible
 ☐ Acciones siempre en la misma posición
PROHIBIDO
 ☐ Pantallas tipo “overview bonito”
 ☐ Cards
 ☐ Widgets
 ☐ Resúmenes gráficos
 ☐ KPI visuales

2. FORMULARIOS (EL CORAZÓN DEL SISTEMA)
Se aplica a:
Altas


Modificaciones


Consultas


Aprobaciones


2.1 Estructura del formulario
☐ Formulario centrado
 ☐ Ancho fijo
 ☐ Campos en columnas rígidas
 ☐ Etiquetas a la izquierda o arriba (consistente)
 ☐ Orden lógico (no visual)
 ☐ Secciones separadas por líneas
Ejemplo estructural:
[ Datos Generales ]
------------------
Código:
Nombre:
Documento:

[ Información Laboral ]
----------------------
Cargo:
Fecha Ingreso:


2.2 Comportamiento
☐ Enter NO salta de campo (solo guarda)
 ☐ Tab navega secuencialmente
 ☐ ESC cancela formulario
 ☐ Confirmación antes de guardar
 ☐ Bloqueo del formulario durante guardado

2.3 Validaciones
☐ Validación obligatoria al guardar
 ☐ Validación backend obligatoria
 ☐ Error muestra campo exacto
 ☐ Error posiciona foco en campo inválido

3. BOTONES (CRÍTICOS, NO DECORATIVOS)
Este punto sí faltaba y es CLAVE.
3.1 Estilo
☐ Rectangulares
 ☐ Bordes visibles
 ☐ Sin sombras
 ☐ Sin animaciones hover
 ☐ Texto plano (sin íconos)
Ejemplos válidos:
GUARDAR


CANCELAR


BUSCAR


SALIR


Ejemplos prohibidos:
💾


✔


Icon + texto


Floating buttons



3.2 Colores de botones
☐ Gris → acción neutral
 ☐ Blanco → acción secundaria
 ☐ Rojo → acción destructiva
 ☐ Verde → SOLO después de confirmar

3.3 Comportamiento
☐ Click único (no doble acción)
 ☐ Deshabilitado durante ejecución
 ☐ Acción clara e inmediata
 ☐ Confirmación explícita en acciones críticas

4. CAMPOS DE ENTRADA (INPUTS)
☐ Bordes visibles
 ☐ Fondo blanco
 ☐ Texto negro
 ☐ Tamaño proporcional al dato
 ☐ Sin placeholders explicativos
 ☐ Máscara SOLO si es obligatoria
 ☐ No auto-correcciones

5. TABLAS (MUY IMPORTANTES)
Se aplica a:
Listados


Búsquedas


Históricos


☐ Tabla como estructura principal
 ☐ Encabezados claros
 ☐ Filas compactas
 ☐ Sin zebra decorativa
 ☐ Selección explícita (fila resaltada)
 ☐ Ordenamiento manual o fijo
PROHIBIDO:
 ☐ DataTables modernos
 ☐ Filtros mágicos
 ☐ Scroll infinito
 ☐ Paginación animada

6. MODALES / DIÁLOGOS
☐ Uso mínimo
 ☐ Fondo bloqueado
 ☐ Texto claro
 ☐ Acciones explícitas
 ☐ ESC cierra
 ☐ Sin animaciones
Ejemplo correcto:
¿CONFIRMAR REGISTRO DE CONTRATO?
 [SÍ] [NO]

7. MENÚS DE NAVEGACIÓN
☐ Texto plano
 ☐ Siempre visible
 ☐ Sin íconos
 ☐ Orden fijo
 ☐ Accesos directos por teclado

8. ESTADOS DE ERROR / ALERTAS
☐ Error bloquea interacción
 ☐ Fondo claro
 ☐ Texto oscuro
 ☐ Error no desaparece solo
 ☐ Requiere acción del usuario

9. ESTILO CSS (NORMAS TÉCNICAS)
☐ CSS plano
 ☐ Sin frameworks visuales
 ☐ Clases semánticas
 ☐ Variables CSS mínimas

10. CONSISTENCIA GLOBAL
☐ Un solo estilo para toda la app
 ☐ Ningún componente “bonito” aislado
 ☐ Todo cambio visual es deliberado
 ☐ UI estable en el tiempo

11. PRUEBA FINAL POR COMPONENTE
Para cada componente pregúntate:
☐ ¿Este componente existe para ejecutar una acción o para verse bien?
 ☐ ¿Un hospital real lo aceptaría?
 ☐ ¿Podría operar esto 8 horas seguidas sin fatiga?
Si alguna es NO → no cumple.

12. CONCLUSIÓN CLAVE
El estilo hospitalario no se aplica solo a pantallas.
 Se impone a botones, campos, tablas, mensajes y flujos.
 Un solo componente moderno rompe la ilusión completa.

