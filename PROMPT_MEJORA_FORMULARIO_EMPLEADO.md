# PROMPT: Mejora Integral del Formulario de Nuevo Empleado

## 📋 Contexto del Problema

El formulario de nuevo empleado (`EmpleadoForm.razor` y `EmpleadoOnboarding.razor`) es un componente crítico del sistema SGRRHH. Actualmente tiene las siguientes deficiencias:

1. **Campos de texto libre para EPS, AFP, ARL, Caja Compensación** - Deberían ser selects con catálogos oficiales
2. **Sin asociaciones inteligentes entre campos** - Departamento no filtra cargos, cargo no sugiere clase de riesgo
3. **Validaciones incompletas** - Falta validar edad mínima, fechas coherentes, etc.
4. **Texto en minúsculas** - Los campos de texto no fuerzan mayúsculas
5. **Sin autocompletado de códigos** - Al seleccionar EPS/AFP/ARL, el código debería autocompletarse

## 🎯 Objetivos

1. Crear catálogos de entidades de seguridad social colombiana (EPS, AFP, ARL, Cajas)
2. Implementar asociaciones inteligentes entre campos
3. Forzar MAYÚSCULAS en todos los campos de texto
4. Mejorar validaciones
5. Autocompletar códigos al seleccionar entidades

---

## 📦 FASE 1: Catálogos de Seguridad Social

### 1.1 Crear Entidades en Domain

**Archivo:** `SGRRHH.Local.Domain/Entities/EntidadSeguridadSocial.cs`

```csharp
namespace SGRRHH.Local.Domain.Entities;

/// <summary>
/// Clase base para entidades de seguridad social colombiana
/// </summary>
public abstract class EntidadSeguridadSocial : EntidadBase
{
    /// <summary>Código oficial ante la Superintendencia</summary>
    public string Codigo { get; set; } = string.Empty;
    
    /// <summary>Nombre oficial de la entidad</summary>
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>NIT de la entidad</summary>
    public string? Nit { get; set; }
    
    /// <summary>Si está vigente para nuevas afiliaciones</summary>
    public bool Vigente { get; set; } = true;
}

/// <summary>Entidad Promotora de Salud</summary>
public class Eps : EntidadSeguridadSocial { }

/// <summary>Administradora de Fondo de Pensiones</summary>
public class Afp : EntidadSeguridadSocial { }

/// <summary>Administradora de Riesgos Laborales</summary>
public class Arl : EntidadSeguridadSocial { }

/// <summary>Caja de Compensación Familiar</summary>
public class CajaCompensacion : EntidadSeguridadSocial { }
```

### 1.2 Migración SQL con Datos Oficiales

**Archivo:** `SGRRHH.Local/scripts/migration_catalogo_seguridad_social_v1.sql`

