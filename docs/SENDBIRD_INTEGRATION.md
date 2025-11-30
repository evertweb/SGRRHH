# Integración de Sendbird Chat (BETA)

## Resumen

Se ha integrado **Sendbird** como proveedor alternativo de chat para reducir el lag experimentado con la implementación de Firebase. Esta es una versión **BETA** que permite probar el rendimiento de Sendbird mientras se mantiene el chat legacy de Firebase como respaldo.

## Características

### Chat Legacy (Firebase)
- **Ubicación:** `ChatViewLegacy.xaml` / `ChatViewModelLegacy.cs`
- **Proveedor:** Firebase Firestore + Realtime listeners
- **Estado:** Funcional, usado en producción
- **Problema:** Lag en tiempo real con múltiples listeners

### Chat Nuevo (Sendbird)
- **Ubicación:** `ChatView.xaml` / `ChatViewModel.cs`
- **Proveedor:** Sendbird REST API
- **Estado:** BETA - En testing
- **Ventajas:**
  - Servicio especializado en chat en tiempo real
  - Menor lag
  - Infraestructura optimizada para mensajería
  - UI moderna con tema oscuro (estilo Discord/Slack)

## Configuración

### Datos de Sendbird

Tu cuenta de Sendbird ya está configurada en `appsettings.json`:

```json
{
  "Sendbird": {
    "Enabled": true,
    "ApplicationId": "8943C90E-F942-4A74-875D-71FA2F2AF0B3",
    "ApiToken": "7c00d171809cc981bacebbbe62295726553fb89a",
    "Region": "us-2"
  },
  "ChatProvider": "Sendbird"
}
```

**Región:** North Virginia 2, USA
**API URL:** https://api-8943C90E-F942-4A74-875D-71FA2F2AF0B3.sendbird.com

### Cambiar entre Proveedores

Para cambiar de proveedor, modifica el valor de `ChatProvider` en `appsettings.json`:

**Usar Sendbird (nuevo):**
```json
"ChatProvider": "Sendbird"
```

**Usar Firebase (legacy):**
```json
"ChatProvider": "Firebase"
```

No requiere recompilar, solo reiniciar la aplicación.

## Arquitectura

### Estructura del Código

```
SGRRHH.Core/
├── Interfaces/
│   ├── IChatService.cs              # Interfaz legacy (Firebase)
│   └── ISendbirdChatService.cs      # Interfaz nueva (Sendbird)
├── Models/
│   └── SendbirdSettings.cs          # Configuración de Sendbird
└── Entities/
    └── ChatMessage.cs               # Entidad legacy

SGRRHH.Infrastructure/
└── Services/
    ├── ChatService.cs               # Implementación Firebase (legacy)
    └── SendbirdChatService.cs       # Implementación Sendbird (nuevo)

SGRRHH.WPF/
├── Views/
│   ├── ChatViewLegacy.xaml          # UI legacy (Firebase)
│   └── ChatView.xaml                # UI nueva (Sendbird)
├── ViewModels/
│   ├── ChatViewModelLegacy.cs       # ViewModel legacy
│   └── ChatViewModel.cs             # ViewModel nuevo
└── Helpers/
    └── AppSettings.cs               # Lectura de configuración
```

### Switch Dinámico

El `MainViewModel.cs` detecta automáticamente qué proveedor usar:

```csharp
case "Chat":
    var chatProvider = AppSettings.GetChatProvider();

    if (chatProvider == "Sendbird")
    {
        // Usar nuevo chat con Sendbird
        var sendbirdViewModel = scope.ServiceProvider.GetRequiredService<ChatViewModel>();
        var sendbirdChatView = new ChatView(sendbirdViewModel);
        CurrentView = sendbirdChatView;
    }
    else
    {
        // Usar chat legacy con Firebase
        var chatViewModel = scope.ServiceProvider.GetRequiredService<ChatViewModelLegacy>();
        var chatView = new ChatViewLegacy(chatViewModel);
        CurrentView = chatView;
    }
    break;
```

## Uso

### Primera Vez

1. **Configuración ya está lista** - Los datos de Sendbird ya están en `appsettings.json`
2. **Ejecuta la aplicación:** `dotnet run` (desde `src/SGRRHH.WPF/`)
3. **Inicia sesión** con cualquier usuario existente
4. **Navega a Chat** - Verás la nueva UI con tema oscuro

### Funcionalidades Disponibles

#### ✅ Implementadas
- ✅ Conexión a Sendbird
- ✅ Listado de conversaciones
- ✅ Ver mensajes de una conversación
- ✅ Enviar mensajes
- ✅ Marcar conversaciones como leídas
- ✅ Contador de mensajes no leídos
- ✅ Indicadores de usuario online/offline
- ✅ UI moderna con tema oscuro
- ✅ Avatares con iniciales
- ✅ Timestamps de mensajes
- ✅ Nueva conversación con selector de usuarios
- ✅ Mensajes en tiempo real (polling cada 3 segundos)
- ✅ Actualización automática de canales (polling cada 10 segundos)
- ✅ Adjuntar archivos/imágenes (multipart/form-data, límite 25 MB)
- ✅ Descargar archivos adjuntos
- ✅ Preview de archivos con iconos según extensión

#### ⚠️ Pendientes (Implementación Futura)
- ⚠️ Notificaciones de escritura ("typing...") - Requiere webhooks o SDK nativo
- ⚠️ Reacciones a mensajes - Requiere soporte adicional del API

