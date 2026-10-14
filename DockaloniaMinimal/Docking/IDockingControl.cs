using DockaloniaMinimal.Tabbing;

namespace DockaloniaMinimal.Docking;

/// <summary>
///  Donates the control as a dockable control
/// </summary>
public interface IDockingControl
{
    /// <summary>
    ///  Will handle the docking logic for the control.
    /// </summary>
    /// <param name="args"></param>
    public void HandleDocking(DraggingOutTabArgs args);
    
    /// <summary>
    ///  Will handle the preview for the dock.
    /// </summary>
    /// <param name="args"></param>
    public void DisplayDockingPreview(DraggingOutTabArgs args);
    
    /// <summary>
    ///  Will handle hiding the preview when the control is not over.
    /// </summary>
    /// <param name="args"></param>
    public void HideDockingPreview(DraggingOutTabArgs args);
}