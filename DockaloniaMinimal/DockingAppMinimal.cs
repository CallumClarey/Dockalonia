using Avalonia.Controls;
using DockaloniaMinimal.Docking;
using DockaloniaMinimal.Previews;
using DockaloniaMinimal.Tabbing;

namespace DockaloniaMinimal;

/// <summary>
///  Used to hook up the tab undocking and listen for events.
///  Hooks into <see cref="DraggingOutTabArgs"/>
/// </summary>
public static class DockingAppMinimal 
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
    
    
    // Handle the logic for what to do when a tab is dragged out 
    private static void HandleDraggedOutTab(object? sender, DraggingOutTabArgs e)
    {
        // Somehow something other than a tab control has sent this 
        if (sender is not RearrangeTabControl tabControl)
            throw new ArgumentException("sender is not a RearrangeTabControl", nameof(sender));
        
        // Create a new drag out holderl
        DetachHandlerInstance.Enable(e);
    }
    
    /// <summary>
    /// Configuration value to assign what preview dock table will be shown.
    /// </summary>
    public static Func<TabItem, TabItem>? PreviewTabDockTemplate { get; set; }
    
    /// <summary>
    /// Configuration for what window template will be used when tabs are dropped out. 
    /// </summary>
    public static Func<DraggingOutTabArgs, Window>? NewDockDropWindowTemplate { get; set; }
    
    /// <summary>
    /// Configuration that lets the drag out previewer to be changed
    /// </summary>
    public static Func<DraggingOutTabArgs, PreviewWindow>? DragoutControlTemplate { get; set; }
    
}
