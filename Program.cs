using Avalonia;
using System;

namespace TodoApp;

// Punto de entrada de la aplicación (lo primero que se ejecuta al abrir el .exe)
internal static class Program
{
    // No usar código de Avalonia antes de que AppMain sea llamado:
    // aún no está inicializado y puede fallar.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Configuración del framework Avalonia
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()   // Detecta el sistema operativo (Windows, Linux, macOS)
            .WithInterFont()       // Fuente tipográfica moderna
            .LogToTrace();
}
