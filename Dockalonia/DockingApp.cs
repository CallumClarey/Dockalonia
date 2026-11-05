using Avalonia;
using Avalonia.Controls;
using Dockalonia.Docking;
using Dockalonia.Previews;
using Dockalonia.Tabbing;
using Dockalonia.Tabbing.TabControls;

namespace Dockalonia;

/// <summary>
///  Used to hook up the tab undocking and listen for events.
///  Hooks into <see cref="DraggingOutTabArgs"/>oo
/// </summary>
public static class DockingApp 
{
    /// <summary>
    /// Static Instance of the detach handler
    /// </summary>
    public static readonly DetachHandler DetachHandlerInstance = new();
    
    /// <summary>
    /// Initialize the docking event and Hook into <see cref="DetachableTabControl.DetachTabEvent"/>.
    /// </summary>
    public static void InitDocking()
        => DetachableTabControl.DetachTabEvent += HandleDraggedOutTab;
    
    
    /// <summary>
    /// Handles the detach event raised by a tab control by handing the drag over to the
    /// shared <see cref="DetachHandlerInstance"/>.
    /// </summary>
    /// <param name="sender">The tab control that raised the event.</param>
    /// <param name="e">The arguments describing the tab that was dragged out.</param>
    private static void HandleDraggedOutTab(object? sender, DraggingOutTabArgs e)
    {
        // Somehow something other than a tab control has sent this 
        if (sender is not DetachableTabControl tabControl)
            throw new ArgumentException("sender is not a DetachableTabControl", nameof(sender));
        
        // Create a new drag out holder
        DetachHandlerInstance.Enable(e, tabControl);
    }
    
    /// <summary>
    /// Configuration value to assign what preview dock table will be shown.
    /// </summary>
    public static Func<TabItem, TabItem>? PreviewTabDockTemplate { get; set; }
    
    /// <summary>
    /// Configuration for what window template will be used when tabs are dropped out. 
    /// </summary>
    public static Func<DraggingOutTabArgs, PixelPoint, Window>? NewDockDropWindowTemplate { get; set; }
    
    /// <summary>
    /// Configuration that lets the drag out previewer to be changed.
    /// </summary>
    public static Func<DraggingOutTabArgs, PreviewWindow>? DragoutControlTemplate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public static int OverlapDropPadding { get; set; } = 10;
}
