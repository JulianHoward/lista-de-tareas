# Lista de Tareas (To-Do List)

Aplicación de escritorio para gestionar tareas, desarrollada en **.NET 8** con **Avalonia UI**, base de datos **SQLite** y patrón **MVVM**. Incluye un instalador para Windows generado con **Inno Setup**.

Proyecto de pasantías.

## Características

- Agregar tareas (con Enter o botón).
- Marcar tareas como completadas (el texto se tacha).
- Editar el texto de una tarea.
- Eliminar tareas.
- Limpiar todas las completadas de una vez.
- Contador de tareas pendientes y totales.
- Las tareas se guardan en una base de datos SQLite y persisten al cerrar la app.

## Tecnologías

- .NET 8 / C#
- Avalonia UI 11 (interfaz gráfica multiplataforma, con XAML)
- CommunityToolkit.Mvvm (patrón MVVM)
- Microsoft.Data.Sqlite (base de datos)
- Inno Setup (instalador de Windows)

## Estructura del proyecto

```
TodoApp/
├── Program.cs                  Punto de entrada
├── App.axaml / App.axaml.cs    Configuración e inicio de la app
├── Models/TodoItem.cs          Modelo de una tarea
├── Data/TodoDatabase.cs        Acceso a la base de datos SQLite (CRUD)
├── ViewModels/                 Lógica (MVVM)
│   └── MainWindowViewModel.cs
├── Views/                      Interfaz
│   ├── MainWindow.axaml
│   └── MainWindow.axaml.cs
├── Assets/                     Ícono de la app
└── installer/setup.iss         Script del instalador (Inno Setup)
```

## Cómo ejecutar (modo desarrollo)

Requisito: tener instalado el SDK de .NET 8 (https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
dotnet restore
dotnet run
```

## Cómo generar el instalador

1. Publicar la aplicación (incluye .NET, no requiere instalarlo aparte):

   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -o publish
   ```

2. Abrir `installer/setup.iss` en Inno Setup (https://jrsoftware.org/isdl.php) y compilar (Build -> Compile).

3. El instalador se genera en `installer/Output/`.

## Dónde se guardan los datos

La base de datos se crea automáticamente en:

```
C:\Users\<usuario>\AppData\Local\TodoApp\tareas.db
```
