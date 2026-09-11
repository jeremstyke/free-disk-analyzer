; Inno Setup script for PurgeCore.
;
; Built by GitHub Actions (release.yml) via:
;   iscc /DSourceDir="<path to dotnet publish output>" installer\setup.iss
;
; To build locally: install Inno Setup (https://jrsoftware.org/isinfo.php),
; run `dotnet publish src\FreeDiskAnalyzer\FreeDiskAnalyzer.csproj -c Release
; -r win-x64 --self-contained true -o publish\portable`, then open this
; script in the Inno Setup Compiler (or run iscc with /DSourceDir pointing
; at that publish folder) and Build.

#ifndef SourceDir
  #define SourceDir "..\publish\portable"
#endif
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#define AppName "PurgeCore (Beta)"
#define AppPublisher "Jeremy Jury"
#define AppUrl "https://github.com/jeremstyke/purgecore"
#define AppExeName "PurgeCore.exe"

[Setup]
; Fixed AppId so upgrades replace the previous install instead of side-by-side installing.
; Kept unchanged across the Free Disk Analyzer -> PurgeCore rename so existing installs upgrade in place.
AppId={{6F3B2E8A-6C1E-4B7F-9C39-2C6A6E2D9B41}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppUrl}
AppSupportURL={#AppUrl}
AppUpdatesURL={#AppUrl}
DefaultDirName={autopf}\PurgeCore
DefaultGroupName=PurgeCore
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
OutputDir=output
OutputBaseFilename=PurgeCore-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\{#AppExeName}
SetupIconFile=..\assets\icon.ico
; No commercial code-signing certificate for v1, see README "Windows security warning".
; If the app is running (e.g. the user triggered this update from inside
; PurgeCore), close it automatically before installing over it,
; and relaunch it once the update is done. Makes "download and install"
; from inside the app a genuine one-click update rather than requiring the
; user to manually close the app first.
CloseApplications=yes
RestartApplications=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\PurgeCore"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,PurgeCore}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\PurgeCore"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,PurgeCore}"; Flags: nowait postinstall skipifsilent
