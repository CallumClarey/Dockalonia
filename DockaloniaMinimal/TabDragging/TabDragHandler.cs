using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using DockaloniaMinimal.Tabbing.TabControls;

namespace DockaloniaMinimal.TabDragging;


/// <summary>
///  Class used by default in <see cref="RearrangeTabControl"/> to handle tab dragging.
///  Implements the <see cref="ITabDragHandler"/> interface.
///  It also provides an adjustable threshold to configure the drag with, to tune to your liking.
/// </summary>
public sealed class TabDragHandler(RearrangeTabControl control) : ITabDragHandler
{
    /// <summary>
    /// The <see cref="RearrangeTabControl"/> that owns this drag handler
    /// </summary>
    public RearrangeTabControl? Owner { get; init; } = control;
    
    
    /// <summary>
    /// The percentage which the dragged tab must pass before the swap begins.
    ///  Value clamped between 0 and 1. Ideally do not take this lower than 0.5.
    /// </summary>
    public double DragThreshold { get; set; } = 0.65;
    
    
    /// <summary>
    ///  The offset of the drag from the top left of the tab item.
    /// </summary>
    private Point _grabOffset = new(0, 0);
    
    
    /// <summary>
    /// The transform animation that allows the smooth dragging between tabs.
    /// </summary>
    private TranslateTransform? _translateAnimation;
    
    
    /// <summary>
    ///  On the start of the drag will bring the <see cref="TabItem"/> to the front,
    ///  and then begin a new animation for the smooth drag.
    /// </summary>
    /// <param name="e"> The Pointer event on the drag started.</param>
    /// <param name="draggedTab"> The dragged <see cref="TabItem"/> tuple.</param>
    void ITabDragHandler.OnDragStarted(PointerEventArgs e, (TabItem tabItem, Point topLeft) draggedTab)
    {
        var tab = draggedTab.tabItem;
        _translateAnimation = new TranslateTransform();
        tab.RenderTransform = _translateAnimation;
        tab.ZIndex = 1000;
        _grabOffset = e.GetPosition(Owner) - draggedTab.topLeft;
    }
    
    
    /// <summary>
    ///  Will handle the dragging of the <see cref="TabItem"/> on the mouse cursor move.
    ///  Trigger the animation to move the tab but then also calculate the tab swapping logic.
    /// </summary>
    /// <param name="e"> The pointer event.</param>
    /// <param name="draggedTab"> The dragged tab.</param>
    void ITabDragHandler.HandleTabDragging(PointerEventArgs e, (TabItem tabItem, Point topLeft) draggedTab)
    {
        if (Owner is null) return;

        var tab = draggedTab.tabItem;
        var index = Owner.IndexFromContainer(tab);
        if (index < 0) return;

        tab.SetPseudoClass("dragging", true);

        var horizontal = Owner.TabStripPlacement is Dock.Top or Dock.Bottom;
        double Size(Rect r)  => horizontal ? r.Width : r.Height;
        double Start(Rect r) => horizontal ? r.X : r.Y;

        var pointer = e.GetPosition(Owner);
        var start = (horizontal ? pointer.X - _grabOffset.X : pointer.Y - _grabOffset.Y);
        var end = start + Size(tab.Bounds);

        int? target = null;
        
        // Condition check for the previous tab
        if (index > 0
            && Owner.ContainerFromIndex(index - 1) is { } prev
            && start < Start(prev.Bounds) + Size(prev.Bounds) * (1.0 - DragThreshold))
            target = index - 1;
        // Condition check for the next tab
        else if (index < Owner.ItemCount - 1
                 && Owner.ContainerFromIndex(index + 1) is { } next
                 && end > Start(next.Bounds) + Size(next.Bounds) * DragThreshold)
            target = index + 1;

        // If the target is not null swap the two and invalid visual
        // This will avoid any weird visual bugs
        if (target is { } to)
        {
            Owner.Move(index, to);
            Owner.UpdateLayout();
        }
        TabDraggingAnimation(e, draggedTab, horizontal);
    }
    
    
    /// <summary>
    ///  On the end of the drag will reset all the values and bring the z index back to normal.
    ///  Will also remove the <see cref="IPseudoClasses"/> :dragging.
    /// </summary>
    /// <param name="draggedTab">The tab that has just completed a drag.</param>
    void ITabDragHandler.OnDragCompleted((TabItem tabItem, Point topLeft) draggedTab)
    {
        var tab = draggedTab.tabItem;
        tab.ZIndex = 0;
        tab.RenderTransform = null;
        _translateAnimation = null;
        tab.SetPseudoClass("dragging", false);
    }
    
    
    /// <summary>
    ///  Handles the tab dragging animation and how much the tab should be moved by under the mouse.
    /// </summary>
    /// <param name="e">The pointer event.</param>
    /// <param name="draggedTab">The dragged <see cref="TabItem"/>.</param>
    /// <param name="horizontal">Whether the drag is horizontal.</param>
    private void TabDraggingAnimation(PointerEventArgs e, (TabItem tabItem, Point topLeft) draggedTab, bool horizontal)
    {
        if (_translateAnimation is null || Owner is null) return;

        var tab = draggedTab.tabItem;
        var pointer = e.GetPosition(Owner);

        // Everything below happens along the drag axis only
        var pointerAxis = horizontal ? pointer.X : pointer.Y;
        var grabAxis    = horizontal ? _grabOffset.X : _grabOffset.Y;
        var tabStart    = horizontal ? tab.Bounds.X : tab.Bounds.Y;
        var tabSize     = horizontal ? tab.Bounds.Width : tab.Bounds.Height;

        // Far edge of the strip: the last tab's trailing edge, or the control's size as a fallback
        var stripEnd = Owner.ContainerFromIndex(Owner.ItemCount - 1) is { } last
            ? (horizontal ? last.Bounds.Right : last.Bounds.Bottom)
            : (horizontal ? Owner.Bounds.Width : Owner.Bounds.Height);

        var max = Math.Max(0, stripEnd - tabSize);
        var offset = Math.Clamp(pointerAxis - grabAxis, 0, max) - tabStart;

        _translateAnimation.X = horizontal ? offset : 0;
        _translateAnimation.Y = horizontal ? 0 : offset;
    }
}