; ============================================================
; SGRRHH - Sistema de Gestión de Recursos Humanos
; Script de Instalación para Inno Setup 6.x
; ============================================================

#define MyAppName "SGRRHH"
#define MyAppFullName "Sistema de Gestión de Recursos Humanos"
#define MyAppVersion "1.0.1"
#define MyAppPublisher "Forestech"
#define MyAppURL "https://forestech.com"
#define MyAppExeName "SGRRHH.exe"
#define MyAppAssocName MyAppFullName
#define MyAppAssocExt ".sgrrhh"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt

[Setup]
; Información básica del instalador
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppFullName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppFullName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

; Configuración de directorios
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppFullName}
DisableProgramGroupPage=yes

; Configuración de salida
OutputDir=..\installer\output
OutputBaseFilename=SGRRHH_Setup_{#MyAppVersion}
SetupIconFile=
; Descomenta la siguiente línea si tienes un icono personalizado
; SetupIconFile=..\src\SGRRHH.WPF\Resources\app.ico

; Compresión
Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes

; Configuración visual
WizardStyle=modern
WizardSizePercent=110,110

; Privilegios de instalación
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; Información de desinstalación
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppFullName}

; Configuraciones adicionales
AllowNoIcons=yes
CloseApplications=yes
RestartApplications=yes
CloseApplicationsFilter=*.exe,*.dll

; Información legal
LicenseFile=
; Descomenta si tienes un archivo de licencia
; LicenseFile=..\docs\LICENSE.txt

; Requisitos mínimos
MinVersion=10.0.17763
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; Archivos principales de la aplicación (self-contained para que funcione sin .NET instalado)
Source: "..\src\publish\SGRRHH\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Crear carpeta de datos para la base de datos y configuración
; La base de datos se creará automáticamente al iniciar la aplicación

[Dirs]
; Crear directorios necesarios para la aplicación
Name: "{app}\data"
Name: "{app}\data\config"
Name: "{app}\data\backups"
Name: "{app}\data\logs"
Name: "{app}\data\fotos"
Name: "{app}\data\documentos"

[Icons]
; Acceso directo en el menú de inicio
Name: "{group}\{#MyAppFullName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppFullName}}"; Filename: "{uninstallexe}"

; Acceso directo en el escritorio (opcional)
Name: "{commondesktop}\{#MyAppFullName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

; Acceso rápido (Windows 7 y anteriores)
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppFullName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
; Ejecutar la aplicación después de instalar (opcional)
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppFullName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Eliminar archivos generados durante la ejecución
Type: filesandordirs; Name: "{app}\data\logs\*"
; Nota: No eliminamos data\backups ni la base de datos para preservar los datos del usuario

[Messages]
spanish.BeveledLabel=Español
english.BeveledLabel=English

[CustomMessages]
spanish.LaunchProgram=Ejecutar %1
spanish.CreateDesktopIcon=Crear un acceso directo en el &escritorio
spanish.CreateQuickLaunchIcon=Crear un acceso directo en la &barra de acceso rápido
spanish.AdditionalIcons=Iconos adicionales:
spanish.UninstallProgram=Desinstalar %1

english.LaunchProgram=Launch %1
english.CreateDesktopIcon=Create a &desktop icon
english.CreateQuickLaunchIcon=Create a &Quick Launch icon
english.AdditionalIcons=Additional icons:
english.UninstallProgram=Uninstall %1

[Code]
// Función para verificar si hay una instancia de la aplicación en ejecución
function IsAppRunning(): Boolean;
var
  WMIService: Variant;
  ProcessList: Variant;
begin
  Result := False;
  try
    WMIService := CreateOleObject('WbemScripting.SWbemLocator');
    WMIService := WMIService.ConnectServer('.', 'root\cimv2');
    ProcessList := WMIService.ExecQuery('SELECT * FROM Win32_Process WHERE Name = "SGRRHH.exe"');
    Result := (ProcessList.Count > 0);
  except
    // Si hay error con WMI, asumir que no está corriendo
    Result := False;
  end;
end;

// Función para mostrar un mensaje personalizado de bienvenida
function InitializeSetup(): Boolean;
begin
  Result := True;
  
  // Verificar si la aplicación ya está instalada y ejecutándose
  if IsAppRunning() then
  begin
    MsgBox('SGRRHH está actualmente en ejecución.' + #13#10 + 
           'Por favor, cierre la aplicación antes de continuar con la instalación.',
           mbError, MB_OK);
    Result := False;
  end;
end;

// Función que se ejecuta después de la instalación
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Aquí se pueden agregar acciones post-instalación
    // Por ejemplo, crear archivos de configuración iniciales
  end;
end;

// Función para preguntar si desea conservar los datos al desinstalar
function InitializeUninstall(): Boolean;
var
  MsgResult: Integer;
begin
  Result := True;
  
  MsgResult := MsgBox('¿Desea conservar los datos de la aplicación (base de datos, backups, configuración)?' + #13#10 +
                      'Seleccione "Sí" para conservarlos o "No" para eliminarlos.',
                      mbConfirmation, MB_YESNOCANCEL);
  
  if MsgResult = IDCANCEL then
  begin
    Result := False;
  end
  else if MsgResult = IDNO then
  begin
    // Marcar para eliminar todos los datos
    DelTree(ExpandConstant('{app}\data'), True, True, True);
  end;
end;
