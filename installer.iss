; Inno Setup Script para SGRRHH
; Este script genera un instalador EXE profesional

#define MyAppName "SGRRHH"
#define MyAppVersion "1.1.17"
#define MyAppPublisher "Forestech"
#define MyAppURL "https://github.com/evertweb/SGRRHH"
#define MyAppExeName "SGRRHH.exe"

[Setup]
; Identificador único de la aplicación (NO cambiar después de publicar)
AppId={{A7B8C9D0-E1F2-3456-7890-ABCDEF123456}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}/releases
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
; Requiere permisos de administrador para instalar en Program Files
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog
; Archivo de salida
OutputDir=.
OutputBaseFilename=SGRRHH-Setup-{#MyAppVersion}
; Compresión
Compression=lzma2/ultra64
SolidCompression=yes
; Apariencia
WizardStyle=modern
; Usar icono de la aplicación compilada (sin icono externo)
UninstallDisplayIcon={app}\{#MyAppExeName}
; Información adicional
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Sistema de Gestión de Recursos Humanos
VersionInfoProductName={#MyAppName}

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1; Check: not IsAdminInstallMode

[Files]
; Copiar todos los archivos de la aplicación
Source: "publish\SGRRHH\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTA: No incluir archivos de prueba

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Limpiar archivos de configuración local al desinstalar (opcional)
Type: filesandordirs; Name: "{localappdata}\SGRRHH"

[Code]
// Verificar si .NET 8 está instalado
function IsDotNet8Installed(): Boolean;
var
  ResultCode: Integer;
begin
  Result := Exec('dotnet', '--list-runtimes', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

procedure InitializeWizard();
begin
  // Mensaje inicial si .NET no está instalado
  if not IsDotNet8Installed() then
  begin
    MsgBox('Esta aplicación requiere .NET 8 Desktop Runtime.' + #13#10 + #13#10 +
           'Por favor, descárguelo desde:' + #13#10 +
           'https://dotnet.microsoft.com/download/dotnet/8.0', mbInformation, MB_OK);
  end;
end;
