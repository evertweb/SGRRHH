using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SGRRHH.WPF.Controls
{
    /// <summary>
    /// Control de overlay de carga con estilo Legacy (Windows Classic).
    /// Muestra un indicador de progreso con opciones de personalización.
    /// </summary>
    public partial class LegacyLoadingOverlay : UserControl
    {
        private readonly DispatcherTimer _animationTimer;
        private int _animationFrame;
        private readonly string[] _animationFrames = { "[|  ]", "[|| ]", "[|||]", "[ ||]", "[  |]", "[ ||]", "[|||]", "[|| ]" };

        #region Dependency Properties

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(LegacyLoadingOverlay),
                new PropertyMetadata("Procesando..."));

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(double), typeof(LegacyLoadingOverlay),
                new PropertyMetadata(0.0, OnProgressChanged));

        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(LegacyLoadingOverlay),
                new PropertyMetadata(true, OnIsIndeterminateChanged));

        public static readonly DependencyProperty ShowCancelButtonProperty =
            DependencyProperty.Register(nameof(ShowCancelButton), typeof(bool), typeof(LegacyLoadingOverlay),
                new PropertyMetadata(false));

        public static readonly DependencyProperty ShowProgressTextProperty =
            DependencyProperty.Register(nameof(ShowProgressText), typeof(bool), typeof(LegacyLoadingOverlay),
                new PropertyMetadata(false));

        public static readonly DependencyProperty ProgressTextProperty =
            DependencyProperty.Register(nameof(ProgressText), typeof(string), typeof(LegacyLoadingOverlay),
                new PropertyMetadata(string.Empty));

        #endregion

        #region Properties

        /// <summary>
        /// Mensaje a mostrar durante la carga.
        /// </summary>
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        /// <summary>
        /// Valor del progreso (0-100).
        /// </summary>
        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        /// <summary>
        /// Indica si el progreso es indeterminado.
        /// </summary>
        public bool IsIndeterminate
        {
            get => (bool)GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        /// <summary>
        /// Mostrar botón de cancelar.
        /// </summary>
        public bool ShowCancelButton
        {
            get => (bool)GetValue(ShowCancelButtonProperty);
            set => SetValue(ShowCancelButtonProperty, value);
        }

        /// <summary>
        /// Mostrar texto de progreso.
        /// </summary>
        public bool ShowProgressText
        {
            get => (bool)GetValue(ShowProgressTextProperty);
            set => SetValue(ShowProgressTextProperty, value);
        }

        /// <summary>
        /// Texto de progreso personalizado.
        /// </summary>
        public string ProgressText
        {
            get => (string)GetValue(ProgressTextProperty);
            set => SetValue(ProgressTextProperty, value);
        }

        #endregion

        #region Events

        /// <summary>
        /// Evento disparado cuando se hace clic en Cancelar.
        /// </summary>
        public event EventHandler? CancelRequested;

        #endregion

        public LegacyLoadingOverlay()
        {
            InitializeComponent();

            // Timer para animación del icono ASCII
            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(150)
            };
            _animationTimer.Tick += AnimationTimer_Tick;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (IsIndeterminate)
            {
                _animationTimer.Start();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _animationTimer.Stop();
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            _animationFrame = (_animationFrame + 1) % _animationFrames.Length;
            LoadingIcon.Text = _animationFrames[_animationFrame];
        }

        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LegacyLoadingOverlay overlay)
            {
                var progress = (double)e.NewValue;
                overlay.ProgressText = $"{progress:F0}%";
            }
        }

        private static void OnIsIndeterminateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LegacyLoadingOverlay overlay)
            {
                var isIndeterminate = (bool)e.NewValue;
                if (isIndeterminate)
                {
                    overlay._animationTimer.Start();
                }
                else
                {
                    overlay._animationTimer.Stop();
                    overlay.LoadingIcon.Text = "[===]";
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }

        #region Static Helper Methods

        /// <summary>
        /// Muestra un overlay de carga simple.
        /// </summary>
        public static LegacyLoadingOverlay CreateSimple(string message = "Procesando...")
        {
            return new LegacyLoadingOverlay
            {
                Message = message,
                IsIndeterminate = true,
                ShowCancelButton = false,
                ShowProgressText = false
            };
        }

        /// <summary>
        /// Muestra un overlay de carga con progreso.
        /// </summary>
        public static LegacyLoadingOverlay CreateWithProgress(string message, double initialProgress = 0)
        {
            return new LegacyLoadingOverlay
            {
                Message = message,
                IsIndeterminate = false,
                Progress = initialProgress,
                ShowCancelButton = false,
                ShowProgressText = true
            };
        }

        /// <summary>
        /// Muestra un overlay de carga cancelable.
        /// </summary>
        public static LegacyLoadingOverlay CreateCancelable(string message = "Procesando...")
        {
            return new LegacyLoadingOverlay
            {
                Message = message,
                IsIndeterminate = true,
                ShowCancelButton = true,
                ShowProgressText = false
            };
        }

        #endregion
    }
}
