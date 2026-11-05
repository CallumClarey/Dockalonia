using Avalonia;
using Avalonia.Controls;
using Dockalonia.Tabbing;
using Dockalonia.Tabbing.TabControls;

namespace Dockalonia.Docking;

/// <summary>
/// Default window for when a tab is dropped.
/// </summary>
public class DefaultDropoutWindow : Window
{
    private readonly DetachableTabControl _detachableTabControl;
    /// <summary>
    /// Creates a window that hosts the dropped tab at the given screen position.
    /// </summary>
    /// <param name="tabArgs">The arguments describing the tab that was dragged out.</param>
    /// <param name="dropPoint">The screen position at which the window should appear.</param>
    public DefaultDropoutWindow(DraggingOutTabArgs tabArgs, PixelPoint dropPoint)
    {
        
        // If we can calculate the width and height 
        if(tabArgs.ContentSize is null) return;
        
        var tab = tabArgs.TabDragRef;
        
        // Set the height and width of the window
        var width = tabArgs.ContentSize.Value.Width;
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