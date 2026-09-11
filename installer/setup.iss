; ============================================================
;  Script de Inno Setup para "Lista de Tareas" (TodoApp)
;  Genera un instalador .exe para Windows.
;
;  Antes de compilar este script, publica la app con:
;     dotnet publish -c Release -r win-x64 --self-contained true -o publish
;  Eso crea la carpeta "publish" con TodoApp.exe y sus archivos.
; ============================================================

; --- Datos de la aplicación (edítalos si quieres) ---
#define MiApp        "Lista de Tareas"
#define MiVersion    "1.0.0"
#define MiAutor      "Pasantías"
#define MiEjecutable "TodoApp.exe"

[Setup]
; AppId identifica de forma única a la app (para actualizaciones/desinstalación).
; Puedes dejar este GUID o generar uno nuevo en el menú Tools de Inno Setup.
AppId={{8F3B1E42-9C7A-4D51-B6E2-1A2C3D4E5F60}
AppName={#MiApp}
AppVersion={#MiVersion}
AppPublisher={#MiAutor}

; Carpeta donde se instala (Archivos de programa). "TodoApp" es la subcarpeta.
DefaultDirName={autopf}\TodoApp
DefaultGroupName={#MiApp}

; Nombre y ubicación del instalador que se generará
OutputBaseFilename=ListaDeTareas-Instalador-{#MiVersion}
OutputDir=Output

; Compresión del instalador
Compression=lzma2
SolidCompression=yes

; Ícono del instalador y requisitos
SetupIconFile=..\Assets\app-icon.ico
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible

; Idioma del asistente de instalación
[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

; Opción para que el usuario decida si crea un ícono en el escritorio
[Tasks]
Name: "desktopicon"; Description: "Crear un acceso directo en el escritorio"; GroupDescription: "Accesos directos adicionales:"

; --- Archivos a instalar ---
; Copia TODO el contenido de la carpeta "publish" a la carpeta de instalación.
[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

; --- Accesos directos (menú inicio y escritorio) ---
[Icons]
Name: "{group}\{#MiApp}";           Filename: "{app}\{#MiEjecutable}"
Name: "{group}\Desinstalar {#MiApp}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MiApp}";     Filename: "{app}\{#MiEjecutable}"; Tasks: desktopicon

; --- Ofrecer abrir la app al terminar de instalar ---
[Run]
Filename: "{app}\{#MiEjecutable}"; Description: "Iniciar {#MiApp} ahora"; Flags: nowait postinstall skipifsilent
