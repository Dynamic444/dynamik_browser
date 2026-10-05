#define MyAppName "QuickBrowse"
#ifndef MyAppVersion
  #define MyAppVersion "1.0.1"
#endif
#define MyAppPublisher "QuickBrowse"
#define MyAppExeName "QuickBrowse.exe"
#define PublishDir "bin\Release\net8.0-windows\win-x64\publish"

[Setup]
AppId={{B6D5268C-61E0-4A25-9081-2180F0111AF4}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={localappdata}\Programs\QuickBrowse
DefaultGroupName=QuickBrowse
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=lowest
WizardStyle=modern
CloseApplications=yes
RestartApplications=no
Compression=lzma2
SolidCompression=yes
OutputDir=installer
OutputBaseFilename=QuickBrowse-Setup-{#MyAppVersion}
SetupLogging=yes

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "Создать ярлык на рабочем столе"; GroupDescription: "Дополнительные ярлыки:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\QuickBrowse"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\QuickBrowse"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Запустить QuickBrowse"; Flags: postinstall nowait skipifsilent
