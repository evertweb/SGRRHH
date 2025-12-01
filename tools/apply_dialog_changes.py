import sys

# Leer el archivo
file_path = r"c:\Users\evert\Documents\rrhh\src\SGRRHH.WPF\Views\NewConversationDialog.xaml.cs"

with open(file_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Reemplazo 1: Líneas 77-81 (dentro del foreach)
old_text1 = '''                        // Usar PhoneNumber como ID de Sendbird (único por usuario)
                        // Fallback: FirebaseUid -> Username
                        var sbUserId = !string.IsNullOrEmpty(user.PhoneNumber) 
                            ? user.PhoneNumber 
                            : (user.FirebaseUid ?? user.Username);'''

new_text1 = '''                        // Usar FirebaseUid como ID de Sendbird (único por usuario)
                        var sbUserId = !string.IsNullOrEmpty(user.FirebaseUid)
                            ? user.FirebaseUid
                            : user.Username;'''

# Reemplazo 2: Líneas 103-107 (variable sendbirdUserId)
old_text2 = '''                // El ID de Sendbird es el Phone Number del usuario (único)
                // Fallback: FirebaseUid -> Username
                var sendbirdUserId = !string.IsNullOrEmpty(user.PhoneNumber) 
                    ? user.PhoneNumber 
                    : (user.FirebaseUid ?? user.Username);'''

new_text2 = '''                // El ID de Sendbird es el FirebaseUid del usuario (único)
                var sendbirdUserId = !string.IsNullOrEmpty(user.FirebaseUid)
                    ? user.FirebaseUid
                    : user.Username;'''

# Realizar los reemplazos
modified = False

if old_text1 in content:
    content = content.replace(old_text1, new_text1)
    print("✅ Reemplazo 1 aplicado (foreach de sincronización)")
    modified = True
else:
    print("⚠️ Reemplazo 1 NO encontrado")

if old_text2 in content:
    content = content.replace(old_text2, new_text2)
    print("✅ Reemplazo 2 aplicado (creación de lista combinada)")
    modified = True
else:
    print("⚠️ Reemplazo 2 NO encontrado")

if modified:
    # Escribir el archivo
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)
    
    print("✅ Archivo modificado exitosamente!")
    print("✅ NewConversationDialog.xaml.cs actualizado para usar FirebaseUid")
else:
    print("❌ No se realizaron cambios")
    sys.exit(1)
