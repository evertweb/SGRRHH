---
name: playwright-e2e
description: Creación y ejecución de tests E2E con Playwright para SGRRHH. Usar al crear tests, verificar funcionalidades o hacer testing automatizado.
license: MIT
compatibility: opencode
metadata:
  project: SGRRHH
  testing-framework: playwright
  language: csharp
  test-type: e2e
---
# Tests E2E con Playwright - SGRRHH

## Proyecto de Tests

**Ubicación:** `SGRRHH.Local.Tests.E2E/`

## Estructura del Proyecto

```
SGRRHH.Local.Tests.E2E/
├── PlaywrightSetup.cs           # Fixture base para todos los tests
├── appsettings.test.json        # Configuración (URL base, timeouts)
├── Helpers/
│   ├── AuthHelper.cs            # Login/logout reutilizable
│   ├── EmpleadoHelper.cs        # Crear empleados en wizard
│   └── TestDataHelper.cs        # Generación de datos (cédulas, nombres)
├── PageObjects/
│   ├── LoginPage.cs             # Page Object para /login
│   ├── EmpleadosPage.cs         # Page Object para /empleados
│   ├── OnboardingPage.cs        # Page Object para wizard creación
│   └── ExpedientePage.cs        # Page Object para /empleados/{id}/expediente
└── Tests/
    ├── CreacionEmpleado/
    ├── AprobacionEmpleado/
    ├── CambioEstado/
    └── Permisos/
```

## Usuarios de Prueba

| Usuario | Contraseña | Rol | Permisos |
|---------|------------|-----|----------|
| `secretaria` | `secretaria123` | Operador | Crear (PendienteAprobacion), estados básicos |
| `ingeniera` | `ingeniera123` | Aprobador | Crear (Activo), aprobar/rechazar, suspender |
| `admin` | `admin` | Administrador | Todos los permisos, reactivar |

## URL Base

| Entorno | URL |
|---------|-----|
| Desarrollo | `https://localhost:5001` |
| Producción | `https://192.168.1.248:5001` |

Configurar en: `appsettings.test.json`

## Comandos de Ejecución

### Instalar Browsers (Primera vez)
```powershell
cd SGRRHH.Local.Tests.E2E
npx playwright install
```

### Ejecutar Todos los Tests
```powershell
dotnet test SGRRHH.Local.Tests.E2E
```

### Ejecutar Tests Específicos
```powershell
# Por nombre
dotnet test --filter "FullyQualifiedName~CreacionEmpleado"

# Por categoría
dotnet test --filter "Category=Smoke"

# Test individual
dotnet test --filter "Name=Operador_CreaEmpleado_EstadoEsPendienteAprobacion"
```

### Ejecutar con UI (Debug)
```powershell
$env:HEADED = "true"
dotnet test --filter "Name=MiTest"
```

## Patrón para Nuevos Tests

### 1. Heredar de PlaywrightSetup
```csharp
[TestFixture]
public class MisTests : PlaywrightSetup
{
    [Test]
    public async Task Test_Descripcion()
    {
        // Arrange
        await AuthHelper.LoginAsync(Page, "secretaria", "secretaria123");
        
        // Act
        await Page.GotoAsync($"{BaseUrl}/empleados");
        await Page.ClickAsync(".btn:has-text('NUEVO')");
        
        // Assert
        await Expect(Page.Locator(".wizard-step")).ToBeVisibleAsync();
    }
}
```

### 2. Usar Page Objects
```csharp
var loginPage = new LoginPage(Page);
await loginPage.LoginAsync("ingeniera", "ingeniera123");

var empleadosPage = new EmpleadosPage(Page);
await empleadosPage.AbrirOnboarding();
```

### 3. Generar Datos Únicos
```csharp
var cedula = TestDataHelper.GenerarCedula(); // "1234567890"
var nombre = TestDataHelper.GenerarNombre(); // "Test_20260115_143025"
```

## Selectores CSS Importantes

### Login
| Elemento | Selector |
|----------|----------|
| Usuario | `input[name="username"]` |
| Contraseña | `input[name="password"]` |
| Botón Ingresar | `button:has-text("INGRESAR")` |

### Listado Empleados
| Elemento | Selector |
|----------|----------|
| Botón Nuevo | `.btn:has-text("NUEVO")` |
| Botón Aprobar | `.btn-table:has-text("APROBAR")` |
| Filtro | `select.filter-select` |

### Wizard Onboarding
| Elemento | Selector |
|----------|----------|
| Paso actual | `.wizard-step` |
| Siguiente | `.btn:has-text("SIGUIENTE")` |
| Finalizar | `.btn:has-text("FINALIZAR")` |

### Expediente
| Elemento | Selector |
|----------|----------|
| Estado actual | `.expediente-estado` |
| Selector estado | `.estado-selector select` |
| Aplicar cambio | `.estado-selector button:has-text("APLICAR")` |

## Buenas Prácticas

1. **Waits Explícitos:** Blazor es asíncrono, usar `WaitForAsync`
   ```csharp
   await Page.WaitForSelectorAsync(".resultado", new() { State = WaitForSelectorState.Visible });
   ```

2. **Datos Únicos:** Generar cédulas únicas para evitar conflictos
   
3. **Screenshots en Fallos:**
   ```csharp
   [TearDown]
   public async Task TearDown()
   {
       if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
       {
           await Page.ScreenshotAsync(new() { Path = $"fail_{TestContext.CurrentContext.Test.Name}.png" });
       }
   }
   ```

4. **Modo Corporativo:** Debe estar ACTIVO (valor 1) en BD para restricciones de roles

## Matriz de Transiciones de Estado

| Origen | Destino | Operador | Aprobador | Admin |
|--------|---------|----------|-----------|-------|
| PendienteAprobacion | Activo | ❌ | ✅ | ✅ |
| PendienteAprobacion | Rechazado | ❌ | ✅ | ✅ |
| Activo | EnVacaciones | ✅ | ✅ | ✅ |
| Activo | EnLicencia | ✅ | ✅ | ✅ |
| Activo | EnIncapacidad | ✅ | ✅ | ✅ |
| Activo | Suspendido | ❌ | ✅ | ✅ |
| Activo | Retirado | ❌ | ✅ | ✅ |
| Suspendido | Activo | ❌ | ❌ | ✅ |
| Retirado | - | ❌ | ❌ | ❌ |
| Rechazado | - | ❌ | ❌ | ❌ |

## Prerequisitos

1. Servidor SGRRHH corriendo en `localhost:5001`
2. Browsers instalados (`playwright install`)
3. BD con usuarios de prueba creados
4. Modo Corporativo activado (si se prueban permisos)