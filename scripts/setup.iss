; =============================================================================
; setup.iss — Inno Setup 6 · Script completo y parametrizado
; =============================================================================
;
; Todos los parámetros críticos se inyectan por línea de comandos via /D
; para que el mismo .iss funcione en cualquier release sin edición manual.
;
; Ejemplo de compilación manual:
;   ISCC.exe ^
;     /DAppName=HIS2026                        ^
;     /DVersion=v1.2.3                         ^
;     /DSourceDir=C:\artifacts\release         ^
;     /DOutputDir=C:\artifacts\installer       ^
;     /DOutputBaseFilename=HIS2026_v1.2.3      ^
;     installer\setup.iss
;
; Modo automático (pipeline): OutputBaseFilename = AppName + "_" + Version
; Modo manual:                OutputBaseFilename = valor explícito /D
; =============================================================================


; ============================================================================
; SECCIÓN 0 — Validación de defines requeridos
; ============================================================================
#ifndef AppName
  #error "AppName es requerido. Pasar /DAppName=NombreApp"
#endif
#ifndef Version
  #error "Version es requerida. Pasar /DVersion=v1.0.0"
#endif
#ifndef SourceDir
  #error "SourceDir es requerido. Pasar /DSourceDir=C:\ruta\release"
#endif
#ifndef OutputDir
  #error "OutputDir es requerido. Pasar /DOutputDir=C:\ruta\salida"
#endif

; OutputBaseFilename es OPCIONAL: si no se pasa, se construye automáticamente
#ifndef OutputBaseFilename
  #define OutputBaseFilename AppName + "_" + Version
#endif


; ============================================================================
; SECCIÓN 1 — Constantes derivadas y configuración general
; ============================================================================

; Quitar la 'v' inicial para campos numéricos (v1.2.3 -> 1.2.3)
#define VersionNumeric  Copy(Version, 2, Len(Version) - 1)

; Información de la empresa / producto — EDITAR SEGÚN PROYECTO
#define Publisher          "Your Company S.A."
#define PublisherURL       "https://www.yourcompany.com"
#define SupportURL         "https://support.yourcompany.com"
#define UpdatesURL         "https://www.yourcompany.com/updates"

; Rutas de assets — relativas a la ubicación de este .iss
#define SetupIconFile      "assets\setup.ico"
#define WizardImageFile    "assets\wizard_banner.bmp"   ; 164 x 314 px
#define WizardSmallImage   "assets\wizard_small.bmp"    ; 55 x 58 px
#define LicenseFile        "{#SourceDir}\LICENSE.txt"

; Ejecutable principal
#define MainExe            AppName + ".exe"

; AppId UNICO por aplicación — generar nuevo GUID para cada proyecto distinto
; Herramienta online: https://www.guidgenerator.com/
#define AppGUID            "{{6F3A2B1C-D4E5-4F67-89AB-CDEF01234567}}"


