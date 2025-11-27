using SGRRHH.WPF.ViewModels;
using System.Windows.Controls;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Lógica de interacción para UsuariosListView.xaml
/// </summary>
public partial class UsuariosListView : UserControl
{
    private readonly UsuariosListViewModel _viewModel;
    
    public UsuariosListView(UsuariosListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        
        // Conectar el PasswordBox con el ViewModel (ya que PasswordBox no soporta binding directo)
        PasswordBox.PasswordChanged += (s, e) =>
        {
            _viewModel.EditPassword = PasswordBox.Password;
        };
    }
}