```sql
-- =====================================================
-- MIGRACIÓN: Catálogos de Seguridad Social Colombia
-- Fecha: 2026-01-11
-- Descripción: Crea tablas de EPS, AFP, ARL, Cajas
-- =====================================================

-- TABLA: EPS (Entidades Promotoras de Salud)
CREATE TABLE IF NOT EXISTS eps (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    nit TEXT,
    vigente INTEGER DEFAULT 1,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT (datetime('now', 'localtime'))
);

-- TABLA: AFP (Administradoras de Fondos de Pensiones)
CREATE TABLE IF NOT EXISTS afp (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    nit TEXT,
    vigente INTEGER DEFAULT 1,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT (datetime('now', 'localtime'))
);

-- TABLA: ARL (Administradoras de Riesgos Laborales)
CREATE TABLE IF NOT EXISTS arl (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    nit TEXT,
    vigente INTEGER DEFAULT 1,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT (datetime('now', 'localtime'))
);

-- TABLA: Cajas de Compensación Familiar
CREATE TABLE IF NOT EXISTS cajas_compensacion (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    codigo TEXT NOT NULL UNIQUE,
    nombre TEXT NOT NULL,
    nit TEXT,
    departamento TEXT, -- Departamento donde opera principalmente
    vigente INTEGER DEFAULT 1,
    activo INTEGER DEFAULT 1,
    fecha_creacion TEXT DEFAULT (datetime('now', 'localtime'))
);

-- =====================================================
-- DATOS: EPS VIGENTES EN COLOMBIA (2026)
-- Fuente: Superintendencia Nacional de Salud
-- =====================================================

INSERT OR IGNORE INTO eps (codigo, nombre, nit, vigente) VALUES
-- EPS del Régimen Contributivo
('EPS001', 'ALIANSALUD EPS', '830113831', 1),
('EPS002', 'SALUD TOTAL EPS', '800130907', 1),
('EPS005', 'SANITAS EPS', '800251440', 1),
('EPS008', 'COMPENSAR EPS', '860066942', 1),
('EPS010', 'SURA EPS', '800088702', 1),
('EPS012', 'COMFENALCO VALLE EPS', '890303093', 1),
('EPS013', 'SALUDVIDA EPS', '900156264', 0), -- Liquidada
('EPS016', 'COOMEVA EPS', '805000427', 0), -- Liquidada
('EPS017', 'FAMISANAR EPS', '830003564', 1),
('EPS018', 'SERVICIO OCCIDENTAL DE SALUD SOS', '805001157', 1),
('EPS023', 'CRUZ BLANCA EPS', '900074994', 0), -- Liquidada
('EPS033', 'NUEVA EPS', '900156266', 1),
('EPS037', 'NUEVA EPS (ANTES COLPATRIA)', '900156266', 0), -- Fusionada
('EPS044', 'MUTUAL SER', '806008394', 1),
('EPS045', 'COOSALUD EPS', '900226715', 1),
('EPS046', 'COMFACOR EPS', '891080005', 1);

-- =====================================================
-- DATOS: AFP VIGENTES EN COLOMBIA (2026)
-- Fuente: Superintendencia Financiera
-- =====================================================

INSERT OR IGNORE INTO afp (codigo, nombre, nit, vigente) VALUES
-- Fondos de Pensiones Privados
('230301', 'PORVENIR', '800144331', 1),
('230401', 'PROTECCION', '800138188', 1),
('231001', 'COLFONDOS', '800198281', 1),
('231301', 'SKANDIA (OLD MUTUAL)', '800184039', 1),
-- Fondo Público
('250001', 'COLPENSIONES', '900336004', 1);

-- =====================================================
-- DATOS: ARL VIGENTES EN COLOMBIA (2026)
-- Fuente: Fasecolda / Min. Trabajo
-- =====================================================

INSERT OR IGNORE INTO arl (codigo, nombre, nit, vigente) VALUES
('14-21', 'POSITIVA COMPAÑÍA DE SEGUROS', '860011153', 1),
('14-24', 'SEGUROS DE VIDA COLPATRIA', '860002184', 1),
('14-72', 'AXA COLPATRIA SEGUROS', '860002184', 1),
('14-81', 'SEGUROS BOLÍVAR', '860002503', 1),
('14-91', 'LIBERTY SEGUROS', '860039988', 1),
('14-18', 'SURA ARL', '890903790', 1),
('14-45', 'COLMENA SEGUROS', '800198591', 1),
('14-85', 'MAPFRE SEGUROS', '800221182', 1),
('14-39', 'SEGUROS DE VIDA ALFA', '860002337', 1),
('14-12', 'LA EQUIDAD SEGUROS', '860000543', 1);

-- =====================================================
-- DATOS: CAJAS DE COMPENSACIÓN VIGENTES (2026)
-- Fuente: Superintendencia del Subsidio Familiar
-- =====================================================

INSERT OR IGNORE INTO cajas_compensacion (codigo, nombre, nit, departamento, vigente) VALUES
-- Principales por departamento
('CCF01', 'CAFAM', '860013570', 'CUNDINAMARCA', 1),
('CCF02', 'COMPENSAR', '860066942', 'CUNDINAMARCA', 1),
('CCF03', 'COLSUBSIDIO', '860007336', 'CUNDINAMARCA', 1),
('CCF04', 'COMFACUNDI', '890680014', 'CUNDINAMARCA', 1),
('CCF05', 'COMFAMA', '890900842', 'ANTIOQUIA', 1),
('CCF06', 'COMFENALCO ANTIOQUIA', '890900842', 'ANTIOQUIA', 1),
('CCF07', 'COMFANDI', '890300406', 'VALLE DEL CAUCA', 1),
('CCF08', 'COMFENALCO VALLE', '890303093', 'VALLE DEL CAUCA', 1),
('CCF09', 'COMFAMILIAR ATLÁNTICO', '890102140', 'ATLÁNTICO', 1),
('CCF10', 'COMBARRANQUILLA', '890100645', 'ATLÁNTICO', 1),
('CCF11', 'COMFAMILIAR RISARALDA', '891400270', 'RISARALDA', 1),
('CCF12', 'COMFAMILIAR HUILA', '891100438', 'HUILA', 1),
('CCF13', 'COMFAMILIAR NARIÑO', '800099629', 'NARIÑO', 1),
('CCF14', 'COMFATOLIMA', '890700640', 'TOLIMA', 1),
('CCF15', 'COMFACESAR', '892300678', 'CESAR', 1),
('CCF16', 'COMFAORIENTE', '890500675', 'NORTE DE SANTANDER', 1),
('CCF17', 'COMFASUCRE', '892200015', 'SUCRE', 1),
('CCF18', 'CAJACOPI ATLÁNTICO', '890102044', 'ATLÁNTICO', 1),
('CCF19', 'COMFACOR', '891080005', 'CÓRDOBA', 1),
('CCF20', 'COMFAMILIAR CARTAGENA', '890480110', 'BOLÍVAR', 1);

-- Índices
CREATE INDEX IF NOT EXISTS idx_eps_codigo ON eps(codigo);
CREATE INDEX IF NOT EXISTS idx_eps_vigente ON eps(vigente);
CREATE INDEX IF NOT EXISTS idx_afp_codigo ON afp(codigo);
CREATE INDEX IF NOT EXISTS idx_arl_codigo ON arl(codigo);
CREATE INDEX IF NOT EXISTS idx_cajas_codigo ON cajas_compensacion(codigo);
CREATE INDEX IF NOT EXISTS idx_cajas_departamento ON cajas_compensacion(departamento);
```

