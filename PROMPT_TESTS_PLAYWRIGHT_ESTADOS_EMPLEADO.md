# PROMPT: Crear Tests E2E con Playwright para Sistema de Estados de Empleados

## CONTEXTO DEL PROYECTO

### Tecnología
- **Framework:** Blazor Server (.NET 8)
- **Base de datos:** SQLite (Dapper ORM)
- **Servidor:** Ejecuta en `https://localhost:5001` (desarrollo)
- **Proyecto de tests:** Crear en `SGRRHH.Local.Tests.E2E`

### Usuarios de prueba disponibles
| Usuario | Contraseña | Rol | Permisos |
|---------|------------|-----|----------|
| `secretaria` | `secretaria123` | Operador | Crear empleados (estado PendienteAprobacion), cambiar estados básicos |
| `ingeniera` | `ingeniera123` | Aprobador | Crear empleados (estado Activo), aprobar/rechazar, suspender, retirar |
| `admin` | `admin` | Administrador | Todos los permisos, incluyendo reactivar desde Suspendido |

### Estados de Empleado (Enum)
```csharp
PendienteAprobacion = 0  // Creado por Operador
Activo = 1               // Empleado en funciones
EnVacaciones = 2         // En vacaciones
EnLicencia = 3           // En licencia
Suspendido = 4           // Suspensión disciplinaria
Retirado = 5             // Desvinculado (FINAL)
Rechazado = 6            // Solicitud rechazada (FINAL)
EnIncapacidad = 7        // Incapacidad médica
```

### Rutas principales
- `/login` - Página de login
- `/empleados` - Listado de empleados (botón APROBAR visible para pendientes)
- `/empleados/onboarding` - Wizard de creación (5 pasos)
- `/empleados/{id}/expediente` - Expediente con selector de cambio de estado

### Selectores CSS importantes
- Login: `input[name="username"]`, `input[name="password"]`, `button:has-text("INGRESAR")`
- Listado: `.btn:has-text("NUEVO")`, `.btn-table:has-text("APROBAR")`, `select.filter-select`
- Onboarding: `.wizard-step`, `.btn:has-text("SIGUIENTE")`, `.btn:has-text("FINALIZAR")`
- Expediente: `.expediente-estado`, `.estado-selector select`, `.estado-selector button:has-text("APLICAR")`

---

## ESTRUCTURA DE CARPETAS A CREAR

```
SGRRHH.Local.Tests.E2E/
├── SGRRHH.Local.Tests.E2E.csproj
├── PlaywrightSetup.cs              # Configuración base, browser fixtures
├── Helpers/
│   ├── AuthHelper.cs               # Métodos para login/logout
│   ├── EmpleadoHelper.cs           # Métodos para crear empleados en wizard
│   └── TestDataHelper.cs           # Generación de datos de prueba (cédulas, nombres)
├── PageObjects/
│   ├── LoginPage.cs                # Page Object para /login
│   ├── EmpleadosPage.cs            # Page Object para /empleados
│   ├── OnboardingPage.cs           # Page Object para wizard de creación
│   └── ExpedientePage.cs           # Page Object para /empleados/{id}/expediente
├── Tests/
│   ├── CreacionEmpleado/
│   │   ├── CreacionPorOperadorTests.cs
│   │   └── CreacionPorAprobadorTests.cs
│   ├── AprobacionEmpleado/
│   │   ├── AprobacionDesdeListadoTests.cs
│   │   ├── AprobacionDesdeExpedienteTests.cs
│   │   └── RechazoEmpleadoTests.cs
│   ├── CambioEstado/
│   │   ├── TransicionesOperadorTests.cs
│   │   ├── TransicionesAprobadorTests.cs
│   │   ├── TransicionesAdminTests.cs
│   │   └── EstadosFinalesTests.cs
│   └── Permisos/
│       ├── OperadorNoApruebTests.cs
│       ├── OperadorNoSuspendeTests.cs
│       └── SoloAdminReactivaTests.cs
└── appsettings.test.json           # URL base, timeouts
```

---

## TESTS ESPECÍFICOS A IMPLEMENTAR

