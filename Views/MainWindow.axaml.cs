using Avalonia.Controls;

namespace TodoApp.Views;

// "Code-behind" de la ventana. En MVVM queda casi vacío a propósito:
// toda la lógica vive en el ViewModel, no aquí.
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
