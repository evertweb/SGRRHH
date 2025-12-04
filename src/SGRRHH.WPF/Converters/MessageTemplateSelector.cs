using System.Windows;
using System.Windows.Controls;
using SGRRHH.Core.Interfaces;

namespace SGRRHH.WPF.Converters;

/// <summary>
/// Selector de template para mensajes de chat.
/// Elige entre template de mensaje enviado (mío) o recibido.
/// </summary>
public class MessageTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// Template para mensajes enviados por el usuario actual
    /// </summary>
    public DataTemplate? MyMessageTemplate { get; set; }
    
    /// <summary>
    /// Template para mensajes recibidos de otros usuarios
    /// </summary>
    public DataTemplate? ReceivedMessageTemplate { get; set; }
    
    public override DataTemplate? SelectTemplate(object item, DependencyObject container)
    {
        if (item is SendbirdMessage message)
        {
            return message.IsMyMessage ? MyMessageTemplate : ReceivedMessageTemplate;
        }
        
        return base.SelectTemplate(item, container);
    }
}
