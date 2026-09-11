using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.ViewModels;

/// <summary>
/// El "cerebro" de la ventana principal. Contiene los datos que se muestran
/// y la lógica (agregar, editar, completar, eliminar). La interfaz (View)
/// solo se enlaza a las propiedades y comandos de esta clase. Esto es MVVM.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly TodoDatabase _database;

    // Colección observable: cuando agregamos o quitamos elementos,
    // la lista de la interfaz se actualiza sola.
    public ObservableCollection<TodoItem> Todos { get; } = new();

    // Texto del cuadro donde el usuario escribe una tarea nueva.
    // [NotifyCanExecuteChangedFor] hace que el botón "Agregar" se habilite
    // o deshabilite automáticamente según si hay texto escrito.
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AddCommand))]
    private string _newTodoTitle = string.Empty;

    // Texto del contador que se muestra abajo (ej: "3 pendientes de 5")
    [ObservableProperty]
    private string _statusText = string.Empty;

    public MainWindowViewModel(TodoDatabase database)
    {
        _database = database;
        LoadTodos();
    }

    // Carga todas las tareas desde la base de datos al iniciar
    private void LoadTodos()
    {
        Todos.Clear();
        foreach (var item in _database.GetAll())
        {
            RegisterItem(item);
            Todos.Add(item);
        }
        UpdateStatus();
    }

    // Nos "suscribimos" a los cambios de cada tarea para detectar
    // cuando el usuario marca/desmarca la casilla de completada y guardarlo.
    private void RegisterItem(TodoItem item)
    {
        item.PropertyChanged += OnItemPropertyChanged;
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is TodoItem item && e.PropertyName == nameof(TodoItem.IsCompleted))
        {
            // Se marcó/desmarcó la casilla -> guardamos el nuevo estado
            _database.Update(item);
            UpdateStatus();
        }
    }

    // --- AGREGAR ---
    // [RelayCommand] genera automáticamente una propiedad "AddCommand"
    // que la interfaz usa en el botón. CanAdd controla si está habilitado.
    [RelayCommand(CanExecute = nameof(CanAdd))]
    private void Add()
    {
        var item = new TodoItem { Title = NewTodoTitle.Trim() };
        item.Id = _database.Add(item);   // se guarda y obtiene su Id

        RegisterItem(item);
        Todos.Insert(0, item);           // aparece arriba de la lista
        NewTodoTitle = string.Empty;     // limpia el cuadro de texto
        UpdateStatus();
    }

    // Solo se puede agregar si el cuadro de texto no está vacío
    private bool CanAdd() => !string.IsNullOrWhiteSpace(NewTodoTitle);

    // --- ELIMINAR ---
    [RelayCommand]
    private void Delete(TodoItem item)
    {
        _database.Delete(item.Id);
        item.PropertyChanged -= OnItemPropertyChanged; // dejamos de escuchar
        Todos.Remove(item);
        UpdateStatus();
    }

    // --- EDITAR (entrar en modo edición) ---
    [RelayCommand]
    private void StartEdit(TodoItem item)
    {
        item.IsEditing = true;
    }

    // --- GUARDAR EDICIÓN ---
    [RelayCommand]
    private void SaveEdit(TodoItem item)
    {
        // Si quedó vacío, no permitimos guardar texto en blanco
        if (!string.IsNullOrWhiteSpace(item.Title))
        {
            item.Title = item.Title.Trim();
            _database.Update(item);
        }
        item.IsEditing = false;
    }

    // --- LIMPIAR COMPLETADAS ---
    [RelayCommand]
    private void ClearCompleted()
    {
        // ToList() crea una copia para poder modificar la colección mientras recorremos
        foreach (var item in Todos.Where(t => t.IsCompleted).ToList())
        {
            _database.Delete(item.Id);
            item.PropertyChanged -= OnItemPropertyChanged;
            Todos.Remove(item);
        }
        UpdateStatus();
    }

    // Recalcula el texto del contador
    private void UpdateStatus()
    {
        int total = Todos.Count;
        int pendientes = Todos.Count(t => !t.IsCompleted);

        StatusText = total == 0
            ? "No hay tareas. ¡Agrega una!"
            : $"{pendientes} pendiente(s) de {total} tarea(s)";
    }
}