### 1. CreacionPorOperadorTests.cs
```csharp
[Test] Operador_CreaEmpleado_EstadoEsPendienteAprobacion()
// - Login como secretaria
// - Navegar a /empleados/onboarding
// - Completar wizard con datos válidos
// - Verificar mensaje "PENDIENTE DE APROBACIÓN"
// - Ir a /empleados, filtrar por "PENDIENTES"
// - Verificar empleado aparece en listado
// - Verificar NO hay botón APROBAR visible (secretaria no puede aprobar)

[Test] Operador_NoVeSelectorEstadoEnCreacion()
// - Login como secretaria
// - Navegar a wizard
// - En paso 3 (Laboral), verificar campo estado es READONLY
// - Verificar texto muestra "PENDIENTE DE APROBACIÓN"
```

### 2. CreacionPorAprobadorTests.cs
```csharp
[Test] Aprobador_CreaEmpleado_EstadoEsActivo()
// - Login como ingeniera
// - Crear empleado vía wizard
// - Verificar mensaje "Empleado creado correctamente"
// - Verificar empleado aparece con estado ACTIVO en listado

[Test] Admin_CreaEmpleado_EstadoEsActivo()
// - Login como admin
// - Crear empleado
// - Verificar estado ACTIVO
```

### 3. AprobacionDesdeListadoTests.cs
```csharp
[Test] Aprobador_VeBotonAprobar_EnEmpleadosPendientes()
// - Pre: Crear empleado con secretaria (pendiente)
// - Login como ingeniera
// - Navegar a /empleados
// - Verificar botón APROBAR visible en fila del empleado

[Test] Aprobador_ApruebaDesdeLlistado_EmpleadoPasaAActivo()
// - Pre: Crear empleado pendiente
// - Login como ingeniera
// - Click en APROBAR
// - Verificar mensaje éxito
// - Verificar estado cambia a ACTIVO en UI
// - Verificar botón APROBAR desaparece

[Test] Operador_NoVeBotonAprobar()
// - Pre: Crear empleado pendiente (con otro usuario)
// - Login como secretaria
// - Verificar NO existe botón APROBAR en la fila
```

### 4. TransicionesOperadorTests.cs
```csharp
[Test] Operador_PuedeCambiar_ActivoAEnVacaciones()
// - Pre: Empleado activo existe
// - Login secretaria
// - Ir a expediente
// - Verificar selector tiene opción "En Vacaciones"
// - Seleccionar y aplicar
// - Verificar estado cambia en UI

[Test] Operador_PuedeCambiar_EnVacacionesAActivo()
// Similar al anterior, retorno

[Test] Operador_PuedeCambiar_ActivoAEnIncapacidad()
[Test] Operador_PuedeCambiar_EnIncapacidadAActivo()
[Test] Operador_PuedeCambiar_ActivoAEnLicencia()
[Test] Operador_PuedeCambiar_EnLicenciaAActivo()

[Test] Operador_NoPuedeSuspender_OpcionNoDisponible()
// - Pre: Empleado activo
// - Login secretaria
// - Ir a expediente
// - Verificar selector NO tiene opción "Suspendido"

[Test] Operador_NoPuedeRetirar_OpcionNoDisponible()
// - Verificar selector NO tiene opción "Retirado"
```

### 5. TransicionesAprobadorTests.cs
```csharp
[Test] Aprobador_PuedeSuspender()
// - Login ingeniera
// - Ir a expediente de empleado activo
// - Verificar opción Suspendido disponible
// - Aplicar cambio
// - Verificar estado

[Test] Aprobador_PuedeRetirar()
[Test] Aprobador_NoPuedeReactivarDesdeSuspendido()
// - Pre: Empleado suspendido
// - Login ingeniera
// - Ir a expediente
// - Verificar selector NO tiene "Activo" como opción
```

### 6. TransicionesAdminTests.cs
```csharp
[Test] Admin_PuedeReactivarDesdeSuspendido()
// - Pre: Empleado suspendido
// - Login admin
// - Ir a expediente
// - Verificar opción Activo disponible
// - Aplicar cambio
// - Verificar estado cambia a Activo

[Test] Admin_TieneTodasLasTransiciones()
// - Verificar admin ve todas las opciones posibles según estado
```

### 7. EstadosFinalesTests.cs
```csharp
[Test] Retirado_EsEstadoFinal_SinOpciones()
// - Pre: Empleado retirado
// - Login admin
// - Ir a expediente
// - Verificar selector está vacío o no aparece

[Test] Rechazado_EsEstadoFinal_SinOpciones()
// - Pre: Empleado rechazado
// - Verificar sin transiciones disponibles
```

