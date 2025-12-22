using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SGRRHH.WPF.Controls
{
    /// <summary>
    /// Botón con feedback visual inmediato para operaciones asíncronas.
    /// Muestra un indicador de carga inline estilo Windows Classic mientras IsLoading=true.
    /// </summary>
    public partial class LegacyAsyncButton : UserControl
    {
        private readonly DispatcherTimer _animationTimer;
        private int _animationFrame;
        private readonly string[] _animationFrames = { "[|  ]", "[|| ]", "[|||]", "[ ||]", "[  |]", "[ ||]", "[|||]", "[|| ]" };
        private bool _isPressed;

        #region Dependency Properties

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(object), typeof(LegacyAsyncButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(LegacyAsyncButton),
                new PropertyMetadata(null, OnCommandChanged));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(LegacyAsyncButton),
                new PropertyMetadata(null));

        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(LegacyAsyncButton),
                new PropertyMetadata(false, OnIsLoadingChanged));

        #endregion

        #region Properties

        /// <summary>
        /// Contenido del botón (texto o elementos)
        /// </summary>
        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        /// <summary>
        /// Comando a ejecutar al hacer clic
        /// </summary>
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        /// Parámetro del comando
        /// </summary>
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        /// <summary>
        /// Indica si está en estado de carga (muestra indicador)
        /// </summary>
        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        #endregion

        public LegacyAsyncButton()
        {
            InitializeComponent();

            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(120)
            };
            _animationTimer.Tick += AnimationTimer_Tick;

            Unloaded += (s, e) => _animationTimer.Stop();
        }

        #region Event Handlers

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            _animationFrame = (_animationFrame + 1) % _animationFrames.Length;
            LoadingIcon.Text = _animationFrames[_animationFrame];
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LegacyAsyncButton button)
            {
                // Desuscribirse del comando anterior
                if (e.OldValue is ICommand oldCommand)
                {
                    oldCommand.CanExecuteChanged -= button.OnCanExecuteChanged;
                }

                // Suscribirse al nuevo comando
                if (e.NewValue is ICommand newCommand)
                {
                    newCommand.CanExecuteChanged += button.OnCanExecuteChanged;
                    button.UpdateIsEnabled();
                }
            }
        }

        private void OnCanExecuteChanged(object? sender, EventArgs e)
        {
            UpdateIsEnabled();
        }

        private void UpdateIsEnabled()
        {
            if (Command != null)
            {
                IsEnabled = !IsLoading && Command.CanExecute(CommandParameter);
            }
        }

        private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LegacyAsyncButton button)
            {
                var isLoading = (bool)e.NewValue;
                button.UpdateLoadingState(isLoading);
            }
        }

        private void UpdateLoadingState(bool isLoading)
        {
            if (isLoading)
            {
                // Mostrar indicador de carga
                LoadingPanel.Visibility = Visibility.Visible;
                ContentHost.Visibility = Visibility.Collapsed;
                
                // Cambiar apariencia a "presionado" (inset)
                SetPressedAppearance(true);
                
                // Iniciar animación
                _animationFrame = 0;
                LoadingIcon.Text = _animationFrames[0];
                _animationTimer.Start();
                
                // Deshabilitar interacción
                IsEnabled = false;
            }
            else
            {
                // Ocultar indicador de carga
                LoadingPanel.Visibility = Visibility.Collapsed;
                ContentHost.Visibility = Visibility.Visible;
                
                // Restaurar apariencia normal (outset)
                SetPressedAppearance(false);
                
                // Detener animación
                _animationTimer.Stop();
                
                // Rehabilitar
                UpdateIsEnabled();
            }
        }

        private void SetPressedAppearance(bool pressed)
        {
            if (pressed)
            {
                // Efecto Inset (presionado/cargando)
                OuterBorder.BorderBrush = new SolidColorBrush(Colors.Black);
                OuterBorderDark.BorderBrush = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
                InnerBorderLight.BorderBrush = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
                InnerBorderDark.BorderBrush = new SolidColorBrush(Color.FromRgb(0xDF, 0xDF, 0xDF));
                BackgroundBorder.Background = new SolidColorBrush(Color.FromRgb(0xB8, 0xB8, 0xB8));
            }
            else
            {
                // Efecto Outset (normal)
                OuterBorder.BorderBrush = new SolidColorBrush(Colors.White);
                OuterBorderDark.BorderBrush = new SolidColorBrush(Colors.Black);
                InnerBorderLight.BorderBrush = new SolidColorBrush(Color.FromRgb(0xDF, 0xDF, 0xDF));
                InnerBorderDark.BorderBrush = new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80));
                BackgroundBorder.Background = new SolidColorBrush(Color.FromRgb(0xC0, 0xC0, 0xC0));
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsLoading && IsEnabled)
            {
                _isPressed = true;
                SetPressedAppearance(true);
                CaptureMouse();
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isPressed)
            {
                _isPressed = false;
                ReleaseMouseCapture();
                
                if (!IsLoading)
                {
                    SetPressedAppearance(false);
                    
                    // Ejecutar comando si es posible
                    if (Command != null && Command.CanExecute(CommandParameter))
                    {
                        Command.Execute(CommandParameter);
                    }
                }
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (_isPressed && !IsLoading)
            {
                SetPressedAppearance(false);
            }
        }

        #endregion
    }
}
