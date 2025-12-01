using SGRRHH.Core.Interfaces;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace SGRRHH.WPF.Views;

/// <summary>
/// Ventana para verificación SMS (MFA)
/// </summary>
public partial class SmsVerificationWindow : Window
{
    private readonly ISmsVerificationService _smsService;
    private readonly string _phoneNumber;
    private readonly string _userId;
    
    /// <summary>
    /// Indica si la verificación fue exitosa
    /// </summary>
    public bool IsVerified { get; private set; }
    
    /// <summary>
    /// Indica si el usuario quiere recordar el dispositivo
    /// </summary>
    public bool TrustDevice => TrustDeviceCheckbox.IsChecked == true;
    
    public SmsVerificationWindow(
        ISmsVerificationService smsService, 
        string phoneNumber,
        string userId)
    {
        InitializeComponent();
        
        _smsService = smsService;
        _phoneNumber = phoneNumber;
        _userId = userId;
        
        // Mostrar número enmascarado
        PhoneLabel.Text = $"Enviado por WhatsApp a {MaskPhoneNumber(phoneNumber)}";
        
        // Focus en el campo de código
        Loaded += (s, e) => CodeTextBox.Focus();
    }
    
    /// <summary>
    /// Envía el código SMS
    /// </summary>
    public async Task<bool> SendCodeAsync()
    {
        SetLoading(true, "Enviando código...");
        
        var result = await _smsService.SendVerificationCodeAsync(_phoneNumber);
        
        SetLoading(false);
        
        if (!result.Success)
        {
            ShowError(result.Message ?? "Error al enviar código");
            return false;
        }
        
        return true;
    }
    
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1)
            DragMove();
    }
    
    private void CodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Solo permitir números
        e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
    }
    
    private void CodeTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // Habilitar botón verificar cuando hay 6 dígitos
        VerifyButton.IsEnabled = CodeTextBox.Text.Length == 6;
        
        // Ocultar error al escribir
        ErrorBorder.Visibility = Visibility.Collapsed;
        
        // Auto-verificar cuando se completa el código
        if (CodeTextBox.Text.Length == 6)
        {
            _ = VerifyCodeAsync();
        }
    }
    
    private async void VerifyButton_Click(object sender, RoutedEventArgs e)
    {
        await VerifyCodeAsync();
    }
    
    private async Task VerifyCodeAsync()
    {
        var code = CodeTextBox.Text.Trim();
        
        if (code.Length != 6)
        {
            ShowError("Ingrese el código de 6 dígitos");
            return;
        }
        
        SetLoading(true, "Verificando...");
        VerifyButton.IsEnabled = false;
        
        var result = await _smsService.VerifyCodeAsync(_phoneNumber, code);
        
        SetLoading(false);
        
        if (result.Success && result.IsValid)
        {
            IsVerified = true;
            
            // Si el usuario quiere recordar el dispositivo
            if (TrustDevice)
            {
                await _smsService.TrustDeviceAsync(_userId, 30);
            }
            
            DialogResult = true;
            Close();
        }
        else
        {
            ShowError(result.Message ?? "Código incorrecto");
            CodeTextBox.SelectAll();
            CodeTextBox.Focus();
            VerifyButton.IsEnabled = true;
        }
    }
    
    private async void ResendButton_Click(object sender, RoutedEventArgs e)
    {
        ResendButton.IsEnabled = false;
        SetLoading(true, "Reenviando código...");
        
        var result = await _smsService.SendVerificationCodeAsync(_phoneNumber);
        
        SetLoading(false);
        
        if (result.Success)
        {
            ShowInfo("Código reenviado");
            CodeTextBox.Clear();
            CodeTextBox.Focus();
        }
        else
        {
            ShowError(result.Message ?? "Error al reenviar código");
        }
        
        // Deshabilitar reenvío por 30 segundos
        await Task.Delay(30000);
        ResendButton.IsEnabled = true;
    }
    
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        IsVerified = false;
        DialogResult = false;
        Close();
    }
    
    private void SetLoading(bool isLoading, string message = "")
    {
        LoadingPanel.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        LoadingText.Text = $" {message}";
        CodeTextBox.IsEnabled = !isLoading;
    }
    
    private void ShowError(string message)
    {
        ErrorBorder.Visibility = Visibility.Visible;
        ErrorBorder.Background = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(255, 235, 238)); // #FFEBEE
        ErrorText.Foreground = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(229, 57, 53)); // #E53935
        ErrorText.Text = message;
    }
    
    private void ShowInfo(string message)
    {
        ErrorBorder.Visibility = Visibility.Visible;
        ErrorBorder.Background = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(227, 242, 253)); // #E3F2FD
        ErrorText.Foreground = new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromRgb(30, 136, 229)); // #1E88E5
        ErrorText.Text = message;
    }
    
    private string MaskPhoneNumber(string phone)
    {
        if (string.IsNullOrEmpty(phone) || phone.Length < 6)
            return "***";
        
        return $"{phone[..4]}****{phone[^3..]}";
    }
}
