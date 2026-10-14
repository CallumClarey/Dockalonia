using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using DockaloniaMinimal.Previews;
using DockaloniaMinimal.TabDragging;

namespace DockaloniaMinimal.Tabbing;


/// <summary>
///   
/// </summary>
public class RearrangeTabControl : TabControl
{
    // Allows for this to use all of tab controls styles
    protected override Type StyleKeyOverride => typeof(TabControl);
    
    // Manually trigger to select in on pointer press instead
    protected override bool ShouldTriggerSelection(Visual selectable, PointerEventArgs eventArgs) => false;
    
    // Tuple value for the dragged tab
    protected (TabItem tabItem, Point offset)? _draggedTab = null;
    
    protected ItemsPresenter? _itemsPresenterPart;
    protected ContentPresenter? _contentPresenterPart;

    // Tab control drag handler 
    protected ITabDragHandler DragHandler = new SmoothDragHandler();
    
    // Class constructor
    public RearrangeTabControl()
    { 
        IsHitTestVisible = true;
        Padding = new Thickness(0);
    }

    // This gets items we need for calculations 
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _itemsPresenterPart = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
        _contentPresenterPart = e.NameScope.Find<ContentPresenter>("PART_SelectedContentHost");
    }
    
    
    /// <summary>
    /// Handles behavior for getting the tab on the click.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        var hitPoint = e.GetPosition(this);
        
        // Return if it is not a tab item
        if(!GetTabItemAt(hitPoint, out var tabItem, out _)) return;
        
        // Only select visible tabs
        if(tabItem!.IsVisible) SelectedItem = tabItem;
        
        var topLeft = tabItem.TranslatePoint(new Point(0, 0), this)!.Value;
        _draggedTab = (tabItem, topLeft);
        DragHandler.OnDragStarted(e ,_draggedTab.Value, this);
    }
    
    /// <summary>
    ///  Releases the dragged tab on pointer release.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if(_draggedTab is not null) DragHandler.OnDragCompleted(_draggedTab.Value,this);
        _draggedTab = null;
    }
    
    
    /// <summary>
    ///  When user drags the tab this function will be called to handle rearranging.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_draggedTab == null) return;
    
        // Call function to rearrange tab rearrange the tab 
        DragHandler.HandleTabDragging(e, _draggedTab!.Value, this);
    }
    
    
    /// <summary>
    ///  Returns the <see cref="TabItem"/> at the index or the container.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public TabItem? GetTabItemAtIndex(int index)
    {
        if (index < 0 || index >= Items.Count) 
            return null;

        // 1. If Items contains TabItem objects directly
        if (Items[index] is TabItem tabItem)
            return tabItem;

        return null;
    }


    /// <summary>
    ///     Will return a tab item under the position.
    /// </summary>
    /// <param name="hitPoint"></param>
    /// <param name="tabItem"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public bool GetTabItemAt(Point hitPoint, out TabItem? tabItem, out int index)
    {
        // Default index to null
        index = -1;
        // Get the list of tabs
        var tabs = Items.OfType<TabItem>().ToList();

        for (var i = tabs.Count - 1; i >= 0; i--)
        {
            if (!this.GetBoundsOf(tabs[i]).Contains(hitPoint)) continue;
            tabItem = tabs[i];
            index = Items.IndexOf(tabItem);
            return true;
        }
        
        // Set tab item to null
        tabItem = null;
        return false;
    }
    
    /// <summary>
    /// Gets the tab index at the position of point.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public int GetTabIndexAt(Point position)
    {
        if (Items.Count == 0) return -1;

        if (GetTabItemAt(position, out var tabItem, out var index))
        {
            Console.WriteLine($"Current tab item,: {tabItem}");
            return index;
        }

        if (ContainerFromIndex(Items.Count - 1) is not { } lastContainer) return -1;
        var lastBounds = this.GetBoundsOf(lastContainer);

        var pastLastTab = position.X > lastBounds.Right
                          && position.Y >= lastBounds.Top
                          && position.Y <= lastBounds.Bottom;
        
        return pastLastTab ? Items.Count : -1;
    }
    

    /// <summary>
    /// Function will swap the position of two tabs. 
    /// </summary>
    /// <param name="from"> The first tab index.</param>
    /// <param name="to"> Second Tab Index</param>
    public void Move(int from, int to)
    {
        var source = ItemsSource;
        var list = source is null ? Items : source as IList;
 
        if (list is null || list.IsReadOnly) return;
 
        var item = list[from];
 
        // ObservableCollection<T>.Move keeps item identity; otherwise remove and re-insert.
        var move = list.GetType().GetMethod("Move", [typeof(int), typeof(int)]);
        if (move is not null) move.Invoke(list, [from, to]); 
        else
        {
            list.RemoveAt(from);
            // Clamp to the last item in the list 
            var clampedTo = Math.Clamp(to, 0, list.Count); 
            list.Insert(clampedTo, item);
        }
        
        // Run animation hook function
        DragHandler.OnTabsSwapped(GetTabItemAtIndex(from)!, from, to, this);
    }

    /// <summary>
    /// Special function to inset so we do not select the preview item.
    /// </summary>
    protected void InsertPreview(int index, TabItem itemToInset)
    {
        var previousSelected = SelectedItem;
        Items.Insert(index, itemToInset);
        SelectedItem = previousSelected;
    }
    
}  