### 1.3 Interfaces de Repositorios

**Archivo:** `SGRRHH.Local.Shared/Interfaces/ISeguridadSocialRepository.cs`

```csharp
using SGRRHH.Local.Domain.Entities;

namespace SGRRHH.Local.Shared.Interfaces;

public interface IEpsRepository : IRepository<Eps>
{
    Task<List<Eps>> GetVigentesAsync();
    Task<Eps?> GetByCodigoAsync(string codigo);
}

public interface IAfpRepository : IRepository<Afp>
{
    Task<List<Afp>> GetVigentesAsync();
    Task<Afp?> GetByCodigoAsync(string codigo);
}

public interface IArlRepository : IRepository<Arl>
{
    Task<List<Arl>> GetVigentesAsync();
    Task<Arl?> GetByCodigoAsync(string codigo);
}

public interface ICajaCompensacionRepository : IRepository<CajaCompensacion>
{
    Task<List<CajaCompensacion>> GetVigentesAsync();
    Task<List<CajaCompensacion>> GetByDepartamentoAsync(string departamento);
    Task<CajaCompensacion?> GetByCodigoAsync(string codigo);
}
```

### 1.4 Implementación de Repositorios

**Archivo:** `SGRRHH.Local.Infrastructure/Repositories/EpsRepository.cs`

