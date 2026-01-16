# Plan de Arquitectura - EmpleadoExpediente

## Árbol de componentes (actual)
```
EmpleadoExpediente.razor (orquestador)
├─ EmpleadoHeader
│  └─ EmpleadoInfoCard
├─ TabsNavigation
├─ DatosGeneralesTab
├─ DocumentosTab
├─ InformacionBancariaTab (existente)
├─ SeguridadSocialTab (existente)
├─ ContratosTab (existente)
├─ DotacionEppTab (existente)
└─ FotoChangeModal
```

## Responsabilidades
- `EmpleadoExpediente`: carga datos, coordina tabs, scanner, printer, modales globales.
- `EmpleadoHeader`: foto y acciones principales del expediente.
- `EmpleadoInfoCard`: datos clave y selector de estado.
- `TabsNavigation`: navegación de tabs y contadores.
- `DatosGeneralesTab`: vista de datos personales/laborales (solo lectura).
- `DocumentosTab`: tabla de documentos y acciones (delegadas al orquestador).
- `FotoChangeModal`: cambio de foto (upload/eliminar) y disparo del scanner.

## Reutilización
- Tabs existentes se mantienen sin cambios:
  - `InformacionBancariaTab`
  - `SeguridadSocialTab`
  - `ContratosTab`
  - `DotacionEppTab`
- Formularios de Agente 1 no se reutilizan aquí para evitar habilitar edición en la vista de expediente (solo lectura).

## Flujos clave
- Selección de tab:
  - `TabsNavigation` dispara `OnTabChanged` → `EmpleadoExpediente.SetActiveTab` actualiza query `tab`.
- Scanner:
  - `DocumentosTab` o tabs existentes solicitan escaneo → `EmpleadoExpediente` maneja `ScannerModal` y persiste documentos.
- Foto:
  - `EmpleadoHeader` abre `FotoChangeModal`, que actualiza foto y notifica al orquestador.
