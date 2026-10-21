using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using DockaloniaMinimal.Docking;

namespace DockaloniaMinimal.Tabbing.TabControls;


/// <summary>
/// Base tab control class that all tab controls in the library are parented to.
/// Extends some of the functionality of a normal <see cref="TabControl"/>.
/// Adding extra helper functions such as move and index over pointer, and introduces tab pinning.
/// Any static extension methods would ideally be applied to this class as every tab control inherits from it.
/// </summary>
public class DockaloniaTabControl : TabControl
{
    // Will style like a normal tab control
    protected override Type StyleKeyOverride => typeof(TabControl);
    
    // Manually trigger to select in on pointer press instead
    protected override bool ShouldTriggerSelection(Visual selectable, PointerEventArgs eventArgs) => false;
    
    /// <summary>
    /// List of all pinned tabs in the tab control
    /// </summary>
    public readonly List<DockaloniaTab> PinnedItems = [];
    
    /// <summary>
    ///  Exposes the <see cref="ItemsPresenter"/> to the user.
    /// </summary>
    protected ItemsPresenter? ExposedItemsPresenterPart;
    
    
    /// <summary>
    /// Exposed the <see cref="ContentPresenter"/> to the user.
    /// </summary>
    protected ContentPresenter? ExposedContentPresenterPart;
    
    
    /// <summary>
    ///  Wrapper function to check for empty tab control
    /// </summary>
    /// <returns>A boolean value for if the list of items is empty or not</returns>
    public bool IsEmpty => Items.Count <= 0;
    
    
    /// <summary>
    ///  Wrapper function to add a <see cref="DockaloniaTab"/>.
    ///  Not Intended for MVVM use.
    /// </summary>
    /// <param name="tabItem"></param>
    public void AddTab(DockaloniaTab tabItem) => Items.Add(tabItem);

    
    /// <summary>
    ///  Wrapper function to add a <see cref="DockaloniaTab"/>.
    ///  If the index passed in is not valid it will just append the tab.
    ///  Not Intended for MVVM use.
    /// </summary>
    /// <param name="tabItem">The tab item to add.</param>
    /// <param name="index">The index of the tab Item</param>
    public void AddTab(DockaloniaTab tabItem, int index) 
        => Items.Insert(index >= Items.Count || index <= 0 ? index : Items.Count - 1, tabItem);
    
    
    /// <summary>
    ///  Wrapper function to remove <see cref="DockaloniaTab"/>.
    ///  Not Intended for MVVM use.
    /// </summary>
    /// <param name="tabItem"></param>
    public void RemoveTab(DockaloniaTab tabItem) => Items.Remove(tabItem);
    
    
    /// <summary>
    ///  Wrapper function to remove <see cref="DockaloniaTab"/>.
    ///  Not Intended for MVVM use.
    /// </summary>
    /// <param name="index"></param>
    public bool RemoveTab(int index)
    { 
        if (index <= 0 && index > Items.Count -1 ) return false;
        Items.RemoveAt(index);
        return true;
    } 
    
    
    
    
    /// <summary>
    ///  Function called when two tabs places are swap using the <see cref="Move"/> function.
    /// </summary>
    protected virtual void OnTabsMoved(int target, int victim){}
    
    
    /// <summary>
    ///  Returns the <see cref="TabItem"/> at the index or the container.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public DockaloniaTab? GetTabItemAtIndex(int index) 
        => (index < 0 || index >= Items.Count) ? null : Items[index] as DockaloniaTab;
    
    
    /// <summary>
    ///  Helper function to swap the positon of two existing tabs within the list.  
    /// </summary>
    /// <param name="oldIndex"> The first tab index.</param>
    /// <param name="newIndex"> Second Tab Index, the tab to be displaced.</param>
    public bool Move(int oldIndex, int newIndex)
    {
        // Nothing to do.
        if (oldIndex == newIndex) return true;
        
        // Use the bound collection if one exists
        var list = ItemsSource is null ? Items : ItemsSource as IList;

        // Ensure the collection supports modification.
        if (list is null || list.IsReadOnly || list.IsFixedSize)
            return false;

        // Validate indexes before accessing the collection.
        if (oldIndex < 0 || oldIndex >= list.Count ||
            newIndex < 0 || newIndex >= list.Count)
            return false;
        
        // Save the item before removing it.
        var item = list[oldIndex];

        // Move it to the requested position.
        list.RemoveAt(oldIndex);
        list.Insert(newIndex, item);

        OnTabsMoved(oldIndex, newIndex);
        return true;
    }
    
    
    /// <summary>
    ///    Will return a tab item under the position,
    ///    Typically Used to hit Test under the mouse.
    /// </summary>
    /// <param name="hitPoint"> The point you wish to perform the hit test.</param>
    /// <param name="tabItem"> The tab item that has been hit.</param>
    /// <param name="index"> The index that has been hit.</param>
    /// <returns> Whether a tab has been hit. </returns>
    public bool GetTabItemAt(Point hitPoint, out TabItem? tabItem, out int index)
    {
        // Set default values
        tabItem = null;
        index = -1;

        // Search from front to back in reverse order.
        for (var i = Items.Count - 1; i >= 0; i--)
        {
            if (Items[i] is not TabItem tab) continue;

            var bounds = this.GetBoundsOf(tab);
            if (!bounds.Contains(hitPoint)) continue;

            tabItem = tab;
            index = i;
            return true;
        }
        return false;
    }
    
    
    /// <summary>
    ///  We find the <see cref="ItemsPresenter"/> and <see cref="ContentPresenter"/>.
    /// This allows us to calculate content size, used throughout the library. 
    /// </summary>
    /// <param name="e"></param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ExposedItemsPresenterPart = e.NameScope.Find<ItemsPresenter>("PART_ItemsPresenter");
        ExposedContentPresenterPart = e.NameScope.Find<ContentPresenter>("PART_SelectedContentHost");
    }
    
    
    /// <summary>
    ///  Helper function to get the rectangle presenter of the tab bar.
    /// </summary>
    /// <param name="rect"> The out value of the <see cref="TabControl"/> rect. </param>
    /// <returns> Boolean value of whether it successfully got this <see cref="TabControl"/> <see cref="Rect"/>.</returns>
    protected bool TryGetTabBarRect(out Rect rect)
    {
        var tabBarPanel = ExposedItemsPresenterPart?.Panel;
        rect = tabBarPanel is null ? default : this.GetBoundsOf(tabBarPanel);
        return tabBarPanel is not null;
    }
}