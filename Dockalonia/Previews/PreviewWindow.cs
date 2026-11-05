using Avalonia.Controls;
using Avalonia.Input;
using Dockalonia.Docking;

namespace Dockalonia.Previews;

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
    /// Will fire a function when hovering a dockable control.
    /// </summary>
    /// <param name="e"></param>
    public virtual void OnHoveringDockable(DraggingOutTabArgs e){}
    
    /// <summary>
    ///  Will fire a function when the control is not hoving a dockable control. 
    /// </summary>
    /// <param name="e"></param>
    public virtual void OnNotHoveringDockable(DraggingOutTabArgs e){}
    
}