### 8. RechazoEmpleadoTests.cs
```csharp
[Test] Aprobador_PuedeRechazar_DesdeExpediente()
// - Pre: Empleado pendiente
// - Login ingeniera
// - Ir a expediente
// - Verificar opción "Rechazado"
// - Aplicar
// - Verificar estado final

[Test] Operador_NoPuedeRechazar()
// - Login secretaria
// - Verificar opción no disponible
```

---

## CONFIGURACIÓN DEL PROYECTO

### SGRRHH.Local.Tests.E2E.csproj
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Playwright" Version="1.40.0" />
    <PackageReference Include="Microsoft.Playwright.NUnit" Version="1.40.0" />
    <PackageReference Include="NUnit" Version="4.0.1" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.5.0" />
  </ItemGroup>
</Project>
```

### PlaywrightSetup.cs (Base fixture)
```csharp
public class PlaywrightSetup : PageTest
{
    protected string BaseUrl => "https://localhost:5001";
    
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true, // Para certificados dev
            Locale = "es-CO",
            TimezoneId = "America/Bogota"
        };
    }
}
```

---

## MATRIZ DE TRANSICIONES PARA VALIDAR

| Estado Origen | Estado Destino | Operador | Aprobador | Admin |
|---------------|----------------|----------|-----------|-------|
| PendienteAprobacion | Activo | ❌ | ✅ | ✅ |
| PendienteAprobacion | Rechazado | ❌ | ✅ | ✅ |
| Activo | EnVacaciones | ✅ | ✅ | ✅ |
| Activo | EnLicencia | ✅ | ✅ | ✅ |
| Activo | EnIncapacidad | ✅ | ✅ | ✅ |
| Activo | Suspendido | ❌ | ✅ | ✅ |
| Activo | Retirado | ❌ | ✅ | ✅ |
| EnVacaciones | Activo | ✅ | ✅ | ✅ |
| EnLicencia | Activo | ✅ | ✅ | ✅ |
| EnIncapacidad | Activo | ✅ | ✅ | ✅ |
| Suspendido | Activo | ❌ | ❌ | ✅ |
| Suspendido | Retirado | ❌ | ✅ | ✅ |
| Retirado | - | ❌ | ❌ | ❌ |
| Rechazado | - | ❌ | ❌ | ❌ |

---

## ORDEN DE IMPLEMENTACIÓN 

1. **Fase 1:** Estructura base
   - Crear proyecto y configuración
   - Implementar PlaywrightSetup
   - Implementar AuthHelper y LoginPage

2. **Fase 2:** Page Objects
   - EmpleadosPage
   - OnboardingPage  
   - ExpedientePage

3. **Fase 3:** Tests de creación
   - CreacionPorOperadorTests
   - CreacionPorAprobadorTests

4. **Fase 4:** Tests de aprobación
   - AprobacionDesdeListadoTests
   - RechazoEmpleadoTests

5. **Fase 5:** Tests de transiciones
   - TransicionesOperadorTests
   - TransicionesAprobadorTests
   - TransicionesAdminTests

6. **Fase 6:** Tests de permisos y estados finales
   - EstadosFinalesTests
   - Tests de permisos negativos

---

## NOTAS IMPORTANTES

1. **Antes de cada test:** Limpiar datos de prueba o usar cédulas únicas generadas
2. **Base URL:** Configurar en appsettings.test.json, el servidor debe estar corriendo
3. **Modo Corporativo:** Debe estar ACTIVO (valor 1) en la BD para que apliquen restricciones de roles
4. **Timeouts:** Usar waits explícitos para elementos dinámicos de Blazor
5. **Screenshots:** Capturar en fallos para debugging
6. **Prerequisito:** Ejecutar `playwright install` después de crear el proyecto

---

## FLUJO COMPLETO A TESTEAR (Escenario principal)

```
1. secretaria login
2. secretaria crea empleado "Juan Test" → Estado: PendienteAprobacion
3. secretaria logout

4. ingeniera login
5. ingeniera ve botón APROBAR en listado
6. ingeniera aprueba → Estado: Activo
7. ingeniera va a expediente
8. ingeniera cambia a EnVacaciones → Estado: EnVacaciones
9. ingeniera cambia a Activo → Estado: Activo
10. ingeniera suspende → Estado: Suspendido
11. ingeniera intenta reactivar → NO puede (opción no disponible)
12. ingeniera logout

13. admin login
14. admin va a expediente
15. admin reactiva → Estado: Activo
16. admin retira → Estado: Retirado (FINAL)
17. admin verifica sin opciones de transición
18. admin logout
```

EL MCP DE playwright ESTA INSTALADO 

---

*Última actualización: Enero 2026*
