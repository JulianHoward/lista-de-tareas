# Lista de Tareas — Guía completa del proyecto

Aplicación de escritorio **To-Do List** hecha en **.NET 8 + Avalonia UI**, con base de datos **SQLite**, empaquetada en un instalador de Windows con **Inno Setup**.

Este documento sirve para **entender, compilar y defender** el proyecto.

---

## 1. ¿Qué hace la aplicación?

Es una lista de tareas donde puedes:

- **Agregar** una tarea escribiéndola y presionando Enter (o el botón "Agregar").
- **Marcar como completada** con una casilla (el texto se tacha).
- **Editar** el texto de una tarea (botón ✏️, guardar con 💾 o Enter).
- **Eliminar** una tarea (botón 🗑️).
- **Limpiar completadas** de un solo clic.
- Ver un **contador** de tareas pendientes y totales.

Todas las tareas se **guardan en una base de datos**, así que siguen ahí aunque cierres y vuelvas a abrir el programa.

---

## 2. Tecnologías usadas (y por qué)

| Tecnología | Para qué sirve | Por qué se eligió |
|---|---|---|
| **.NET 8** | Plataforma de desarrollo de C#. | Es la versión LTS (soporte a largo plazo), estable y gratuita. |
| **C#** | Lenguaje de programación. | Moderno, tipado y muy usado en la industria. |
| **Avalonia UI 11** | Framework para la interfaz gráfica. | Es multiplataforma (Windows, Linux, macOS) y usa XAML, parecido a WPF. |
| **XAML** | Lenguaje para describir la interfaz. | Separa el diseño (visual) de la lógica (código). |
| **MVVM** | Patrón de arquitectura. | Ordena el código y separa responsabilidades. |
| **CommunityToolkit.Mvvm** | Librería que facilita MVVM. | Reduce código repetitivo (propiedades y comandos). |
| **SQLite** | Base de datos. | Es un archivo, no necesita servidor ni instalación aparte. |
| **Inno Setup** | Crea el instalador .exe. | Estándar gratuito y sencillo para instaladores de Windows. |

---

## 3. La arquitectura: patrón MVVM

**MVVM** significa **Model – View – ViewModel**. Es una forma de organizar el código separando tres responsabilidades:

- **Model (Modelo):** los datos. → `Models/TodoItem.cs` y `Data/TodoDatabase.cs`
- **View (Vista):** lo que se ve en pantalla. → `Views/MainWindow.axaml`
- **ViewModel:** la lógica que conecta datos y vista. → `ViewModels/MainWindowViewModel.cs`

### ¿Por qué separar así?

Porque la **Vista no contiene lógica** y el **ViewModel no sabe nada de botones**. Esto hace el código más ordenado, fácil de mantener y de probar. La Vista y el ViewModel se comunican por **enlace de datos (data binding)**: la vista se "engancha" a las propiedades del ViewModel y se actualiza sola cuando estas cambian.

```
   ┌──────────────┐   data binding   ┌──────────────────┐   llama a   ┌──────────────┐
   │   VIEW       │ ◄──────────────► │    VIEWMODEL     │ ──────────► │    MODEL     │
   │ MainWindow   │  (propiedades y  │ MainWindowVM     │  (guarda/   │ TodoDatabase │
   │  .axaml      │    comandos)     │  lógica + datos  │   lee)      │ + TodoItem   │
   └──────────────┘                  └──────────────────┘             └──────────────┘
                                                                        │  SQLite  │
                                                                        │ tareas.db│
```

---

## 4. Explicación archivo por archivo

### `Program.cs` — el arranque
Es el punto de entrada (lo primero que se ejecuta). Solo configura Avalonia y abre la aplicación. No lleva lógica de negocio.

### `app.manifest` — configuración de Windows
Le dice a Windows que la app soporta pantallas de alta resolución (DPI), para que no se vea borrosa.

### `App.axaml` + `App.axaml.cs` — la aplicación
`App.axaml` define el tema visual (Fluent, tema claro). `App.axaml.cs` **arma la aplicación al iniciar**: crea la base de datos, crea el ViewModel, le pasa la base de datos, y abre la ventana principal. Este "pasar la base de datos al ViewModel" se llama **inyección de dependencias**: el ViewModel no crea la base de datos, se la dan hecha. Así es más fácil de probar y de cambiar.

### `Models/TodoItem.cs` — el modelo de una tarea
Representa **una** tarea con: `Id`, `Title` (texto), `IsCompleted` (completada sí/no), `CreatedAt` (fecha). Hereda de `ObservableObject`: gracias al atributo `[ObservableProperty]`, cuando una propiedad cambia, **la interfaz se entera automáticamente** y se actualiza. `IsEditing` es un estado temporal solo visual (no se guarda) que indica si la tarea se está editando.

