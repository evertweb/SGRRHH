using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SGRRHH.WPF.Messages;

public class NavigationMessage : ValueChangedMessage<string>
{
    public object? Parameter { get; }

    public NavigationMessage(string viewName, object? parameter = null) : base(viewName)
    {
        Parameter = parameter;
    }
}
