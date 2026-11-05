using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Dockalonia.Docking;
using Dockalonia.Previews;
using Dockalonia.Tabbing.Tabs;

namespace Dockalonia.Tabbing.TabControls;


/// <summary>
/// An extension of the <see cref="RearrangeTabControl"/>
/// </summary>
public class DetachableTabControl : RearrangeTabControl, IDockingControl
{
    // Allows for this to use all of tab controls styles
    protected override Type StyleKeyOverride => typeof(TabControl);
    
    /// <summary>
    /// Property to add a margin to how much the mouse needs to leave the control by before undocking begins.
    /// </summary>
    public static readonly StyledProperty<double> DragOutMarginProperty =
        AvaloniaProperty.Register<RearrangeTabControl, double>(
            nameof(DragOutMarginProperty),
            defaultValue: 5);
 
    /// <summary>
    ///  How far the mouse must leave the control before undocking begins.
    /// </summary>
    public double DragOutMargin
    {
        get => GetValue(DragOutMarginProperty);
        set => SetValue(DragOutMarginProperty, value);
    }
    
    /// <summary>
    ///  Avalonia property to control whether this will close a window when empty.
    /// </summary>
    public static readonly StyledProperty<bool> CloseParentWindowWhenEmptyProperty =
        AvaloniaProperty.Register<DockaloniaTabControl, bool>
            (nameof(CloseParentWindowWhenEmpty), 
                defaultValue: true);

    /// <summary>
    ///  Backing property to control whether the control closes parent window on empty.
    /// </summary>
    public bool CloseParentWindowWhenEmpty
    {
        get => GetValue(CloseParentWindowWhenEmptyProperty);
        set => SetValue(CloseParentWindowWhenEmptyProperty, value);
    }
    
    /// <summary>
    ///  Event for when the tab is dragged out of the window.
    /// </summary>
    public static event EventHandler<DraggingOutTabArgs>? DetachTabEvent;
    
    /// <summary>
    /// Tab item used to preview reattach positioning. 
    /// </summary>
    public TabItem? PreviewTabItem;
    
    /// <summary>
    ///  Checks for detachment on every pointer move while a tab is being dragged.
    /// </summary>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        // Check if the tab should be detached
        if(CheckForDetachment(e) && DraggedTab is not null) DetachTab(e);
        base.OnPointerMoved(e);
    }


    /// <summary>
    /// Function used to validate that the <see cref="TabItem"/> should be detached from the control.
    /// </summary>
    /// <param name="e">Pointer event of the drag.</param>
    /// <param name="dragEvent"></param>
    /// <returns> Whether the pointer fall within the detachment area.</returns>
    private bool CheckForDetachment(PointerEventArgs e, DraggingOutTabArgs? dragEvent = null)
    {
        var hitPoint = e.GetPosition(this);
        var gotTabBarRect = TryGetTabBarRect(out var rect);
        rect = rect.Inflate(DragOutMargin);
        var tabBarHovered = gotTabBarRect && rect.Contains(hitPoint);
        return !tabBarHovered;
    }
    

    /// <summary>
    /// Function called to remove <see cref="TabItem"/> from this tab control.
    /// Will also invoke the <see cref="DetachTabEvent"/>. 
    /// </summary>
    /// <param name="e"> The pointer event of the drag. </param>
    private void DetachTab(PointerEventArgs e)
    {
        if(DraggedTab is null) return;
        
        DragHandler.OnDragCompleted(DraggedTab.Value);
        var (tabItem, offset) = DraggedTab.Value;
        
        // Pinned tabs cannot be detached out of the dockspace.
        if (tabItem is PinnableTab { IsPinned: true })
        {
            DraggedTab = null;
            return;
        }
        
        var tabIndex = Items.IndexOf(tabItem);
        if (tabIndex > 0)
            SelectedItem = Items[tabIndex - 1];
        else if (Items.Count > 1)
            SelectedItem = Items[1];
        
        Items.Remove(tabItem);
        DraggedTab = null;
        
        var newTabDraggedEvent = new DraggingOutTabArgs(e, tabItem, offset,
            ExposedContentPresenterPart?.Bounds.Size ?? Bounds.Size);
        
        DetachTabEvent?.Invoke(this, newTabDraggedEvent);
        
        // if(Items.Count > 0 ) return;
        // if(!CloseParentWindowWhenEmpty) return;
        // DockingApp.DetachHandlerInstance.CloseWindowOnEmptyControl(this);
    }
    
    
    // All docking logic here 
    //----------------------------------------
    
    /// <summary>
    ///  Returns the index of the tab under the pointer.
    /// </summary>
    private int GetIndexUnderPointer(PointerEventArgs args)
    {
        var position = args.GetPosition(this);
        GetTabItemAt(position, out _,out var index);
        return index;
    }
    
    /// <summary>
    ///  Drops the dragged tab into this control at the position under the pointer.
    ///  Pinned tabs always occupy the start of the strip, so the tab lands at the first slot after them at the earliest.
    /// </summary>
    void IDockingControl.HandleDocking(DraggingOutTabArgs args)
    {
        var indexUnderPointer = GetIndexUnderPointer(args.Pointer);
        var insertIndex = Math.Clamp(Math.Max(PinnedCount, indexUnderPointer < 0 ? Items.Count : indexUnderPointer), 0, Items.Count);
        Items.Insert(insertIndex, args.TabDragRef);
        SelectedItem = args.TabDragRef;
        InvalidateVisual();
    }

    /// <summary>
    ///  Shows a preview of where the dragged tab will be dropped.
    ///  The preview never sits inside the pinned region, it lands at the first slot after it.
    /// </summary>
    void IDockingControl.DisplayDockingPreview(DraggingOutTabArgs args)
    {
        var position = args.Pointer.GetPosition(this);
        GetTabItemAt(position, out var tabItem,out var rawIndex);
        
        PreviewTabItem ??= 
            DockingApp.PreviewTabDockTemplate?.Invoke(args.TabDragRef) 
            ?? new PreviewDropTab(args.TabDragRef);
        
        var currentIndex = Items.IndexOf(PreviewTabItem);
        if (currentIndex == -1) 
        {
            // Insert can legally target Items.Count (append).
            // The preview never sits inside the pinned region, it lands at the first slot after it.
            var insertIndex = Math.Clamp(Math.Max(PinnedCount, rawIndex < 0 ? Items.Count : rawIndex), 0, Items.Count);
            InsertPreview(insertIndex, PreviewTabItem);
        }
        else
        {
            // Move's target must be an existing index — clamp to the last valid slot.
            // The preview never sits inside the pinned region, it lands at the first slot after it.
            var moveIndex = Math.Clamp(Math.Max(PinnedCount, rawIndex < 0 ? Items.Count - 1 : rawIndex), 0, Items.Count - 1);
            if (currentIndex != moveIndex)
                Move(currentIndex, moveIndex, overridePinned: true);
        }
    }

    /// <summary>
    ///  Hides the docking preview.
    /// </summary>
    void IDockingControl.HideDockingPreview(DraggingOutTabArgs args)
    {
        if (PreviewTabItem is null) return;
        // Safe add but this should always contain
        if (Items.Contains(PreviewTabItem)) Items.Remove(PreviewTabItem);
        PreviewTabItem = null;
    }
    
    
    /// <summary>
    /// Special function to inset so we do not select the preview item.
    /// </summary>
    protected void InsertPreview(int index, TabItem itemToInset)
    {
        var previousSelected = SelectedItem;
        Console.WriteLine(index);
        Items.Insert(index, itemToInset);
        SelectedItem = previousSelected;
    }
}