```csharp
using Dapper;
using Microsoft.Data.Sqlite;
using SGRRHH.Local.Domain.Entities;
using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Repositories;

public class EpsRepository : IEpsRepository
{
    private readonly string _connectionString;

    public EpsRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<List<Eps>> GetVigentesAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = "SELECT * FROM eps WHERE vigente = 1 AND activo = 1 ORDER BY nombre";
        var result = await connection.QueryAsync<Eps>(sql);
        return result.ToList();
    }

    public async Task<Eps?> GetByCodigoAsync(string codigo)
    {
        using var connection = new SqliteConnection(_connectionString);
        const string sql = "SELECT * FROM eps WHERE codigo = @Codigo AND activo = 1";
        return await connection.QueryFirstOrDefaultAsync<Eps>(sql, new { Codigo = codigo });
    }

    // Implementar resto de IRepository<Eps>...
    public Task<Eps> AddAsync(Eps entity) => throw new NotImplementedException();
    public Task<Eps?> GetByIdAsync(int id) => throw new NotImplementedException();
    public Task<List<Eps>> GetAllAsync() => GetVigentesAsync();
    public Task UpdateAsync(Eps entity) => throw new NotImplementedException();
    public Task DeleteAsync(int id) => throw new NotImplementedException();
}
```

**(Repetir patrón similar para AfpRepository, ArlRepository, CajaCompensacionRepository)**

### 1.5 Actualizar CatalogCacheService

**Archivo:** `SGRRHH.Local.Infrastructure/Services/CatalogCacheService.cs`

Agregar métodos:

```csharp
// Nuevos campos privados
private List<Eps>? _epsCache;
private List<Afp>? _afpCache;
private List<Arl>? _arlCache;
private List<CajaCompensacion>? _cajasCache;

// Nuevos métodos públicos
public async Task<List<Eps>> GetEpsAsync()
{
    if (_epsCache == null)
    {
        _epsCache = await _epsRepository.GetVigentesAsync();
    }
    return _epsCache;
}

public async Task<List<Afp>> GetAfpAsync()
{
    if (_afpCache == null)
    {
        _afpCache = await _afpRepository.GetVigentesAsync();
    }
    return _afpCache;
}

public async Task<List<Arl>> GetArlAsync()
{
    if (_arlCache == null)
    {
        _arlCache = await _arlRepository.GetVigentesAsync();
    }
    return _arlCache;
}

public async Task<List<CajaCompensacion>> GetCajasCompensacionAsync()
{
    if (_cajasCache == null)
    {
        _cajasCache = await _cajaRepository.GetVigentesAsync();
    }
    return _cajasCache;
}
```

---

## 📦 FASE 2: Componente InputUpperCase

### 2.1 Crear Componente Reutilizable

**Archivo:** `SGRRHH.Local.Server/Components/Shared/InputUpperCase.razor`

```razor
@namespace SGRRHH.Local.Server.Components.Shared
@inherits InputBase<string>

<input type="text"
       id="@Id"
       class="@CssClass @CssClassFromParent"
       style="text-transform: uppercase;"
       value="@CurrentValueAsString"
       @oninput="OnInput"
       @onblur="OnBlur"
       maxlength="@MaxLength"
       placeholder="@Placeholder"
       readonly="@Readonly"
       disabled="@Disabled"
       tabindex="@TabIndex" />

@code {
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? CssClassFromParent { get; set; }
    [Parameter] public int MaxLength { get; set; } = 100;
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public int TabIndex { get; set; } = 0;
    [Parameter] public EventCallback<string> OnValueChanged { get; set; }

    protected override bool TryParseValueFromString(string? value, out string result, out string validationErrorMessage)
    {
        result = value?.ToUpperInvariant() ?? string.Empty;
        validationErrorMessage = string.Empty;
        return true;
    }

    private async Task OnInput(ChangeEventArgs e)
    {
        var value = e.Value?.ToString()?.ToUpperInvariant() ?? string.Empty;
        CurrentValueAsString = value;
        await OnValueChanged.InvokeAsync(value);
    }

    private void OnBlur(FocusEventArgs e)
    {
        // Asegurar mayúsculas al perder foco
        if (!string.IsNullOrEmpty(CurrentValueAsString))
        {
            CurrentValueAsString = CurrentValueAsString.ToUpperInvariant();
        }
    }
}
```