; ============================================================================
; SECCIÓN 2 — [Setup]
; ============================================================================
[Setup]
; --- Identidad ---
AppId                        = {#AppGUID}
AppName                      = {#AppName}
AppVersion                   = {#Version}
AppVerName                   = {#AppName} {#Version}
AppPublisher                 = {#Publisher}
AppPublisherURL              = {#PublisherURL}
AppSupportURL                = {#SupportURL}
AppUpdatesURL                = {#UpdatesURL}
VersionInfoVersion           = {#VersionNumeric}
VersionInfoProductName       = {#AppName}
VersionInfoProductVersion    = {#VersionNumeric}
VersionInfoCompany           = {#Publisher}
VersionInfoDescription       = {#AppName} Setup

; --- Instalación ---
DefaultDirName               = {autopf}\{#AppName}
DefaultGroupName             = {#AppName}
AllowNoIcons                 = yes
DisableProgramGroupPage      = yes
PrivilegesRequired           = admin
PrivilegesRequiredOverridesAllowed = dialog

; --- Arquitectura ---
ArchitecturesInstallIn64BitMode = x64compatible
MinVersion                   = 10.0.17763

; --- Output ---
OutputDir                    = {#OutputDir}
OutputBaseFilename           = {#OutputBaseFilename}
SetupIconFile                = {#SetupIconFile}
WizardImageFile              = {#WizardImageFile}
WizardSmallImageFile         = {#WizardSmallImage}
WizardStyle                  = modern
WizardResizable              = no

; --- Licencia ---
LicenseFile                  = {#LicenseFile}

; --- Compresión LZMA2 maxima ---
Compression                  = lzma2/ultra64
SolidCompression             = yes
LZMAUseSeparateProcess       = yes
LZMANumBlockThreads          = 4
LZMADictionarySize           = 1048576

; --- Uninstall ---
UninstallDisplayIcon         = {app}\{#MainExe}
UninstallDisplayName         = {#AppName} {#Version}
CreateUninstallRegKey        = yes

; --- Firma digital (descomentar cuando haya certificado disponible) ---
; SignTool = signtool
;   sign /fd SHA256
;   /tr http://timestamp.sectigo.com /td SHA256
;   /f "$(CERT_FILE)"
;   /p "$(CERT_PASSWORD)"
;   $f

; --- Otros ---
ShowLanguageDialog           = auto
ChangesAssociations          = no
RestartIfNeededByRun         = no


; ============================================================================
; SECCIÓN 3 — [Languages]
; ============================================================================
[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"


; ============================================================================
; SECCIÓN 4 — [CustomMessages] — Textos personalizados multilenguaje
; ============================================================================
[CustomMessages]
spanish.LaunchAppNow=Iniciar {#AppName} ahora
english.LaunchAppNow=Launch {#AppName} now
spanish.CreateDesktopShortcut=Crear acceso directo en el escritorio
english.CreateDesktopShortcut=Create desktop shortcut
spanish.AppIsRunning={#AppName} está en ejecución. Por favor ciérrelo antes de continuar.
english.AppIsRunning={#AppName} is running. Please close it before continuing.
spanish.UninstallPrevious=Se encontró la versión anterior %1. Se desinstalará antes de continuar.%n¿Desea continuar?
english.UninstallPrevious=A previous version (%1) was found and will be uninstalled first.%nDo you want to continue?


; ============================================================================
; SECCIÓN 5 — [Tasks]
; ============================================================================
[Tasks]
Name: "desktopicon";
  Description: "{cm:CreateDesktopShortcut}";
  GroupDescription: "{cm:AdditionalIcons}";
  Flags: unchecked


; ============================================================================
; SECCIÓN 6 — [Dirs] — Directorios adicionales en {app}
; ============================================================================
[Dirs]
Name: "{app}\logs";   Permissions: users-full
Name: "{app}\data";   Permissions: users-full
Name: "{app}\config"; Permissions: users-full


; ============================================================================
; SECCIÓN 7 — [Files]
; ============================================================================
[Files]
; Todos los archivos del release folder (recursivo)
; Los .pdb y .xml de IntelliSense quedan excluidos (ya eliminados por Remove-MergedFiles.ps1)
Source: "{#SourceDir}\*";
  DestDir: "{app}";
  Flags: ignoreversion recursesubdirs createallsubdirs;
  Excludes: "*.pdb,*.xml,*.log"

; Archivos de configuración: no sobreescribir si el usuario ya los modificó
Source: "{#SourceDir}\config\*.json";
  DestDir: "{app}\config";
  Flags: ignoreversion onlyifdoesntexist


; ============================================================================
; SECCIÓN 8 — [Registry]
; ============================================================================
[Registry]
; Ruta de instalación accesible para herramientas externas
Root: HKLM;
  Subkey: "Software\{#Publisher}\{#AppName}";
  ValueType: string; ValueName: "InstallPath";
  ValueData: "{app}";
  Flags: createvalueifdoesntexist uninsdeletekey

Root: HKLM;
  Subkey: "Software\{#Publisher}\{#AppName}";
  ValueType: string; ValueName: "Version";
  ValueData: "{#Version}";
  Flags: createvalueifdoesntexist


; ============================================================================
; SECCIÓN 9 — [Icons]
; ============================================================================
[Icons]
Name: "{group}\{#AppName}";
  Filename: "{app}\{#MainExe}";
  WorkingDir: "{app}"

Name: "{group}\Desinstalar {#AppName}";
  Filename: "{uninstallexe}"

Name: "{autodesktop}\{#AppName}";
  Filename: "{app}\{#MainExe}";
  WorkingDir: "{app}";
  Tasks: desktopicon


; ============================================================================
; SECCIÓN 10 — [Run] — Acciones post-instalación
; ============================================================================
[Run]
Filename: "{app}\{#MainExe}";
  Description: "{cm:LaunchAppNow}";
  WorkingDir: "{app}";
  Flags: nowait postinstall skipifsilent unchecked


; ============================================================================
; SECCIÓN 11 — [UninstallRun]
; ============================================================================
[UninstallRun]
; Detener servicio Windows si aplica — descomentar y ajustar
; Filename: "net.exe"; Parameters: "stop {#AppName}"; Flags: runhidden waituntilterminated


; ============================================================================
; SECCIÓN 12 — [UninstallDelete]
; ============================================================================
[UninstallDelete]
Type: filesandordirs; Name: "{app}\logs"
Type: filesandordirs; Name: "{localappdata}\{#AppName}"


; ============================================================================
; SECCIÓN 13 — [Code] — Lógica Pascal personalizada
; ============================================================================
[Code]

// ---------------------------------------------------------------------------
// Mutex para detectar si la app está corriendo
// ---------------------------------------------------------------------------
const
  APP_MUTEX = 'Global\{#AppName}_RunningInstance';

// ---------------------------------------------------------------------------
// NeedsAddPath: para agregar {app} al PATH del sistema (si se activa)
// ---------------------------------------------------------------------------
function NeedsAddPath(Param: string): boolean;
var
  OrigPath: string;
begin
  if not RegQueryStringValue(
    HKEY_LOCAL_MACHINE,
    'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
    'Path', OrigPath)
  then begin
    Result := True;
    exit;
  end;
  Result := Pos(';' + Uppercase(Param) + ';',
                ';' + Uppercase(OrigPath) + ';') = 0;
end;

// ---------------------------------------------------------------------------
// GetUninstallString: busca la cadena de desinstalación de versión previa
// ---------------------------------------------------------------------------
function GetUninstallString(): string;
var
  sUnInstPath: string;
  sUnInstallString: string;
begin
  sUnInstPath := ExpandConstant(
    'Software\Microsoft\Windows\CurrentVersion\Uninstall\{#AppGUID}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;

// ---------------------------------------------------------------------------
// GetInstalledVersion: lee la versión instalada desde el registro
// ---------------------------------------------------------------------------
function GetInstalledVersion(): string;
var
  sVersion: string;
begin
  sVersion := '';
  RegQueryStringValue(
    HKLM,
    'Software\{#Publisher}\{#AppName}',
    'Version',
    sVersion);
  Result := sVersion;
end;

// ---------------------------------------------------------------------------
// InitializeSetup
// ---------------------------------------------------------------------------
function InitializeSetup(): Boolean;
var
  sUnInstallString : string;
  iResultCode      : integer;
  sInstalledVer    : string;
  bProceed         : Boolean;
begin
  Result := True;

  // 1. Bloquear si la app está ejecutándose
  if CheckForMutexes(APP_MUTEX) then begin
    MsgBox(ExpandConstant('{cm:AppIsRunning}'), mbError, MB_OK);
    Result := False;
    Exit;
  end;

  // 2. Detectar versión previa
  sUnInstallString := GetUninstallString();
  if sUnInstallString = '' then Exit;

  sInstalledVer := GetInstalledVersion();
  if sInstalledVer = '' then sInstalledVer := '(desconocida)';

  // 3. Preguntar al usuario
  bProceed := MsgBox(
    FmtMessage(ExpandConstant('{cm:UninstallPrevious}'), [sInstalledVer]),
    mbConfirmation, MB_YESNO) = IDYES;

  if not bProceed then begin
    Result := False;
    Exit;
  end;

  // 4. Desinstalar silenciosamente
  sUnInstallString := RemoveQuotes(sUnInstallString);
  Exec(sUnInstallString,
       '/SILENT /NORESTART /SUPPRESSMSGBOXES',
       '', SW_HIDE, ewWaitUntilTerminated, iResultCode);

  // Pausa para que el desinstalador libere handles
  Sleep(1500);
end;

// ---------------------------------------------------------------------------
// InitializeWizard: personalizar UI del wizard
// ---------------------------------------------------------------------------
procedure InitializeWizard();
begin
  WizardForm.Caption := '{#AppName} {#Version} — Asistente de instalación';
end;

// ---------------------------------------------------------------------------
// CurStepChanged: hooks en pasos específicos
// ---------------------------------------------------------------------------
procedure CurStepChanged(CurStep: TSetupStep);
begin
  case CurStep of
    ssInstall:
      Log('[Step] Iniciando copia de archivos...');
    ssPostInstall:
      Log('[Step] Archivos copiados. Ejecutando configuración post-instalación...');
    ssDone:
      Log('[Step] ' + '{#AppName} {#Version}' + ' instalado correctamente.');
  end;
end;

// ---------------------------------------------------------------------------
// CurUninstallStepChanged: hooks durante desinstalación
// ---------------------------------------------------------------------------
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  case CurUninstallStep of
    usUninstall:
      Log('[Uninstall] Eliminando archivos de {#AppName}...');
    usDone:
      Log('[Uninstall] {#AppName} desinstalado correctamente.');
  end;
end;
