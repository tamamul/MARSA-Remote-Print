
#define AppName "MARSA Remote Print"
#define AppVersion "1.0.0"
#define AppPublisher "MARSA"
#define AppExeName "MarsaRemotePrint.exe"

[Setup]
AppId={{7A2D9B13-75A7-4B93-9D8D-2F5A3A0D1180}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\MARSA Remote Print
DefaultGroupName={#AppName}
OutputDir=..\dist
OutputBaseFilename=MARSA-Remote-Print-Setup
Compression=lzma
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
UninstallDisplayIcon={app}\{#AppExeName}

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Tasks]
Name: "desktopicon"; Description: "Buat ikon di Desktop"; GroupDescription: "Ikon tambahan:"

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Jalankan MARSA Remote Print"; Flags: postinstall nowait skipifsilent
