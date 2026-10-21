using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using DockaloniaMinimal.Docking;
using DockaloniaMinimal.TabDragging;

namespace DockaloniaMinimal.Tabbing.TabControls;


/// <summary>
///  Control will introduce a <see cref="ITabDragHandler"/> to implement tab dragging.
///  Users can override this functionality by providing a customer <see cref="ITabDragHandler"/>.
///  If this is not provided it will use the <see cref="TabDragHandler"/>.
/// </summary>
public class RearrangeTabControl : DockaloniaTabControl
{
    // Will style like a normal tab control
    protected override Type StyleKeyOverride => typeof(TabControl);
    
    /// <summary>
    ///  The backing avalonia property for the <see cref="DragHandler"/>.
    ///  Allows us to change the drag handler in XAML.
    /// </summary>
    public static readonly StyledProperty<ITabDragHandler> DragHandlerProperty =
        AvaloniaProperty.Register<RearrangeTabControl, ITabDragHandler>(nameof(DragHandler));

    
    /// <summary>
    /// The handler responsible for tab-dragging behavior. Set this to supply custom drag logic.
    /// </summary>
    public ITabDragHandler DragHandler
    {
        get => GetValue(DragHandlerProperty);
        set => SetValue(DragHandlerProperty, value);
    }
    
    
    /// <summary>
    /// Set the class to be hit test visible on initialization.
    /// </summary>
    public RearrangeTabControl()
    {
        DragHandler = new TabDragHandler(this);
        IsHitTestVisible = true;
    }

    /// <summary>
    ///  The tuple value that stores the currently dragged <see cref="TabItem"/>.
    ///  This should always be a <see cref="DockaloniaTab"/>.
    /// </summary>
    protected (TabItem tabItem, Point offset)? DraggedTab;
    
    
    /// <summary>
    /// Handles behavior for getting the tab on the click and starting the tab dragging event.
    /// </summary>
    /// <param name="e"> The pointer pressed event.</param>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var hitPoint = e.GetPosition(this);
        
        // Return if it is not a tab item
        if(!GetTabItemAt(hitPoint, out var tabItem, out _)) return;
        
        // Only select visible tabs
        if(tabItem!.IsVisible) SelectedItem = tabItem;
        
        var topLeft = tabItem.TranslatePoint(new Point(0, 0), this)!.Value;
        DraggedTab = (tabItem, topLeft);
        DragHandler.OnDragStarted(e ,DraggedTab.Value);
    }
    
    
    /// <summary>
    ///  Releases the dragged tab on pointer release and calls the drag handler to do the same.
    /// </summary>
    /// <param name="e">The pointer released event.</param>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if(DraggedTab is not null) DragHandler.OnDragCompleted(DraggedTab.Value);
        DraggedTab = null;
    }
    
    
    /// <summary>
    ///  Function called on the pointer moved will let trigger the tab <see cref="ITabDragHandler"/>,
    ///  To handle the dragging logic for the control. 
    /// </summary>
    /// <param name="e"> The moving pointer event arguments.</param>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (DraggedTab == null) return;
    
        // Call function to rearrange tab rearrange the tab 
        DragHandler.HandleTabDragging(e, DraggedTab!.Value);
    }
}  