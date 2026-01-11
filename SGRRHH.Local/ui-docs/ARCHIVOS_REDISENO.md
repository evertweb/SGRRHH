# 📁 ARCHIVOS A TOCAR PARA REDISEÑO COMPLETO

---

## 🎨 CSS (Estilos Globales) - 5 archivos

| Archivo | Propósito |
|---------|-----------|
| `wwwroot/css/hospital.css` | PRINCIPAL - Todo el sistema de diseño |
| `wwwroot/app.css` | Overrides de Bootstrap |
| `wwwroot/bootstrap/bootstrap.min.css` | Framework base (considerar reemplazar/remover) |
| `Components/Layout/MainLayout.razor.css` | Estilos aislados del layout |
| `Components/Layout/NavMenu.razor.css` | Estilos del menú lateral (no usado actualmente) |

---

## 🏗️ Layout (Estructura Principal) - 4 archivos

| Archivo | Propósito |
|---------|-----------|
| `Components/Layout/MainLayout.razor` | Header, nav, breadcrumb, contenedor principal |
| `Components/Layout/NavMenu.razor` | Menú lateral (alternativo) |
| `Components/Layout/EmptyLayout.razor` | Layout vacío para login |
| `Components/App.razor` | Punto de entrada, carga de CSS/JS |

---

## 🧩 Componentes Compartidos (UI Reutilizable) - 14 archivos

| Archivo | Propósito |
|---------|-----------|
| `Shared/KeyboardHandler.razor` | Barra de atajos de teclado |
| `Shared/FormModal.razor` | Modal genérico para formularios |
| `Shared/ConfirmDialog.razor` | Diálogo de confirmación |
| `Shared/DataTable.razor` | Tabla de datos reutilizable |
| `Shared/EstadoBadge.razor` | Badges de estado |
| `Shared/EmpleadoCard.razor` | Tarjeta de empleado |
| `Shared/EmpleadoSelector.razor` | Selector/búsqueda de empleados |
| `Shared/CalendarioMini.razor` | Calendario pequeño |
| `Shared/MessageToast.razor` | Notificaciones toast |
| `Shared/NotificationBell.razor` | Campana de notificaciones |
| `Shared/SatelliteSpinner.razor` | Spinner de carga |
| `Shared/ResumenVacacionesPanel.razor` | Panel resumen vacaciones |
| `Shared/UnsavedChangesGuard.razor` | Alerta cambios sin guardar |
| `Shared/AuthorizeViewLocal.razor` | Control de autorización visual |

---

## 📄 Páginas (Vistas Principales) - 17 archivos

| Archivo | Propósito |
|---------|-----------|
| `Pages/Login.razor` | Pantalla de login |
| `Pages/Home.razor` | Dashboard/inicio |
| `Pages/Empleados.razor` | Gestión de empleados |
| `Pages/EmpleadoOnboarding.razor` | Wizard nuevo empleado |
| `Pages/Contratos.razor` | Gestión de contratos |
| `Pages/Documentos.razor` | Gestión de documentos |
| `Pages/Permisos.razor` | Solicitudes de permisos |
| `Pages/Vacaciones.razor` | Gestión de vacaciones |
| `Pages/ControlDiario.razor` | Control de asistencia |
| `Pages/ControlDiarioWizard.razor` | Wizard asistencia rápida |
| `Pages/Reportes.razor` | Reportes e informes |
| `Pages/Auditoria.razor` | Logs de auditoría |
| `Pages/Usuarios.razor` | Gestión de usuarios |
| `Pages/Configuracion.razor` | Configuración del sistema |
| `Pages/Catalogos.razor` | Contenedor de catálogos |
| `Pages/DepartamentosTab.razor` | Tab departamentos |
| `Pages/CargosTab.razor` | Tab cargos |
| `Pages/ActividadesTab.razor` | Tab actividades |
| `Pages/ProyectosTab.razor` | Tab proyectos |
| `Pages/TiposPermisoTab.razor` | Tab tipos de permiso |
| `Pages/Error.razor` | Página de error |

---

## 🖼️ Assets (Imágenes/Iconos) - 3 archivos

| Archivo | Propósito |
|---------|-----------|
| `wwwroot/images/logo-watermark.svg` | Marca de agua |
| `wwwroot/images/default-avatar.svg` | Avatar por defecto |
| `wwwroot/favicon.png` | Favicon |

---

## ⚡ JavaScript (Interactividad) - 2 archivos

| Archivo | Propósito |
|---------|-----------|
| `wwwroot/js/app.js` | Funciones generales JS |
| `wwwroot/js/keyboard-handler.js` | Manejo de atajos de teclado |

---

## 📊 RESUMEN TOTAL

| Categoría | Cantidad |
|-----------|----------|
| CSS | 5 |
| Layout | 4 |
| Componentes Shared | 14 |
| Páginas | 21 |
| Assets | 3 |
| JavaScript | 2 |
| **TOTAL** | **49 archivos** |

---

## ✅ ORDEN RECOMENDADO DE REDISEÑO

1. **FASE 1:** Layout base (`App.razor` → `MainLayout.razor` → `EmptyLayout.razor`)
2. **FASE 2:** CSS globales (`hospital.css` → `app.css`)
3. **FASE 3:** Componentes compartidos (empezar por `FormModal`, `DataTable`, `ConfirmDialog`)
4. **FASE 4:** Páginas principales (`Login` → `Home` → `Empleados`)
5. **FASE 5:** Resto de páginas y tabs
