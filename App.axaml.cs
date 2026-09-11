using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TodoApp.Data;
using TodoApp.ViewModels;
using TodoApp.Views;

namespace TodoApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    // Se ejecuta cuando el framework terminó de inicializar.
    // Aquí "armamos" la aplicación: base de datos -> ViewModel -> ventana.
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 1. Creamos/abrimos la base de datos SQLite
            var database = new TodoDatabase();

            // 2. Creamos el ViewModel y le pasamos la base de datos (inyección de dependencias)
            var mainViewModel = new MainWindowViewModel(database);

            // 3. Creamos la ventana principal y le asignamos su ViewModel (DataContext)
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
