import shutil

# Copiar el nuevo XAML que creé previamente
src = r"C:\Users\evert\.gemini\antigravity\brain\78ca7dc6-bcbe-4074-b32f-312059e07289\LoginWindow_new.xaml"
dst = r"c:\Users\evert\Documents\rrhh\src\SGRRHH.WPF\Views\LoginWindow.xaml"

# Leer el contenido del XAML mejorado (en artifacts)
# Si no existe, lo creo aquí directamente

xaml_content = '''<Window x:Class="SGRRHH.WPF.Views.LoginWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        mc:Ignorable="d"
        Title="SGRRHH - Iniciar Sesión"
        Height="500" Width="420"
        WindowStartupLocation="CenterScreen"
        WindowStyle="None"
        AllowsTransparency="True"
        Background="Transparent"
        ResizeMode="NoResize">
    
    <Window.Resources>
        <!-- Animación de entrada -->
        <Storyboard x:Key="LoadAnimation">
            <DoubleAnimation Storyboard.TargetName="MainBorder" 
                             Storyboard.TargetProperty="Opacity"
                             From="0" To="1" Duration="0:0:0.4"/>
            <DoubleAnimation Storyboard.TargetName="MainBorder"
                             Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)"
                             From="0.95" To="1" Duration="0:0:0.3">
                <DoubleAnimation.EasingFunction>
                    <CubicEase EasingMode="EaseOut"/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
            <DoubleAnimation Storyboard.TargetName="MainBorder"
                             Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleY)"
                             From="0.95" To="1" Duration="0:0:0.3">
                <DoubleAnimation.EasingFunction>
                    <CubicEase EasingMode="EaseOut"/>
                </DoubleAnimation.EasingFunction>
            </DoubleAnimation>
        </Storyboard>
    </Window.Resources>
    
    <Window.Triggers>
        <EventTrigger RoutedEvent="Loaded">
            <BeginStoryboard Storyboard="{StaticResource LoadAnimation}"/>
        </EventTrigger>
    </Window.Triggers>
    
    <Border x:Name="MainBorder" Background="#FAFAFA" CornerRadius="15" 
            BorderBrush="#1E88E5" BorderThickness="0"
            RenderTransformOrigin="0.5,0.5">
        <Border.RenderTransform>
            <ScaleTransform />
        </Border.RenderTransform>
        <Border.Effect>
            <DropShadowEffect Color="#000000" Opacity="0.25" 
                              BlurRadius="25" ShadowDepth="8"
                              Direction="270"/>
        </Border.Effect>
        
        <Grid>
            <Grid.RowDefinitions>
                <RowDefinition Height="Auto"/>
                <RowDefinition Height="*"/>
            </Grid.RowDefinitions>
            
            <!-- Barra de título con gradiente -->
            <Border Grid.Row="0" CornerRadius="15,15,0,0" 
                    MouseLeftButtonDown="TitleBar_MouseLeftButtonDown">
                <Border.Background>
                    <LinearGradientBrush StartPoint="0,0" EndPoint="1,1">
                        <GradientStop Color="#1E88E5" Offset="0"/>
                        <GradientStop Color="#1565C0" Offset="1"/>
                    </LinearGradientBrush>
                </Border.Background>
                <Grid Margin="20,15">
                    <TextBlock Text="SGRRHH" FontSize="20" FontWeight="Bold" 
                               Foreground="White" HorizontalAlignment="Left"/>
                    <Button Content="✕" HorizontalAlignment="Right" 
                            Background="Transparent" BorderThickness="0"
                            Foreground="White" FontSize="18" Cursor="Hand"
                            Command="{Binding ExitCommand}"
                            Padding="8,0">
                        <Button.Style>
                            <Style TargetType="Button">
                                <Setter Property="Template">
                                    <Setter.Value>
                                        <ControlTemplate TargetType="Button">
                                            <Border Background="{TemplateBinding Background}" 
                                                    CornerRadius="3">
                                                <ContentPresenter HorizontalAlignment="Center" 
                                                                  VerticalAlignment="Center"/>
                                            </Border>
                                        </ControlTemplate>
                                    </Setter.Value>
                                </Setter>
                                <Style.Triggers>
                                    <Trigger Property="IsMouseOver" Value="True">
                                        <Setter Property="Background" Value="#40FFFFFF"/>
                                    </Trigger>
                                </Style.Triggers>
                            </Style>
                        </Button.Style>
                    </Button>
                </Grid>
            </Border>
            
            <!-- Contenido -->
            <StackPanel Grid.Row="1" Margin="45,40,45,35" VerticalAlignment="Center">
                
                <!-- Logo/Ícono -->
                <Border Width="90" Height="90" CornerRadius="45" 
                        Background="#1E88E5" Margin="0,0,0,15"
                        HorizontalAlignment="Center">
                    <Border.Effect>
                        <DropShadowEffect Color="#1E88E5" Opacity="0.4" 
                                          BlurRadius="15" ShadowDepth="3"/>
                    </Border.Effect>
                    <TextBlock Text="👤" FontSize="50" 
                               HorizontalAlignment="Center" 
                               VerticalAlignment="Center"/>
                </Border>
                
                <TextBlock Text="Iniciar Sesión" FontSize="26" FontWeight="SemiBold" 
                           Foreground="#263238" HorizontalAlignment="Center" Margin="0,0,0,35"/>
                
                <!-- Usuario -->
                <TextBlock Text="Usuario" Foreground="#546E7A" FontSize="13" 
                           FontWeight="Medium" Margin="2,0,0,6"/>
                <Grid Margin="0,0,0,18">
                    <TextBox x:Name="UsernameTextBox"
                             Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}"
                             Padding="12,11" FontSize="14"
                             BorderBrush="#CFD8DC" BorderThickness="1.5">
                        <TextBox.Style>
                            <Style TargetType="TextBox">
                                <Setter Property="Background" Value="White"/>
                                <Setter Property="Template">
                                    <Setter.Value>
                                        <ControlTemplate TargetType="TextBox">
                                            <Border Background="{TemplateBinding Background}" 
                                                    BorderBrush="{TemplateBinding BorderBrush}"
                                                    BorderThickness="{TemplateBinding BorderThickness}"
                                                    CornerRadius="6">
                                                <ScrollViewer x:Name="PART_ContentHost" 
                                                              Margin="{TemplateBinding Padding}"/>
                                            </Border>
                                        </ControlTemplate>
                                    </Setter.Value>
                                </Setter>
                                <Style.Triggers>
                                    <Trigger Property="IsFocused" Value="True">
                                        <Setter Property="BorderBrush" Value="#1E88E5"/>
                                        <Setter Property="BorderThickness" Value="2"/>
                                    </Trigger>
                                </Style.Triggers>
                            </Style>
                        </TextBox.Style>
                    </TextBox>
                    <!-- Placeholder mejorado -->
                    <TextBlock Text="usuario@sgrrhh.local" Foreground="#90A4AE" FontSize="14"
                               Margin="14,11,0,0" IsHitTestVisible="False"
                               Visibility="{Binding Username, Converter={StaticResource IsNullOrEmptyToVisibilityConverter}}"/>
                </Grid>
                
                <!-- Contraseña -->
                <TextBlock Text="Contraseña" Foreground="#546E7A" FontSize="13" 
                           FontWeight="Medium" Margin="2,0,0,6"/>
                <PasswordBox x:Name="PasswordBox" 
                             Padding="12,11" FontSize="14" Margin="0,0,0,18"
                             BorderBrush="#CFD8DC" BorderThickness="1.5"
                             PasswordChanged="PasswordBox_PasswordChanged">
                    <PasswordBox.Style>
                        <Style TargetType="PasswordBox">
                            <Setter Property="Background" Value="White"/>
                            <Setter Property="Template">
                                <Setter.Value>
                                    <ControlTemplate TargetType="PasswordBox">
                                        <Border Background="{TemplateBinding Background}" 
                                                BorderBrush="{TemplateBinding BorderBrush}"
                                                BorderThickness="{TemplateBinding BorderThickness}"
                                                CornerRadius="6">
                                            <ScrollViewer x:Name="PART_ContentHost" 
                                                          Margin="{TemplateBinding Padding}"/>
                                        </Border>
                                    </ControlTemplate>
                                </Setter.Value>
                            </Setter>
                            <Style.Triggers>
                                <Trigger Property="IsFocused" Value="True">
                                    <Setter Property="BorderBrush" Value="#1E88E5"/>
                                    <Setter Property="BorderThickness" Value="2"/>
                                </Trigger>
                            </Style.Triggers>
                        </Style>
                    </PasswordBox.Style>
                </PasswordBox>
                
                <!-- Mensaje de error -->
                <Border Background="#FFEBEE" CornerRadius="6" Padding="12,10" Margin="0,0,0,18"
                        Visibility="{Binding HasError, Converter={StaticResource BoolToVisibilityConverter}}">
                    <TextBlock Text="{Binding ErrorMessage}" Foreground="#C62828" 
                               TextWrapping="Wrap" FontSize="12.5"/>
                </Border>
                
                <!-- Botón de Login con animaciones -->
                <Button x:Name="LoginButton" Content="Iniciar Sesión" 
                        Command="{Binding LoginCommand}"
                        IsEnabled="{Binding IsLoading, Converter={StaticResource InverseBoolConverter}}"
                        Padding="0,14" FontSize="15" FontWeight="SemiBold"
                        Cursor="Hand" Margin="0,8,0,0"
                        RenderTransformOrigin="0.5,0.5">
                    <Button.RenderTransform>
                        <ScaleTransform />
                    </Button.RenderTransform>
                    <Button.Triggers>
                        <EventTrigger RoutedEvent="Button.Click">
                            <BeginStoryboard>
                                <Storyboard>
                                    <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleX)"
                                                     From="1" To="0.96" Duration="0:0:0.08" AutoReverse="True"/>
                                    <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(ScaleTransform.ScaleY)"
                                                     From="1" To="0.96" Duration="0:0:0.08" AutoReverse="True"/>
                                </Storyboard>
                            </BeginStoryboard>
                        </EventTrigger>
                    </Button.Triggers>
                    <Button.Style>
                        <Style TargetType="Button">
                            <Setter Property="Background" Value="#1E88E5"/>
                            <Setter Property="Foreground" Value="White"/>
                            <Setter Property="BorderThickness" Value="0"/>
                            <Setter Property="Template">
                                <Setter.Value>
                                    <ControlTemplate TargetType="Button">
                                        <Border x:Name="ButtonBorder" 
                                                Background="{TemplateBinding Background}" 
                                                CornerRadius="6" 
                                                Padding="{TemplateBinding Padding}">
                                            <Border.Effect>
                                                <DropShadowEffect x:Name="ButtonShadow" 
                                                                  Color="#1E88E5" Opacity="0.3" 
                                                                  BlurRadius="12" ShadowDepth="3"/>
                                            </Border.Effect>
                                            <ContentPresenter HorizontalAlignment="Center" 
                                            VerticalAlignment="Center"/>
                                        </Border>
                                        <ControlTemplate.Triggers>
                                            <Trigger Property="IsMouseOver" Value="True">
                                                <Trigger.EnterActions>
                                                    <BeginStoryboard>
                                                        <Storyboard>
                                                            <ColorAnimation Storyboard.TargetName="ButtonBorder"
                                                                            Storyboard.TargetProperty="(Border.Background).(SolidColorBrush.Color)"
                                                                            To="#1976D2" Duration="0:0:0.2"/>
                                                            <DoubleAnimation Storyboard.TargetName="ButtonShadow"
                                                                             Storyboard.TargetProperty="BlurRadius"
                                                                             To="18" Duration="0:0:0.2"/>
                                                        </Storyboard>
                                                    </BeginStoryboard>
                                                </Trigger.EnterActions>
                                                <Trigger.ExitActions>
                                                    <BeginStoryboard>
                                                        <Storyboard>
                                                            <ColorAnimation Storyboard.TargetName="ButtonBorder"
                                                                            Storyboard.TargetProperty="(Border.Background).(SolidColorBrush.Color)"
                                                                            To="#1E88E5" Duration="0:0:0.2"/>
                                                            <DoubleAnimation Storyboard.TargetName="ButtonShadow"
                                                                             Storyboard.TargetProperty="BlurRadius"
                                                                             To="12" Duration="0:0:0.2"/>
                                                        </Storyboard>
                                                    </BeginStoryboard>
                                                </Trigger.ExitActions>
                                            </Trigger>
                                            <Trigger Property="IsEnabled" Value="False">
                                                <Setter TargetName="ButtonBorder" Property="Background" Value="#BDBDBD"/>
                                                <Setter TargetName="ButtonShadow" Property="Opacity" Value="0.1"/>
                                            </Trigger>
                                        </ControlTemplate.Triggers>
                                    </ControlTemplate>
                                </Setter.Value>
                            </Setter>
                        </Style>
                    </Button.Style>
                </Button>
                
                <!-- Indicador de carga animado -->
                <StackPanel Orientation="Horizontal" HorizontalAlignment="Center" 
                            Margin="0,18,0,0"
                            Visibility="{Binding IsLoading, Converter={StaticResource BoolToVisibilityConverter}}">
                    <TextBlock Text="⏳" FontSize="14" Margin="0,0,6,0">
                        <TextBlock.RenderTransform>
                            <RotateTransform x:Name="LoadingRotation" CenterX="7" CenterY="7"/>
                        </TextBlock.RenderTransform>
                        <TextBlock.Triggers>
                            <EventTrigger RoutedEvent="Loaded">
                                <BeginStoryboard>
                                    <Storyboard RepeatBehavior="Forever">
                                        <DoubleAnimation Storyboard.TargetName="LoadingRotation"
                                                         Storyboard.TargetProperty="Angle"
                                                         From="0" To="360" Duration="0:0:2"/>
                                    </Storyboard>
                                </BeginStoryboard>
                            </EventTrigger>
                        </TextBlock.Triggers>
                    </TextBlock>
                    <TextBlock Text="Verificando credenciales..." Foreground="#78909C" FontSize="13"/>
                </StackPanel>
                
            </StackPanel>
        </Grid>
    </Border>
</Window>'''

with open(dst, 'w', encoding='utf-8') as f:
    f.write(xaml_content)

print("✅ LoginWindow.xaml mejorado aplicado exitosamente!")
print("✨ Mejoras aplicadas:")
print("  - Sombra elevada al diálogo")
print("  - Gradiente en barra de título") 
print("  - Animación de entrada")
print("  -Animación de click en botón")
print("  - Indicador de carga animado")
print("  - Estilos modernos con bordes redondeados")