### `Data/TodoDatabase.cs` — el acceso a la base de datos
Concentra **todo el SQL** en un solo lugar. Ofrece métodos claros:
- `GetAll()` → leer todas las tareas.
- `Add()` → insertar una tarea (y devuelve el Id que generó la base de datos).
- `Update()` → actualizar texto o estado.
- `Delete()` → borrar por Id.

Al iniciarse crea la tabla si no existe (`CREATE TABLE IF NOT EXISTS`). La base de datos se guarda en la carpeta del usuario (`AppData\Local\TodoApp\tareas.db`), así **no necesita permisos de administrador** para escribir y cada usuario tiene sus propias tareas.

> **Detalle importante para defender:** se usan **parámetros** (`@titulo`, `@id`) en las consultas en vez de "pegar" el texto directamente. Esto evita la **inyección SQL** (un ataque de seguridad) y problemas con comillas o tildes.

### `ViewModels/MainWindowViewModel.cs` — el cerebro
Contiene:
- `Todos`: la **lista observable** de tareas que se muestra en pantalla. Al ser `ObservableCollection`, cuando agregas o quitas un elemento, la lista visual se actualiza sola.
- `NewTodoTitle`: el texto que el usuario está escribiendo.
- `StatusText`: el texto del contador.
- **Comandos** (`Add`, `Delete`, `StartEdit`, `SaveEdit`, `ClearCompleted`): son las acciones que disparan los botones. El atributo `[RelayCommand]` los genera automáticamente.

Cuando el usuario marca una casilla, el ViewModel **escucha ese cambio** (`PropertyChanged`) y guarda el nuevo estado en la base de datos.

### `Views/MainWindow.axaml` — la interfaz
Describe la ventana en XAML: el encabezado, el cuadro de texto para escribir, la lista de tareas y el pie con el contador. Cada elemento visual se **enlaza** a una propiedad o comando del ViewModel (por ejemplo, `Text="{Binding NewTodoTitle}"`). El `code-behind` (`MainWindow.axaml.cs`) está casi vacío **a propósito**: en MVVM la lógica no va ahí.

---

## 5. Cómo funciona SQLite en este proyecto

SQLite es una base de datos que vive en **un solo archivo** (`tareas.db`), sin servidor. La tabla es:

```sql
CREATE TABLE Tareas (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,  -- número único automático
    Title       TEXT    NOT NULL,                   -- el texto de la tarea
    IsCompleted INTEGER NOT NULL DEFAULT 0,          -- 0 = pendiente, 1 = completada
    CreatedAt   TEXT    NOT NULL                     -- fecha de creación
);
```

> SQLite no tiene un tipo "booleano", por eso `IsCompleted` se guarda como número: **0 = falso, 1 = verdadero**.

El ciclo es: la app **lee** con `SELECT`, **inserta** con `INSERT`, **modifica** con `UPDATE` y **borra** con `DELETE`. Son las 4 operaciones básicas, conocidas como **CRUD** (Create, Read, Update, Delete).

---

## 6. Requisitos previos (instalar en tu Windows)

1. **.NET 8 SDK** — descárgalo de: https://dotnet.microsoft.com/download/dotnet/8.0
   Elige "SDK x64" para Windows. Verifica en una terminal (CMD o PowerShell):
   ```
   dotnet --version
   ```
   Debe mostrar algo como `8.0.xxx`.

2. **Inno Setup** (solo para el instalador) — descárgalo de: https://jrsoftware.org/isdl.php
   Instálalo con las opciones por defecto.

---

## 7. Cómo ejecutar la app (modo desarrollo)

Abre una terminal en la carpeta del proyecto (donde está `TodoApp.csproj`) y ejecuta:

```
dotnet restore     (descarga las librerías la primera vez)
dotnet run         (compila y ejecuta la app)
```

Se abrirá la ventana. Prueba agregar, completar, editar y eliminar tareas.

---

## 8. Cómo generar el instalador (paso a paso)

### Paso 1 — Publicar la aplicación
Esto genera el `.exe` final y todos sus archivos en una carpeta `publish`. Usamos **self-contained** para que el usuario final **no necesite instalar .NET**:

```
dotnet publish -c Release -r win-x64 --self-contained true -o publish
```

- `-c Release` → versión optimizada.
- `-r win-x64` → para Windows de 64 bits.
- `--self-contained true` → incluye .NET dentro del programa.
- `-o publish` → carpeta de salida.

Al terminar, dentro de `publish` estará `TodoApp.exe`.