## Diferencias entre Proveedores

| Característica | Firebase (Legacy) | Sendbird (Nuevo) |
|----------------|-------------------|------------------|
| **Mensajes en tiempo real** | ✅ Listeners en tiempo real | ✅ Polling cada 3 segundos |
| **Lag** | ❌ Alto con múltiples usuarios | ✅ Bajo, optimizado |
| **UI** | Tema claro (WhatsApp-like) | Tema oscuro (Discord-like) |
| **Presencia online** | ✅ Sí | ✅ Sí |
| **Historial de mensajes** | ✅ Sí | ✅ Sí |
| **Nueva conversación** | ✅ Sí | ✅ Sí (con selector de usuarios) |
| **Archivos adjuntos** | ✅ Sí | ✅ Sí (límite 25 MB) |

## Testing

### Probar el Chat de Sendbird

1. **Con 2 usuarios diferentes:**
   - PC 1: Inicia sesión con `admin`
   - PC 2: Inicia sesión con `secretaria`

2. **Crear conversación:**
   - Haz clic en el botón "➕ Nueva conversación"
   - Busca y selecciona el usuario con quien chatear
   - El canal se creará automáticamente

3. **Enviar mensajes de texto:**
   - Escribe en el input de texto
   - Presiona Enter o clic en "Enviar ➤"

4. **Adjuntar archivos:**
   - Haz clic en el botón 📎 (adjuntar)
   - Selecciona archivo (máx 25 MB)
   - El archivo se envía automáticamente con preview

5. **Descargar archivos:**
   - Haz clic en "⬇ Descargar" en el mensaje de archivo
   - Elige ubicación de descarga

6. **Verificar lag:**
   - Compara la velocidad de entrega con Firebase
   - Los mensajes deberían aparecer en ~3 segundos (polling)

### Problemas Conocidos

1. ~~**Mensajes no aparecen en tiempo real automáticamente**~~ - **✅ RESUELTO**
   - **Solución:** Se implementó polling cada 3 segundos para verificar nuevos mensajes
   - Los mensajes nuevos aparecen automáticamente en la conversación actual

2. ~~**No se puede crear nuevas conversaciones desde la app**~~ - **✅ RESUELTO**
   - **Solución:** Se implementó selector de usuarios con botón "Nueva conversación"
   - Permite buscar y seleccionar cualquier usuario activo del sistema

3. **Typing indicators no están implementados:**
   - **Causa:** Requiere webhooks de Sendbird o SDK nativo
   - **Estado:** Pendiente para implementación futura
   - **Workaround:** No disponible por ahora

## Pasos Siguientes

### Para Producción
1. ✅ Testear con usuarios reales
2. ✅ Implementar polling para mensajes en tiempo real
3. ✅ Implementar selector de usuarios para nueva conversación
4. ✅ Implementar adjuntar y descargar archivos
5. ⚠️ Agregar manejo de errores robusto
6. ⚠️ Configurar límites de rate limiting
7. ⚠️ Implementar retry logic para requests fallidos
8. ⚠️ Considerar migrar a webhooks para mejor rendimiento (opcional)

### Mejoras Opcionales
- Typing indicators ("Usuario está escribiendo...")
- Mensajes de voz
- Videollamadas (Sendbird Calls)
- Stickers/GIFs
- Buscar en mensajes
- Menciones (@usuario)
- Hilos de conversación

## API de Sendbird

### Endpoints Principales (ya implementados)

```
POST   /v3/users                              # Crear/actualizar usuario
GET    /v3/users/{user_id}/my_group_channels  # Listar canales del usuario
POST   /v3/group_channels                     # Crear canal
GET    /v3/group_channels/{channel_url}/messages  # Obtener mensajes
POST   /v3/group_channels/{channel_url}/messages  # Enviar mensaje (texto o archivo)
PUT    /v3/group_channels/{channel_url}/messages/mark_as_read  # Marcar como leído
GET    /v3/users/{user_id}/unread_message_count  # Contador de no leídos
```

**Nota sobre archivos:**
- El endpoint POST `/messages` acepta `multipart/form-data` para archivos
- Límite de tamaño: 25 MB por archivo
- Almacenamiento: Sendbird Storage (10 GB/mes en plan free)
- Tipos soportados: Todos los tipos MIME comunes

### Autenticación

Todas las requests usan el header:
```
Api-Token: 7c00d171809cc981bacebbbe62295726553fb89a
```

## Logs y Debugging

Para ver logs de Sendbird, busca en los logs de la aplicación:

```
SendbirdChatService - Usuario {username} conectado a Sendbird
SendbirdChatService - Error al crear canal con usuario {userId}
SendbirdChatService - Mensaje enviado exitosamente al canal {channelUrl}
```

## Soporte

- **Dashboard Sendbird:** https://dashboard.sendbird.com/
- **Documentación API:** https://sendbird.com/docs/chat/platform-api/v3/overview
- **Support:** https://sendbird.com/support

## Notas de Seguridad

⚠️ **IMPORTANTE:** El API Token está en el código fuente. Para producción:
1. Mover el token a un archivo de configuración seguro (no en Git)
2. Usar variables de entorno
3. Considerar usar Session Tokens en lugar de API Token global
4. Rotar el token periódicamente

## Rollback

Si necesitas volver a Firebase:

1. Editar `appsettings.json`:
   ```json
   "ChatProvider": "Firebase"
   ```

2. Reiniciar la aplicación

Todo el código legacy está intacto y funcionando.
