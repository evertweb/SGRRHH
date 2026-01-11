namespace SGRRHH.Local.Shared.Interfaces;

/// <summary>
/// Servicio para manejar atajos de teclado globales
/// </summary>
public interface IKeyboardShortcutService
{
    /// <summary>
    /// Evento que se dispara cuando cambian los atajos
    /// </summary>
    event EventHandler? OnShortcutsChanged;
    
    /// <summary>
    /// Lista de atajos actualmente registrados
    /// </summary>
    List<KeyboardShortcutDto> CurrentShortcuts { get; }
    
    /// <summary>
    /// Registra atajos para una página específica
    /// </summary>
    void RegisterPageShortcuts(string pageId, List<KeyboardShortcutDto> shortcuts);
    
    /// <summary>
    /// Limpia los atajos de una página
    /// </summary>
    void ClearPageShortcuts(string pageId);
    
    /// <summary>
    /// Ejecuta la acción de un atajo
    /// </summary>
    Task ExecuteShortcutAsync(string key, bool ctrlKey = false, bool altKey = false, bool shiftKey = false);
    
    /// <summary>
    /// Indica si el servicio está habilitado
    /// </summary>
    bool IsEnabled { get; set; }
}

/// <summary>
/// DTO para un atajo de teclado
/// </summary>
public class KeyboardShortcutDto
{
    public string Key { get; set; } = string.Empty;
    public string KeyDisplay { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool CtrlKey { get; set; }
    public bool AltKey { get; set; }
    public bool ShiftKey { get; set; }
    public bool ShowInBar { get; set; } = true;
    public Func<Task>? Action { get; set; }
    public string PageId { get; set; } = string.Empty;
    public int Priority { get; set; } = 100; // Menor número = mayor prioridad
}
