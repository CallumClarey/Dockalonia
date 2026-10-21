using Avalonia;
using Avalonia.Controls;
using DockaloniaMinimal.Tabbing;
using DockaloniaMinimal.Tabbing.TabControls;

namespace DockaloniaMinimal.Docking;

/// <summary>
/// Default window for when a tab is dropped.
/// </summary>
public class DefaultDropoutWindow : Window
{
    private DetachableTabControl _detachableTabControl;
    public DefaultDropoutWindow(DraggingOutTabArgs tabArgs, PixelPoint dropPoint)
    {
        
        // If we can calculate the width and height 
        if(tabArgs.ContentSize is null) return;
        
        var tab = tabArgs.TabDragRef;
        
        // Set the height and width of the window
        var width = tabArgs.ContentSize.Value.Width + tab.Bounds.Width;
        var height = tabArgs.ContentSize.Value.Height + tab.Bounds.Height;
        Width = width;
        Height = height;
        // Set the position
        Position = dropPoint;
        
        // Allow this to be configured
        _detachableTabControl = new DetachableTabControl();
        Content = _detachableTabControl;
        // Tab reference value
        _detachableTabControl.Items.Add(tabArgs.TabDragRef);
    }
    
}