using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using TodoApp.Models;

namespace TodoApp.Data;

/// <summary>
/// Encapsula TODO el acceso a la base de datos SQLite.
/// El resto de la app no sabe nada de SQL: solo pide "dame las tareas",
/// "agrega esta", etc. Esto se llama capa de acceso a datos (DAL).
/// </summary>
public class TodoDatabase
{
    private readonly string _connectionString;

    public TodoDatabase()
    {
        // Guardamos la base de datos en la carpeta de datos del usuario:
        //   Windows -> C:\Users\<usuario>\AppData\Local\TodoApp\tareas.db
        // Así cada usuario del equipo tiene sus propias tareas y no se
        // necesitan permisos de administrador para escribir.
        string folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TodoApp");
        Directory.CreateDirectory(folder);

        string dbPath = Path.Combine(folder, "tareas.db");
        _connectionString = $"Data Source={dbPath}";

        InitializeDatabase();
    }

    // Crea la tabla la primera vez que se abre la app (si aún no existe)
    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
            @"CREATE TABLE IF NOT EXISTS Tareas (
                Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                Title       TEXT    NOT NULL,
                IsCompleted INTEGER NOT NULL DEFAULT 0,
                CreatedAt   TEXT    NOT NULL
            );";
        command.ExecuteNonQuery();
    }

    /// <summary>Lee todas las tareas ordenadas: primero las pendientes, luego por fecha.</summary>
    public List<TodoItem> GetAll()
    {
        var items = new List<TodoItem>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
            "SELECT Id, Title, IsCompleted, CreatedAt FROM Tareas ORDER BY IsCompleted ASC, CreatedAt DESC;";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            items.Add(new TodoItem
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                IsCompleted = reader.GetInt32(2) == 1,
                CreatedAt = DateTime.Parse(reader.GetString(3))
            });
        }

        return items;
    }

    /// <summary>Inserta una tarea nueva y devuelve el Id generado por la base de datos.</summary>
    public int Add(TodoItem item)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        // Usamos parámetros (@titulo) en vez de concatenar texto:
        // así evitamos inyección SQL y errores con comillas.
        command.CommandText =
            @"INSERT INTO Tareas (Title, IsCompleted, CreatedAt)
              VALUES (@titulo, @completada, @fecha);
              SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("@titulo", item.Title);
        command.Parameters.AddWithValue("@completada", item.IsCompleted ? 1 : 0);
        command.Parameters.AddWithValue("@fecha", item.CreatedAt.ToString("o")); // formato ISO 8601

        long newId = (long)command.ExecuteScalar()!;
        return (int)newId;
    }

    /// <summary>Actualiza el texto y el estado (completada/pendiente) de una tarea existente.</summary>
    public void Update(TodoItem item)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
            @"UPDATE Tareas
              SET Title = @titulo, IsCompleted = @completada
              WHERE Id = @id;";
        command.Parameters.AddWithValue("@titulo", item.Title);
        command.Parameters.AddWithValue("@completada", item.IsCompleted ? 1 : 0);
        command.Parameters.AddWithValue("@id", item.Id);
        command.ExecuteNonQuery();
    }

    /// <summary>Borra una tarea por su Id.</summary>
    public void Delete(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Tareas WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id);
        command.ExecuteNonQuery();
    }
}
