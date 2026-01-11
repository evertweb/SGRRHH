using SGRRHH.Local.Shared.Interfaces;

namespace SGRRHH.Local.Infrastructure.Services;

/// <summary>
/// Servicio para manejar atajos de teclado globales
/// </summary>
public class KeyboardShortcutService : IKeyboardShortcutService
{
    private readonly Dictionary<string, List<KeyboardShortcutDto>> _pageShortcuts = new();
    private bool _isEnabled = true;
    
    public event EventHandler? OnShortcutsChanged;
    
    public List<KeyboardShortcutDto> CurrentShortcuts
    {
        get
        {
            var allShortcuts = _pageShortcuts.Values
                .SelectMany(x => x)
                .OrderBy(x => x.Priority)
                .ToList();
            return allShortcuts;
        }
    }
    
    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }
    
    public void RegisterPageShortcuts(string pageId, List<KeyboardShortcutDto> shortcuts)
    {
        foreach (var shortcut in shortcuts)
        {
            shortcut.PageId = pageId;
        }
        
        _pageShortcuts[pageId] = shortcuts;
        OnShortcutsChanged?.Invoke(this, EventArgs.Empty);
    }
    
    public void ClearPageShortcuts(string pageId)
    {
        if (_pageShortcuts.Remove(pageId))
        {
            OnShortcutsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    
    public async Task ExecuteShortcutAsync(string key, bool ctrlKey = false, bool altKey = false, bool shiftKey = false)
    {
        if (!_isEnabled) return;
        
        // Buscar el shortcut que coincida (prioridad a los más específicos primero)
        var shortcut = CurrentShortcuts
            .Where(s => s.Key.Equals(key, StringComparison.OrdinalIgnoreCase) &&
                       s.CtrlKey == ctrlKey &&
                       s.AltKey == altKey &&
                       s.ShiftKey == shiftKey)
            .OrderBy(s => s.Priority)
            .FirstOrDefault();
        
        if (shortcut?.Action != null)
        {
            await shortcut.Action.Invoke();
        }
    }
}
