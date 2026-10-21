using Avalonia.Controls;
using Avalonia.Input;

namespace DockaloniaMinimal.Previews;

/// <summary>
/// Abstract middle class that represents the special requires of a docking preview.
/// </summary>
public abstract class PreviewWindow : Window
{
    /// <summary>
    /// The required content container for default movement config.
    /// </summary>
    public required Border ContentContainer { get; init; }
    
    /// <summary>
    /// Will delegate moving of the preview to inherit the control.
    /// </summary>
    /// <param name="e"></param>
    public virtual void UpdatePosition(PointerEventArgs e){}
}