#define MyAppName "OpenXR Runtime Manager"
#define MyAppExeName "OpenXR-Runtime-Manager.exe"

#define MyAppVersion GetEnv("APP_VERSION")
#if MyAppVersion == ""
#define MyAppVersion "0.1.8.0"
#endif

#define SourceDir GetEnv("PUBLISH_DIR")
#if SourceDir == ""
#define SourceDir "..\artifacts\publish"
#endif

#define InstallerOutputDir GetEnv("INSTALLER_DIR")
#if InstallerOutputDir == ""
#define InstallerOutputDir "..\artifacts\installer"
#endif

[Setup]
AppId={{DD99F892-3E89-442E-B59D-D0251C2BDFC0}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher=Arthur Brainville (Ybalrid)
AppPublisherURL=https://github.com/Ybalrid/OpenXR-Runtime-Manager
AppSupportURL=https://github.com/Ybalrid/OpenXR-Runtime-Manager/issues
AppUpdatesURL=https://github.com/Ybalrid/OpenXR-Runtime-Manager/releases
DefaultDirName={autopf}\OpenXR Runtime Manager
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
OutputDir={#InstallerOutputDir}
OutputBaseFilename=OpenXR-Runtime-Manager-Setup-{#MyAppVersion}
SetupIconFile=..\OpenXR-Runtime-Manager\OpenXR-Runtime-Manager\openxr_runtime_manager_icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
WizardStyle=modern
Compression=lzma2
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon
