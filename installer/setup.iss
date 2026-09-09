; Inno Setup script for Free Disk Analyzer.
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

#define AppName "Free Disk Analyzer"
#define AppPublisher "Jeremy Jury"
#define AppUrl "https://github.com/jeremstyke/free-disk-analyzer"
#define AppExeName "FreeDiskAnalyzer.exe"

[Setup]
; Fixed AppId so upgrades replace the previous install instead of side-by-side installing.
AppId={{6F3B2E8A-6C1E-4B7F-9C39-2C6A6E2D9B41}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppUrl}
AppSupportURL={#AppUrl}
AppUpdatesURL={#AppUrl}
DefaultDirName={autopf}\Free Disk Analyzer
DefaultGroupName=Free Disk Analyzer
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
OutputDir=output
OutputBaseFilename=FreeDiskAnalyzer-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\{#AppExeName}
SetupIconFile=..\assets\icon.ico
; No commercial code-signing certificate for v1, see README "Windows security warning".

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Free Disk Analyzer"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,Free Disk Analyzer}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Free Disk Analyzer"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,Free Disk Analyzer}"; Flags: nowait postinstall skipifsilent
