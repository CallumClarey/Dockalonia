using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using DockaloniaMinimal.Tabbing;

namespace DockaloniaMinimal.TabDragging;

/// <summary>
/// Interface to allow a hook for tab dragging. So end users can provide their own functionality.
/// </summary>
public interface ITabDragHandler
{
    /// <summary>
    ///  Property to allow the dragging to be paused.
    ///  Used mainly for the <see cref="DetachableTabControl"/> but could have other uses cases. 
    /// </summary>
    public bool PauseDrag { get; set; }
    
    /// <summary>
    /// Interface used to handle the tab dragging in the <see cref="RearrangeTabControl"/>
    /// Allows user to implement their own dragging logic if the default is undesirable.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="draggedTab"></param>
    /// <param name="tabControl"></param>
    public void HandleTabDragging(
        PointerEventArgs e,
        (TabItem tabItem, Point topLeft) draggedTab,
        RearrangeTabControl tabControl
    );
    
    // Animation hook functions
    //-------------------------------------

    /// <summary>
    ///  Reset the tab item on the drag handler.
    ///  Used primarily for when a tab item is remove or undocked in the middle of a drag.
    /// </summary>
    /// <param name="tabItem">The tab item to reset.</param>
    public void ResetDragging(TabItem tabItem);
    
    
    /// <summary>
    ///  Animation event to hook on tab drag handling.
    /// </summary>
    /// <param name="e"> Pointer event.</param>
    /// <param name="draggedTab"> The dragging tab.</param>
    /// <param name="tabControl"> The tab control that owns the tab.</param>
    public void OnDragStarted(PointerEventArgs e, 
        (TabItem tabItem, Point topLeft) draggedTab, 
        RearrangeTabControl? tabControl = null) { }

    /// <summary>
    ///  
    /// </summary>
    /// <param name="draggedTab"></param>
    /// <param name="fromIndex"></param>
    /// <param name="toIndex"></param>
    /// <param name="tabControl"></param>
    public void OnTabsSwapped(TabItem draggedTab, 
        int fromIndex, int toIndex, 
        RearrangeTabControl tabControl) { }

    /// <summary>
    /// On pointer release. 
    /// </summary>
    /// <param name="draggedTab"></param>
    /// <param name="tabControl"></param>
    public void OnDragCompleted((TabItem tabItem, Point topLeft) draggedTab, 
        RearrangeTabControl? tabControl = null) { }
}