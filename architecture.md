# Arquitectura de SGRRHH.Local

## Visión general
- Aplicación Blazor Server (.NET 8) para gestión de RRHH local.
- Patrón Clean Architecture: separación de dominio, infraestructura y presentación.
- Persistencia en SQLite con acceso vía Dapper; scripts SQL en `scripts/` para migraciones y cargas iniciales.
- Despliegue self-contained `win-x64`, con servicio `nssm` en `C:\SGRRHH` y base de datos en `C:\SGRRHH\Data\sgrrhh.db`.

## Capas y proyectos
- **SGRRHH.Local.Domain**: entidades de negocio, enums y contratos de dominio; sin dependencias externas.
- **SGRRHH.Local.Shared**: DTOs y tipos compartidos entre servidor y clientes (validaciones, contratos de transporte).
- **SGRRHH.Local.Infrastructure**: repositorios Dapper, proveedores de datos y servicios concretos; gestiona conexiones SQLite y SQL estático.
- **SGRRHH.Local.Server**: UI Blazor Server, endpoints y composición de servicios; aplica el estilo "hospitalario" definido en `wwwroot/css/hospital.css`.
- **SGRRHH.Local.Tests / SGRRHH.Local.Tests.E2E**: pruebas unitarias/integración y end-to-end; usan bases SQLite temporales.
- **scripts/**: automatizaciones PowerShell y SQL (deploy, backups, migraciones, seeds).

## Servicios y módulos activos (Server)
- **Liquidación, Nómina y Validación CST**: servicios `ILiquidacionService`, `INominaService`, `IValidacionService` para cálculo de prestaciones y cumplimiento normativo colombiano.
- **Alertas de permisos y reportes silviculturales**: `IAlertaPermisoService` y `IReporteProductividadService` para seguimiento operativo.
- **Escaneo/OCR/Impresión**: `IScannerService`, `IImageProcessingService`, `IOcrService`, `IPrintService`, `IScanProfileRepository` habilitan captura, corrección y reconocimiento de documentos.
- **Mantenimiento y respaldos**: `IDatabaseMaintenanceService` y `BackupSchedulerService` ejecutan tareas programadas de mantenimiento/backup de SQLite.
- **Capa de seguridad social y dotación**: repositorios para EPS/AFP/ARL/Cajas, dispositivos autorizados, cuentas bancarias y dotación/EPP.
- **Cache y sesión**: `ICatalogCacheService`, `ISessionService`, `INotificationService`, `IKeyboardShortcutService` optimizan UX y navegación.
- Registro de dependencias y servicios en [SGRRHH.Local/SGRRHH.Local.Server/Program.cs](SGRRHH.Local/SGRRHH.Local.Server/Program.cs#L19-L155).

## Puertos, archivos estáticos y multimedia
- El servidor selecciona puertos disponibles desde 5002/5003 si no hay configuración explícita de Kestrel, evitando conflictos en despliegues self-contained.
- Respuestas estáticas deshabilitan caché para CSS/JS en desarrollo; en producción sirven estáticos normalmente.
- Las fotos de empleados se exponen desde el almacenamiento local (`/fotos`) mediante `PhysicalFileProvider` y mapeo de MIME para PNG/JPEG.
- Implementación en [SGRRHH.Local/SGRRHH.Local.Server/Program.cs](SGRRHH.Local/SGRRHH.Local.Server/Program.cs#L19-L207).

## Licencias y dependencias especiales
- **QuestPDF** exige seleccionar licencia Community al iniciar el servidor antes de generar reportes.
- Configuración en [SGRRHH.Local/SGRRHH.Local.Server/Program.cs](SGRRHH.Local/SGRRHH.Local.Server/Program.cs#L29-L32).


## Flujo de datos (alto nivel)
1) Usuario interactúa con componentes Blazor en `SGRRHH.Local.Server`.
2) El servidor invoca servicios/repositorios (Infrastructure), usando contratos definidos en Domain/Shared.
3) Los repositorios ejecutan SQL parametrizado con Dapper sobre SQLite.
4) Los resultados viajan de regreso a la UI mediante DTOs para renderizado y validaciones.

## Configuración y entornos
- Configuración principal en `appsettings*.json` (puerto, cadenas SQLite, claves); variantes locales y de cliente en la raíz y en publicación.
- Variables de entorno controlan el modo (`Development`/`Production`) y la carga de certificados (`certs/localhost+2.p12`).
- Estilo UI debe usar variables y clases existentes en `wwwroot/css/hospital.css`; evitar estilos inline.
- Manual de usuario publicado en [SGRRHH.Local/SGRRHH.Local.Server/wwwroot/docs/manual-usuario.md](SGRRHH.Local/SGRRHH.Local.Server/wwwroot/docs/manual-usuario.md).

## Deploy y operaciones
- Deploy recomendado vía `scripts/Deploy-ToServer.ps1` (SSH a `192.168.1.248`), o `scripts/Deploy-Production.ps1` para el host local.
- Servicio `SGRRHH_Local` gestionado con `nssm`; logs en `C:\SGRRHH\logs` con rotación.
- Backups de DB mediante `scripts/Backup-Database.ps1`; limpieza controlada preserva `Data`, `certs`, `logs`.
- Actualización automática opcional: carpeta de red compartida `SGRRHHUpdates` y tarea programada que compara `version.json`, respalda DB, aplica binarios y reinicia el servicio (ver [SGRRHH.Local/ACTUALIZACION_AUTOMATICA.md](SGRRHH.Local/ACTUALIZACION_AUTOMATICA.md)).
- Scripts clave: [SGRRHH.Local/scripts/Deploy-ToServer.ps1](SGRRHH.Local/scripts/Deploy-ToServer.ps1), [SGRRHH.Local/scripts/Deploy-Production.ps1](SGRRHH.Local/scripts/Deploy-Production.ps1), [SGRRHH.Local/scripts/Backup-Database.ps1](SGRRHH.Local/scripts/Backup-Database.ps1).

## Principios de contribución
- Código y documentación en español; seguir estilo hospitalario en UI.
- Evitar dependencias adicionales en Domain/Shared; Infrastructure aísla detalles técnicos.
- Preferir SQL explícito y validaciones en servidor; mantener tests cuando cambien contratos o consultas clave.