### 2.2 Componente InputUpperCase Simplificado (sin InputBase)

**Archivo alternativo si hay problemas con InputBase:**

```razor
@namespace SGRRHH.Local.Server.Components.Shared

<input type="text"
       id="@Id"
       class="@CssClass"
       style="text-transform: uppercase;"
       value="@Value"
       @oninput="OnInput"
       maxlength="@MaxLength"
       placeholder="@Placeholder"
       readonly="@Readonly"
       disabled="@Disabled"
       tabindex="@TabIndex" />

@code {
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public string CssClass { get; set; } = "hospital-input";
    [Parameter] public int MaxLength { get; set; } = 100;
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public int TabIndex { get; set; } = 0;

    private async Task OnInput(ChangeEventArgs e)
    {
        var newValue = e.Value?.ToString()?.ToUpperInvariant() ?? string.Empty;
        Value = newValue;
        await ValueChanged.InvokeAsync(newValue);
    }
}
```

---

## 📦 FASE 3: Asociaciones Inteligentes de Campos

### 3.1 Modificar la Entidad Cargo

Agregar campo para sugerir clase de riesgo:

**En Cargo.cs agregar:**

```csharp
/// <summary>
/// Clase de riesgo ARL sugerida para este cargo (1-5)
/// </summary>
public int ClaseRiesgoSugerida { get; set; } = 1;
```

**Migración SQL:**

```sql
-- Agregar columna a cargos
ALTER TABLE cargos ADD COLUMN clase_riesgo_sugerida INTEGER DEFAULT 1;

-- Actualizar cargos de campo/silvicultura con riesgo alto
UPDATE cargos SET clase_riesgo_sugerida = 4 
WHERE UPPER(nombre) LIKE '%OPERARIO%' 
   OR UPPER(nombre) LIKE '%CAMPO%'
   OR UPPER(nombre) LIKE '%SILVICULTOR%'
   OR UPPER(nombre) LIKE '%FORESTAL%';

UPDATE cargos SET clase_riesgo_sugerida = 5 
WHERE UPPER(nombre) LIKE '%MOTOSIERR%'
   OR UPPER(nombre) LIKE '%TALA%'
   OR UPPER(nombre) LIKE '%ALTURA%';
```

### 3.2 Actualizar EmpleadoForm.razor

**Cambios principales:**

