# Sendbird User Presence & ID Refactor

## Goal Description
Refactor the Sendbird integration to reliably show all application users and their online status. The previous approach using phone numbers as IDs was unreliable. We will switch to using `FirebaseUid` (or `Username` as fallback) as the unique Sendbird User ID. We will also modify the Chat UI to display a list of all users, allowing the current user to see who is online and leave messages for those who are not.

## User Review Required
> [!WARNING]
> **Breaking Change**: Changing the Sendbird User ID strategy means that previous chat history associated with "Phone Number IDs" will not be accessible with the new "FirebaseUid/Username IDs". Since the user asked to "create logic from scratch", this is assumed to be acceptable.

## Proposed Changes

### Core Logic
#### [MODIFY] [SendbirdChatService.cs](file:///c:/Users/evert/Documents/rrhh/src/SGRRHH.Infrastructure/Services/SendbirdChatService.cs)
- Update `ConnectAsync` to prioritize `FirebaseUid` > `Username` as `user_id`.
- Update `EnsureUserExistsAsync` to use the same ID generation logic.
- Ensure `GetUsersAsync` returns the correct status.

### ViewModel
#### [MODIFY] [ChatViewModel.cs](file:///c:/Users/evert/Documents/rrhh/src/SGRRHH.WPF/ViewModels/ChatViewModel.cs)
- Add `ObservableCollection<UserListItem> Users` property.
- Implement `LoadUsersAsync` to:
    1. Fetch all active users from `IUsuarioService`.
    2. Sync them to Sendbird using `EnsureUserExistsAsync`.
    3. Fetch Sendbird online status.
    4. Populate `Users` list.
- Add `OpenChatCommand` to handle clicking on a user in the list (creates/opens channel).
- Add a timer to refresh user presence periodically.

### UI
#### [MODIFY] [ChatView.xaml](file:///c:/Users/evert/Documents/rrhh/src/SGRRHH.WPF/Views/ChatView.xaml)
- Add a section to display the list of users (`Users` collection).
- Use a `Grid` with two columns or a `TabControl` to separate "Chats" (Channels) and "Contacts" (Users), or show Contacts in a sidebar. Given the requirement "show me users active or not", a sidebar or a prominent list is best. I will implement a 2-column layout: Left = List of Users/Chats, Right = Active Chat.
- I will add a "Usuarios" tab next to "Conversaciones" in the left panel.

#### [MODIFY] [NewConversationDialog.xaml.cs](file:///c:/Users/evert/Documents/rrhh/src/SGRRHH.WPF/Views/NewConversationDialog.xaml.cs)
- Update the ID generation logic to match `SendbirdChatService`.

## Verification Plan

### Manual Verification
1.  **Login**: Log in with a user.
2.  **Check List**: Verify that the "Usuarios" list appears and shows all other users.
3.  **Status**: Check if online users have the green indicator.
4.  **Chat**: Click on a user. Verify it opens a chat channel.
5.  **Message**: Send a message. Verify it is received.
6.  **Offline Message**: Send a message to an offline user. Verify they see it when they log in.
