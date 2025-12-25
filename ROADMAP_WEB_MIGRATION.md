# Roadmap Técnico: Migración SGRRHH a Blazor WebAssembly

Este documento sirve como la guía maestra para la transición del sistema SGRRHH de una arquitectura de escritorio (WPF) a una arquitectura Web moderna (Blazor WASM), garantizando la integridad total de las reglas de negocio.

## 1. Visión Arquitectónica (The "Shared Core" Strategy)

El pilar de esta migración es la **Arquitectura Limpia**. No reescribiremos la lógica; la reutilizaremos.

*   **SGRRHH.Core (Referencia Directa):** Contiene todas las `Entities`, `Interfaces` y `Enums`. Este proyecto se mantendrá como una librería `.NET Standard` o `.NET 8` pura para ser consumida tanto por WPF como por la Web.
*   **SGRRHH.Infrastructure (Adaptación):** Aquí reside el mayor cambio técnico. Actualmente, este proyecto usa el `Firebase Admin SDK` (con archivos JSON de Service Account). En la Web, introduciremos una capa de **Interoperabilidad** para que los repositorios cambien al `Firebase Web SDK` (Client-side) de forma transparente.

## 2. Fase 1: Desacoplamiento e Infraestructura (Semana 1)
*   **Abstracción de I/O:** Identificar servicios que usan `System.IO` (como `PdfCompressionService` o `DocumentService`). Debemos asegurar que en la web el flujo sea `Stream` -> `Browser Download`, sin tocar el disco duro local.
*   **Aislamiento del Admin SDK:** Crear directivas de compilación `#if !BROWSER` para evitar que el código del `FirebaseAdmin` se compile en la versión web, ya que causaría errores de seguridad en el navegador.

## 3. Fase 2: El Nuevo Paradigma de Seguridad (Firebase Serverless)
> [!IMPORTANT]
> En WPF, el "Backend" es el Admin SDK. En la Web, el "Backend" son las **Reglas de Seguridad de Firestore**.

*   **Migración de Repositorios:** Ajustar `FirestoreRepository<T>` para que soporte el SDK de cliente. 
*   **Lógica Intocable:** Los servicios centrales (`VacacionService`, `DateCalculationService`, `ControlDiarioService`) **no se modifican**. Su lógica de cálculos legales colombianos debe ser idéntica.
*   **Security Rules Deployment:** Definir el archivo `firestore.rules` que validará que un empleado no pueda aprobar sus propias vacaciones, replicando las validaciones que hoy hace el código de C#.

## 4. Fase 3: UI/UX Legacy y Estado de Aplicación
*   **Look & Feel Windows Classic:** Implementación de un sistema de diseño CSS que replique el "Brutalismo Funcional". Usaremos variables CSS para `#C0C0C0`, `#808080`, etc.
*   **Manejo de Estado (State Management):** Sustituir el patrón de navegación de WPF por un `AppState` centralizado en Blazor para mantener la sesión del usuario y los datos cargados sin re-consultar Firestore constantemente (ahorro de costos).

## 5. Fase 4: Integraciones Externas y Despliegue
*   **Sendbird (Chat):** Migrar de la implementación de C# de escritorio a la librería de JavaScript de Sendbird si es necesario por rendimiento.
*   **Twilio (SMS):** Mantenemos la lógica de servidor (posiblemente vía `Cloud Functions`) para no exponer las llaves de Twilio en el navegador.
*   **GitHub Updates:** Esta funcionalidad se retira en la web, ya que la actualización es nativa al refrescar la página.

## 6. Matrix de Riesgos y Mitigación
| Riesgo | Impacto | Mitigación |
| :--- | :--- | :--- |
| **Precisión Matemática** | Crítico | Reuso binario de `SGRRHH.Core`. No se traduce a JS. |
| **Exposición de Datos** | Alto | Implementación de Reglas de Seguridad en la consola de Firebase. |
| **Rendimiento Inicial** | Medio | Uso de Pre-renderizado y compresión Brotli en el Hosting. |

---
*Este roadmap es un documento vivo y se actualizará a medida que surjan necesidades técnicas específicas durante la Fase 1.*