```razor
@* SECCIÓN: Variables adicionales *@
@code {
    // Catálogos de seguridad social
    private List<Eps> EpsLista { get; set; } = new();
    private List<Afp> AfpLista { get; set; } = new();
    private List<Arl> ArlLista { get; set; } = new();
    private List<CajaCompensacion> CajasLista { get; set; } = new();
    
    // Cargos filtrados por departamento
    private List<Cargo> CargosFiltrados => Empleado?.DepartamentoId.HasValue == true
        ? Cargos.Where(c => c.DepartamentoId == Empleado.DepartamentoId || c.DepartamentoId == null).ToList()
        : Cargos;
}

@* CARGAR CATÁLOGOS *@
private async Task LoadCatalogs()
{
    try
    {
        Cargos = await CatalogCache.GetCargosAsync();
        Departamentos = await CatalogCache.GetDepartamentosAsync();
        
        // Nuevos catálogos
        EpsLista = await CatalogCache.GetEpsAsync();
        AfpLista = await CatalogCache.GetAfpAsync();
        ArlLista = await CatalogCache.GetArlAsync();
        CajasLista = await CatalogCache.GetCajasCompensacionAsync();
    }
    catch (Exception ex)
    {
        Logger.LogError(ex, "Error cargando catálogos");
    }
}

@* EVENTO: Al cambiar departamento, refrescar cargos *@
private void OnDepartamentoChanged()
{
    // Si el cargo actual no pertenece al nuevo departamento, limpiarlo
    if (Empleado?.CargoId.HasValue == true)
    {
        var cargoActual = Cargos.FirstOrDefault(c => c.Id == Empleado.CargoId);
        if (cargoActual?.DepartamentoId != Empleado.DepartamentoId && cargoActual?.DepartamentoId != null)
        {
            Empleado.CargoId = null;
            Empleado.SalarioBase = null;
        }
    }
    StateHasChanged();
}

@* EVENTO: Al cambiar cargo, actualizar salario Y clase de riesgo *@
private void OnCargoChanged()
{
    if (Empleado?.CargoId.HasValue == true)
    {
        var cargoSeleccionado = Cargos.FirstOrDefault(c => c.Id == Empleado.CargoId);
        if (cargoSeleccionado != null)
        {
            // Copiar salario
            if (cargoSeleccionado.SalarioBase.HasValue)
            {
                Empleado.SalarioBase = cargoSeleccionado.SalarioBase;
            }
            
            // Sugerir clase de riesgo
            if (cargoSeleccionado.ClaseRiesgoSugerida > 0)
            {
                Empleado.ClaseRiesgoARL = cargoSeleccionado.ClaseRiesgoSugerida;
            }
        }
    }
    StateHasChanged();
}

@* EVENTOS: Al seleccionar EPS/AFP/ARL/Caja, autocompletar código *@
private void OnEpsChanged(ChangeEventArgs e)
{
    if (int.TryParse(e.Value?.ToString(), out int epsId))
    {
        var eps = EpsLista.FirstOrDefault(x => x.Id == epsId);
        if (eps != null)
        {
            Empleado!.EPS = eps.Nombre;
            Empleado.CodigoEPS = eps.Codigo;
        }
    }
    StateHasChanged();
}

private void OnAfpChanged(ChangeEventArgs e)
{
    if (int.TryParse(e.Value?.ToString(), out int afpId))
    {
        var afp = AfpLista.FirstOrDefault(x => x.Id == afpId);
        if (afp != null)
        {
            Empleado!.AFP = afp.Nombre;
            Empleado.CodigoAFP = afp.Codigo;
        }
    }
    StateHasChanged();
}

private void OnArlChanged(ChangeEventArgs e)
{
    if (int.TryParse(e.Value?.ToString(), out int arlId))
    {
        var arl = ArlLista.FirstOrDefault(x => x.Id == arlId);
        if (arl != null)
        {
            Empleado!.ARL = arl.Nombre;
            Empleado.CodigoARL = arl.Codigo;
        }
    }
    StateHasChanged();
}

private void OnCajaChanged(ChangeEventArgs e)
{
    if (int.TryParse(e.Value?.ToString(), out int cajaId))
    {
        var caja = CajasLista.FirstOrDefault(x => x.Id == cajaId);
        if (caja != null)
        {
            Empleado!.CajaCompensacion = caja.Nombre;
            Empleado.CodigoCajaCompensacion = caja.Codigo;
        }
    }
    StateHasChanged();
}
```

---

## 📦 FASE 4: Actualizar HTML del Formulario

### 4.1 Reemplazar campos de texto con InputUpperCase

**Ejemplo para NOMBRES:**

```razor
@* ANTES *@
<input type="text" 
       id="field-nombres"
       class="hospital-input @(GetFieldErrorClass("Nombres"))"
       @bind="Empleado.Nombres" 
       @bind:event="oninput"
       maxlength="100"
       tabindex="3" />

@* DESPUÉS *@
<InputUpperCase @bind-Value="Empleado.Nombres"
                Id="field-nombres"
                CssClassFromParent="@GetFieldErrorClass("Nombres")"
                MaxLength="100"
                TabIndex="3" />
```

