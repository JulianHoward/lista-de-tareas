using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TodoApp.Models;

/// <summary>
/// Representa una tarea. Hereda de ObservableObject para que la interfaz
/// se actualice automáticamente cuando cambian sus propiedades (patrón MVVM).
/// </summary>
public partial class TodoItem : ObservableObject
{
    // Identificador único en la base de datos (clave primaria)
    public int Id { get; set; }

    // El texto de la tarea. [ObservableProperty] genera automáticamente
    // la propiedad pública "Title" con notificación de cambios.
    [ObservableProperty]
    private string _title = string.Empty;

    // Si la tarea está completada o no
    [ObservableProperty]
    private bool _isCompleted;

    // Fecha y hora en que se creó la tarea
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Estado temporal SOLO de interfaz (no se guarda en la base de datos):
    // indica si la tarea está en modo edición para mostrar un cuadro de texto.
    [ObservableProperty]
    private bool _isEditing;
}
