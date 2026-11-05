using Avalonia.Controls;
using Dockalonia.Docking;

namespace Dockalonia.Testing;

/// <summary>
/// Sample dockable control used to try out the docking API; logs the docking callbacks it receives.
/// </summary>
public class TestPanel : TextBox, IDockingControl
{
    protected override Type StyleKeyOverride => typeof(TextBox);

    /// <summary>
    /// Called when a tab is dropped onto this control. Logs the call only.
    /// </summary>
    /// <param name="args">The arguments describing the tab being dragged.</param>
    void IDockingControl.HandleDocking(DraggingOutTabArgs args)
    {
        Console.WriteLine($"{nameof(IDockingControl.HideDockingPreview)} called");
    }

    /// <summary>
    /// Called while a dragged tab hovers this control. Logs the call only.
    /// </summary>
    /// <param name="args">The arguments describing the tab being dragged.</param>
    void IDockingControl.DisplayDockingPreview(DraggingOutTabArgs args)
    {
        Console.WriteLine($"{nameof(IDockingControl.DisplayDockingPreview)} called");
    }

    /// <summary>
    /// Called when the dragged tab stops hovering this control. Logs the call only.
    /// </summary>
    /// <param name="args">The arguments describing the tab being dragged.</param>
    void IDockingControl.HideDockingPreview(DraggingOutTabArgs args)
    {
        Console.WriteLine($"{nameof(IDockingControl.HideDockingPreview)} called");
    }
}