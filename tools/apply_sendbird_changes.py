import sys

# Leer el archivo
file_path = r"c:\Users\evert\Documents\rrhh\src\SGRRHH.WPF\ViewModels\ChatViewModel.cs"

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Definir el código a insertar
sync_code = '''
            // ✨ NUEVO: Sincronizar TODOS los usuarios del sistema a Sendbird (UNA SOLA VEZ)
            // Esto asegura que TODOS los usuarios estén disponibles para chatear
            LoadingMessage = "Sincronizando usuarios...";
            try
            {
                var allUsers = await _usuarioService.GetAllActiveAsync();
                var syncedCount = await _sendbirdService.SyncAllUsersAsync(allUsers);
                System.Diagnostics.Debug.WriteLine($"✅ Sincronizados {syncedCount} usuarios con Sendbird");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error en sincronización (no crítico): {ex.Message}");
                // No bloquear la app si la sincronización falla
            }

'''

# Buscar y reemplazar
old_text = '''            }

            LoadingMessage = "Cargando conversaciones...";

            // Cargar canales'''

new_text = '''            }
''' + sync_code + '''            LoadingMessage = "Cargando conversaciones...";

            // Cargar canales'''

# Realizar el reemplazo
if old_text in content:
    content = content.replace(old_text, new_text)
    
    # Escribir el archivo
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)
    
    print("✅ Archivo modificado exitosamente!")
    print("✅ Se agregó sincronización de usuarios en ChatViewModel.cs")
else:
    print("❌ No se encontró el texto a reemplazar")
    print("Verificar manualmente el archivo")
    sys.exit(1)
