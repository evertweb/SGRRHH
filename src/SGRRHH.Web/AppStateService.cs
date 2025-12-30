using SGRRHH.Core.Entities;
using SGRRHH.Core.Enums;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.Web;

public class AppStateService
{
    public event Action? OnChange;

    public FirebaseAuthResult? CurrentAuth { get; private set; }
    public Usuario? CurrentUser => CurrentAuth?.Usuario as Usuario;
    
    // Role-based authorization helpers
    public bool IsAdmin => CurrentUser?.Rol == RolUsuario.Administrador;
    public bool CanApprove => CurrentUser?.Rol == RolUsuario.Administrador || 
                              CurrentUser?.Rol == RolUsuario.Aprobador;
    public bool CanManageUsers => CurrentUser?.Rol == RolUsuario.Administrador;
    
    // Cargo module permissions
    public bool CanCreateCargos => CurrentUser?.Rol == RolUsuario.Administrador ||
                                    CurrentUser?.Rol == RolUsuario.Aprobador;
    public bool CanEditCargos => CurrentUser?.Rol == RolUsuario.Administrador ||
                                  CurrentUser?.Rol == RolUsuario.Aprobador;
    public bool CanDeleteCargos => CurrentUser?.Rol == RolUsuario.Administrador;

    // Departamento module permissions
    public bool CanCreateDepartamentos => CurrentUser?.Rol == RolUsuario.Administrador ||
                                           CurrentUser?.Rol == RolUsuario.Aprobador;
    public bool CanEditDepartamentos => CurrentUser?.Rol == RolUsuario.Administrador ||
                                         CurrentUser?.Rol == RolUsuario.Aprobador;
    public bool CanDeleteDepartamentos => CurrentUser?.Rol == RolUsuario.Administrador;

    // Vacaciones module permissions
    public bool CanEditVacaciones => CurrentUser?.Rol == RolUsuario.Administrador ||
                                      CurrentUser?.Rol == RolUsuario.Aprobador;
    
    public string CurrentViewTitle { get; private set; } = "Inicio";
    public string WindowTitle => $"SGRRHH v2.0 - {CurrentViewTitle}";
    
    public string CurrentUserName => CurrentUser?.NombreCompleto ?? CurrentAuth?.Usuario?.Username ?? "Usuario";
    public string CurrentUserRole => CurrentUser?.Rol.ToString() ?? "Invitado";
    
    public int PendingAlerts { get; private set; } = 0;
    public string AppVersion { get; private set; } = "2.0.0-web";

    public bool IsAuthenticated => CurrentAuth != null;

    public void SetAuth(FirebaseAuthResult auth)
    {
        CurrentAuth = auth;
        NotifyStateChanged();
    }

    public void Logout()
    {
        CurrentAuth = null;
        NotifyStateChanged();
    }

    public void SetViewTitle(string title)
    {
        CurrentViewTitle = title;
        NotifyStateChanged();
    }

    public void SetAlerts(int count)
    {
        PendingAlerts = count;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
