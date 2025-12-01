import sys

file_path = r"c:\Users\evert\Documents\rrhh\src\SGRRHH.WPF\ViewModels\LoginViewModel.cs"

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Reemplazo 1: Modificar LoginAsync para usar GetFullUsername()
old_login = """            var result = await _authService.AuthenticateAsync(Username, Password);"""

new_login = """            // Usar username con dominio auto-completado
            var fullUsername = GetFullUsername();
            var result = await _authService.AuthenticateAsync(fullUsername, Password);"""

# Reemplazo 2: Agregar método GetFullUsername antes del método Exit
old_exit = """    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }
}"""

new_exit = """    /// <summary>
    /// Obtiene el username completo con dominio @sgrrhh.local si no está incluido
    /// </summary>
    private string GetFullUsername()
    {
        if (string.IsNullOrWhiteSpace(Username))
            return string.Empty;
        
        // Si ya contiene @, usarlo tal cual (permite dominios personalizados)
        if (Username.Contains("@"))
            return Username.Trim();
        
        // Auto-completar con dominio predeterminado
        return $"{Username.Trim()}@sgrrhh.local";
    }
    
    [RelayCommand]
    private void Exit()
    {
        Application.Current.Shutdown();
    }
}"""

modified = False

if old_login in content:
    content = content.replace(old_login, new_login)
    print("✅ Modificado LoginAsync para usar GetFullUsername()")
    modified = True
else:
    print("⚠️ No se encontró LoginAsync original")

if old_exit in content:
    content = content.replace(old_exit, new_exit)
    print("✅ Agregado método GetFullUsername()")
    modified = True
else:
    print("⚠️ No se encontró método Exit original")

if modified:
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)
    print("✅ LoginViewModel.cs modificado exitosamente!")
else:
    print("❌ No se realizaron cambios")
    sys.exit(1)