### Paso 2 — Compilar el instalador con Inno Setup
1. Abre **Inno Setup Compiler**.
2. Menú **File → Open** y abre `installer/setup.iss`.
3. Presiona **Build → Compile** (o el botón ▶️).
4. Se generará el instalador en `installer/Output/ListaDeTareas-Instalador-1.0.0.exe`.

Ese `.exe` es el instalador que puedes entregar: instala la app, crea accesos directos en el menú inicio y (opcionalmente) en el escritorio, y agrega un desinstalador.

> **Nota:** el script `setup.iss` busca la carpeta `..\publish`. Por eso primero debes hacer el Paso 1 y luego el Paso 2.

---

## 9. Preguntas típicas de defensa (con respuestas)

**¿Qué es MVVM y por qué lo usaste?**
Es un patrón que separa datos (Model), interfaz (View) y lógica (ViewModel). Lo usé para que el código quede ordenado, la interfaz no tenga lógica mezclada, y sea más fácil de mantener y probar.

**¿Qué es el data binding (enlace de datos)?**
Es la conexión automática entre la interfaz y el ViewModel. Por ejemplo, el cuadro de texto está "enganchado" a la propiedad `NewTodoTitle`: si el usuario escribe, la propiedad cambia sola, y viceversa.

**¿Por qué Avalonia y no Windows Forms o WPF?**
Avalonia es moderno y **multiplataforma** (funciona en Windows, Linux y Mac), mientras que WPF y WinForms solo funcionan en Windows. Usa XAML, muy parecido a WPF, así que es fácil de aprender.

**¿Por qué SQLite y no un archivo de texto o SQL Server?**
SQLite es una base de datos real (permite consultas SQL) pero **sin servidor**: es un solo archivo. Es perfecta para una app de escritorio pequeña. SQL Server sería excesivo y requeriría instalar un servidor.

**¿Dónde se guardan los datos?**
En `C:\Users\<usuario>\AppData\Local\TodoApp\tareas.db`. Lo puse ahí porque no requiere permisos de administrador y cada usuario tiene sus propias tareas.

**¿Qué es CRUD?**
Las 4 operaciones básicas sobre datos: **C**reate (insertar), **R**ead (leer), **U**pdate (actualizar) y **D**elete (borrar). Mi clase `TodoDatabase` implementa las cuatro.

**¿Cómo evitas problemas de seguridad en las consultas?**
Uso **parámetros** en el SQL (`@titulo`) en lugar de concatenar texto. Esto previene la **inyección SQL**.

**¿Qué significa self-contained en la publicación?**
Que el programa incluye dentro de sí el motor de .NET, así el usuario final puede instalarlo y usarlo **sin tener .NET instalado**.

**¿Qué hace Inno Setup?**
Toma la carpeta con el programa ya publicado y crea un **instalador .exe** que copia los archivos, crea accesos directos y permite desinstalar.

**¿Qué es un ObservableObject / ObservableCollection?**
Son clases que **avisan cuando cambian**. Gracias a ellas, cuando cambia un dato o se agrega un elemento a la lista, la interfaz se actualiza automáticamente sin escribir código extra.

---

## 10. Glosario rápido

- **SDK:** conjunto de herramientas para desarrollar (compilador, librerías).
- **Compilar:** traducir el código a un programa ejecutable.
- **Publicar:** generar la versión final lista para distribuir.
- **XAML:** lenguaje para describir interfaces (etiquetas parecidas a HTML).
- **Comando (Command):** una acción que dispara un botón, definida en el ViewModel.
- **Inyección de dependencias:** dar a una clase lo que necesita ya creado, en vez de que ella lo cree.

---

## 11. Estructura de archivos del proyecto

```
TodoApp/
├── TodoApp.csproj              → configuración del proyecto y librerías
├── app.manifest                → soporte de alta resolución (DPI)
├── Program.cs                  → punto de entrada (arranque)
├── App.axaml / App.axaml.cs    → tema visual y armado inicial de la app
├── Assets/
│   └── app-icon.ico            → ícono de la app y del instalador
├── Models/
│   └── TodoItem.cs             → modelo de una tarea
├── Data/
│   └── TodoDatabase.cs         → acceso a la base de datos SQLite (CRUD)
├── ViewModels/
│   └── MainWindowViewModel.cs  → lógica de la ventana (el "cerebro")
├── Views/
│   ├── MainWindow.axaml        → diseño de la interfaz
│   └── MainWindow.axaml.cs     → code-behind (casi vacío, por MVVM)
└── installer/
    └── setup.iss               → script de Inno Setup para el instalador
```