### 4.2 Reemplazar campos de seguridad social con selects

**Ejemplo para EPS:**

```razor
@* ANTES *@
<input type="text" class="hospital-input" 
       @bind="Empleado.EPS" maxlength="100" 
       placeholder="Ej: SURA, SANITAS, NUEVA EPS" />

@* DESPUÉS *@
<div class="hospital-form-row">
    <div class="hospital-form-field">
        <label class="hospital-label">EPS (SALUD)</label>
        <select class="hospital-input" @onchange="OnEpsChanged">
            <option value="">-- SELECCIONE EPS --</option>
            @foreach (var eps in EpsLista)
            {
                <option value="@eps.Id" selected="@(Empleado?.EPS == eps.Nombre)">
                    @eps.Nombre
                </option>
            }
        </select>
    </div>
    
    <div class="hospital-form-field">
        <label class="hospital-label">CODIGO EPS</label>
        <input type="text" class="hospital-input" 
               value="@Empleado?.CodigoEPS" 
               readonly 
               style="background-color: #f0f0f0; text-transform: uppercase;" />
        <div class="hospital-field-hint">Autocompletado</div>
    </div>
</div>
```

### 4.3 Filtrar cargos por departamento

```razor
@* ANTES *@
<select class="hospital-input" 
        id="field-cargo"
        @bind="Empleado.CargoId" 
        @bind:after="OnCargoChanged"
        tabindex="12">
    <option value="">-- SELECCIONE --</option>
    @foreach (var c in Cargos)
    {
        <option value="@c.Id">@c.Nombre</option>
    }
</select>

@* DESPUÉS *@
<select class="hospital-input" 
        id="field-cargo"
        @bind="Empleado.CargoId" 
        @bind:after="OnCargoChanged"
        tabindex="12">
    <option value="">-- SELECCIONE --</option>
    @foreach (var c in CargosFiltrados)
    {
        <option value="@c.Id">
            @c.Nombre @(c.SalarioBase.HasValue ? $"(${c.SalarioBase.Value:N0})" : "")
        </option>
    }
</select>
@if (!CargosFiltrados.Any() && Empleado?.DepartamentoId.HasValue == true)
{
    <div class="hospital-field-hint" style="color: #CC6600;">
        No hay cargos para este departamento
    </div>
}
```

---

## 📦 FASE 5: Validaciones Mejoradas

### 5.1 Agregar validaciones al método ValidateForm()

```csharp
private bool ValidateForm()
{
    FieldErrors.Clear();
    ValidationMessage = "";

    if (Empleado == null)
    {
        ValidationMessage = "Datos del empleado no disponibles.";
        return false;
    }

    // === VALIDACIONES EXISTENTES ===
    // ... (mantener las que ya existen)

    // === NUEVAS VALIDACIONES ===
    
    // Validar edad mínima (18 años)
    if (Empleado.FechaNacimiento.HasValue)
    {
        var edad = DateTime.Today.Year - Empleado.FechaNacimiento.Value.Year;
        if (Empleado.FechaNacimiento.Value.Date > DateTime.Today.AddYears(-edad))
            edad--;
        
        if (edad < 18)
        {
            FieldErrors["FechaNacimiento"] = "El empleado debe ser mayor de 18 años.";
        }
        
        if (edad > 100)
        {
            FieldErrors["FechaNacimiento"] = "La fecha de nacimiento no parece válida.";
        }
    }
    
    // Validar fecha de ingreso no futura
    if (Empleado.FechaIngreso > DateTime.Today)
    {
        FieldErrors["FechaIngreso"] = "La fecha de ingreso no puede ser futura.";
    }
    
    // Validar fecha de ingreso no muy antigua (más de 50 años)
    if (Empleado.FechaIngreso < DateTime.Today.AddYears(-50))
    {
        FieldErrors["FechaIngreso"] = "La fecha de ingreso no parece válida.";
    }
    
    // Validar fecha de retiro posterior a ingreso
    if (Empleado.FechaRetiro.HasValue && Empleado.FechaRetiro.Value <= Empleado.FechaIngreso)
    {
        FieldErrors["FechaRetiro"] = "La fecha de retiro debe ser posterior a la fecha de ingreso.";
    }
    
    // Validar salario mínimo (advertencia, no bloqueo)
    // Ya se muestra advertencia en UI, pero podemos agregar a FieldErrors si queremos bloquear
    
    // Validar coherencia: si hay fecha de retiro, el estado debería ser Retirado
    if (Empleado.FechaRetiro.HasValue && Empleado.Estado == EstadoEmpleado.Activo)
    {
        // Solo advertencia, no error
        Logger.LogWarning("Empleado tiene fecha de retiro pero estado Activo");
    }

    // Si hay errores de campo, mostrar el primero en el mensaje general
    if (FieldErrors.Any())
    {
        var firstError = FieldErrors.First();
        ValidationMessage = $"Campo {firstError.Key}: {firstError.Value}";
        return false;
    }

    return true;
}
```

