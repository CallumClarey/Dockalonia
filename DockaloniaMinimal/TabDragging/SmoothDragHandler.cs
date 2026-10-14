using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using DockaloniaMinimal.Tabbing;

namespace DockaloniaMinimal.TabDragging;

/// <summary>
/// Class to handle smooth dragging of tabs with hysteresis swap thresholding.
/// </summary>
public class SmoothDragHandler : ITabDragHandler
{
    /// <summary>
    /// The percentage which the dragged tab must pass before the swap begins.
    ///  Value clamped between 0 and 1. Ideally do not take this lower than 0.5.
    /// </summary>
    public double DragThreshold { get; set; } = 0.65;

    /// <summary>
    ///  Controls whether the drag can be activated or not,
    ///  setting this to false will turn off the dragging.
    /// </summary>
    public bool PauseDrag { get; set; } = false;
    
    
    private Point _grabOffset = new(0, 0);
    private TranslateTransform? _translateAnimation;


    public void HandleTabDragging(PointerEventArgs e, (TabItem tabItem, Point topLeft) draggedTab, RearrangeTabControl tabControl)
    {
        if(PauseDrag)
        {
            if(_translateAnimation is null) return;
            _translateAnimation.X = 0;
            _translateAnimation.Y = 0;
            return;
        }
        
        // Get the dragged tab and its index 
        var tab = draggedTab.tabItem;
        var index = tabControl.IndexFromContainer(tab);
        if (index < 0) return;

        // Set this line so dragging tabs can be styled 
        if (!(tab.Classes as IPseudoClasses).Contains(":dragging"))
            (tab.Classes as IPseudoClasses).Set(":dragging", true);
        
        
        // Decide whether tab dragging is horizontal or vertical
        var horizontal = tabControl.TabStripPlacement is Dock.Top or Dock.Bottom;

        // 1. Update visual position first
        TabDraggingAnimation(e, draggedTab, tabControl, horizontal);

        // Calculate dragged tab's current world-space bounds
        var currentPointer = e.GetPosition(tabControl);
        var draggedBounds = horizontal
            ? new Rect(currentPointer.X - _grabOffset.X, tab.Bounds.Y, tab.Bounds.Width, tab.Bounds.Height)
            : new Rect(tab.Bounds.X, currentPointer.Y - _grabOffset.Y, tab.Bounds.Width, tab.Bounds.Height);

        // 2. Dragging Left / Up (Towards previous tab)
        if (index > 0 && tabControl.ContainerFromIndex(index - 1) is { } prev)
        {
            var prevBounds = prev.Bounds;
            var thresholdPoint = horizontal
                ? prevBounds.X + (prevBounds.Width * (1.0 - DragThreshold))
                : prevBounds.Y + (prevBounds.Height * (1.0 - DragThreshold));

            var draggedLeadingEdge = horizontal ? draggedBounds.X : draggedBounds.Y;

            if (draggedLeadingEdge < thresholdPoint)
            {
                tabControl.Move(index, index - 1);
                OnTabsSwapped(tab, index, index - 1, tabControl);
                TabDraggingAnimation(e, draggedTab, tabControl, horizontal);
                return;
            }
        }

        // 3. Dragging Right / Down (Towards next tab)
        if (index >= tabControl.ItemCount - 1 || tabControl.ContainerFromIndex(index + 1) is not { } next) return;
        {
            var nextBounds = next.Bounds;
            var thresholdPoint = horizontal
                ? nextBounds.X + (nextBounds.Width * DragThreshold)
                : nextBounds.Y + (nextBounds.Height * DragThreshold);

            var draggedTrailingEdge = horizontal ? draggedBounds.Right : draggedBounds.Bottom;

            if (!(draggedTrailingEdge > thresholdPoint)) return;
            tabControl.Move(index, index + 1);
            OnTabsSwapped(tab, index, index + 1, tabControl);
            TabDraggingAnimation(e, draggedTab, tabControl, horizontal);
        }
    }

    /// <summary>
    ///  Reset the tab after the drag.
    /// </summary>
    /// <param name="tab"></param>
    public void ResetDragging(TabItem tab)
    {
        tab.RenderTransform = null;
        _translateAnimation = null;
        tab.ZIndex = 0;
        (tab.Classes as IPseudoClasses).Set(":dragging", false);
    }
    
    /// <summary>
    /// Bring the tab forward on the drag start and then calculate the grab offset.
    /// </summary>
    /// <param name="e"><see cref="PointerEventArgs"/> The pointer of the drag. </param>
    /// <param name="draggedTab"></param>
    /// <param name="tabControl"></param>
    public void OnDragStarted(PointerEventArgs e, (TabItem tabItem, Point topLeft) draggedTab, RearrangeTabControl? tabControl)
    {
        var tab = draggedTab.tabItem;
        _translateAnimation = new TranslateTransform();
        tab.RenderTransform = _translateAnimation;
        tab.ZIndex = 1000;
        _grabOffset = e.GetPosition(tabControl) - draggedTab.topLeft;
    }

    // Update layout when the tab swaps to avoid jumping
    public void OnTabsSwapped(TabItem draggedTab, int fromIndex, int toIndex, RearrangeTabControl tabControl)
    => tabControl.UpdateLayout();
    
    
    public void OnDragCompleted((TabItem tabItem, Point topLeft) draggedTab, RearrangeTabControl? tabControl)
    {
        var tab = draggedTab.tabItem;
        tab.RenderTransform = null;
        _translateAnimation = null;
        tab.ZIndex = 0;
        (tab.Classes as IPseudoClasses).Set(":dragging", false);
    }

    private void TabDraggingAnimation(
        PointerEventArgs e, (TabItem tabItem, Point topLeft) draggedTab, RearrangeTabControl tabControl, bool horizontal)
    {
        if (_translateAnimation == null) return;

        var tab = draggedTab.tabItem;
        var pointerPos = e.GetPosition(tabControl);

        // Get the container for the last tab item
        var lastTabContainer = tabControl.ContainerFromIndex(tabControl.ItemCount - 1) as TabItem;

        if (horizontal)
        {
            // Calculate max right edge based on the last tab's position, falling back to control width
            var maxRightEdge = lastTabContainer != null 
                ? lastTabContainer.Bounds.Right 
                : tabControl.Bounds.Width;

            var maxX = maxRightEdge - tab.Bounds.Width;
            var desiredX = Math.Clamp(pointerPos.X - _grabOffset.X, 0, Math.Max(0, maxX));
        
            _translateAnimation.X = desiredX - tab.Bounds.X;
            _translateAnimation.Y = 0;
        }
        else
        {
            // Calculate max bottom edge based on the last tab's position
            var maxBottomEdge = lastTabContainer != null 
                ? lastTabContainer.Bounds.Bottom 
                : tabControl.Bounds.Height;

            var maxY = maxBottomEdge - tab.Bounds.Height;
            var desiredY = Math.Clamp(pointerPos.Y - _grabOffset.Y, 0, Math.Max(0, maxY));
        
            _translateAnimation.Y = desiredY - tab.Bounds.Y;
            _translateAnimation.X = 0;
        }
    }
}