---

## 📦 FASE 6: Lista de Campos a Convertir a MAYÚSCULAS

Todos los campos de texto libre deben usar `InputUpperCase` o tener `style="text-transform: uppercase;"`:

| Campo | Componente Actual | Cambiar a |
|-------|-------------------|-----------|
| Código | input text (readonly) | Agregar uppercase style |
| Nombres | input text | InputUpperCase |
| Apellidos | input text | InputUpperCase |
| Dirección | input text | InputUpperCase |
| Teléfono | input text | InputUpperCase |
| Teléfono Emergencia | input text | InputUpperCase |
| Contacto Emergencia | input text | InputUpperCase |
| Email | input email | Mantener (emails no van en mayúscula) |
| Observaciones | textarea | Agregar uppercase style |
| EPS | ahora es select | N/A |
| AFP | ahora es select | N/A |
| ARL | ahora es select | N/A |
| Caja Compensación | ahora es select | N/A |

---

## 📋 Orden de Implementación Sugerido

1. **Crear migración SQL** con tablas y datos de EPS, AFP, ARL, Cajas
2. **Crear entidades** en Domain
3. **Crear repositorios** en Infrastructure
4. **Actualizar CatalogCacheService** para cargar nuevos catálogos
5. **Crear componente InputUpperCase**
6. **Actualizar EmpleadoForm.razor**:
   - Agregar variables para nuevos catálogos
   - Agregar métodos OnXxxChanged
   - Reemplazar inputs de texto por InputUpperCase
   - Reemplazar inputs de seguridad social por selects
   - Agregar filtrado de cargos por departamento
7. **Actualizar EmpleadoOnboarding.razor** (mismos cambios)
8. **Agregar validaciones mejoradas**
9. **Probar todos los flujos**

---

## ⚠️ Consideraciones Importantes

1. **Migración de datos existentes**: Los empleados existentes tienen EPS, AFP, ARL como texto libre. La migración debe mapear los valores existentes a los nuevos catálogos donde sea posible.

2. **Permitir "Otro"**: Considerar agregar una opción "OTRO" en cada catálogo para casos no cubiertos, con un campo de texto adicional.

3. **Mantener compatibilidad**: Los campos `EPS`, `AFP`, etc. en la entidad `Empleado` seguirán siendo strings, almacenando el NOMBRE de la entidad (no el ID).

4. **Actualización de catálogos**: Las EPS, AFP, etc. cambian periódicamente en Colombia. Considerar un mecanismo de actualización.

5. **Validación de Libreta Militar**: Para hombres colombianos menores de 50 años es obligatoria. Considerar agregar esta validación.

---

*Prompt creado: 2026-01-11*
*Afecta: EmpleadoForm.razor, EmpleadoOnboarding.razor, Domain, Infrastructure*
