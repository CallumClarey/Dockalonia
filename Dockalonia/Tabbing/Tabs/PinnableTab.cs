using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using Dockalonia.Tabbing.TabControls;

namespace Dockalonia.Tabbing.Tabs;


/// <summary>
///  Will add pinnable tab functionality to the standard <see cref="TabItem"/>.
///  Can be used by the <see cref="DockaloniaTabControl"/> to create tabs.
/// </summary>
[PseudoClasses(":pinned")]
public class PinnableTab : TabItem
{
    // Override style key to allow styling like a normal tab
    protected override Type StyleKeyOverride => typeof(TabItem);

    /// <summary>
    ///  Backing property for <see cref="IsPinned"/>.
    /// </summary>
    public static readonly StyledProperty<bool> IsPinnedProperty =
        AvaloniaProperty.Register<PinnableTab, bool>(nameof(IsPinned));

    /// <summary>
    ///  Backing property for <see cref="IsPinnable"/>.
    /// </summary>
    public static readonly StyledProperty<bool> IsPinnableProperty =
        AvaloniaProperty.Register<PinnableTab, bool>(nameof(IsPinnable), true);

    /// <summary>
    ///  Raised when the tab becomes pinned.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> TabPinnedEvent =
        RoutedEvent.Register<PinnableTab, RoutedEventArgs>("TabPinned", RoutingStrategies.Bubble);

    /// <summary>
    ///  Raised when the tab is no longer pinned.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> TabUnpinnedEvent =
        RoutedEvent.Register<PinnableTab, RoutedEventArgs>("TabUnpinned", RoutingStrategies.Bubble);

    /// <summary>
    /// Registers the class handler that reacts to changes of <see cref="IsPinned"/>.
    /// </summary>
    static PinnableTab()
    {
        // Handle property changes safely (works with XAML styles & bindings)
        IsPinnedProperty.Changed.AddClassHandler<PinnableTab>((tab, e) => tab.OnPinnedChanged(e.GetNewValue<bool>()));
    }
    
    /// <summary>
    ///  If set to <see langword="true"/> the tab is pinned and is anchored to the start of the tab strip.
    /// </summary>
    public bool IsPinned
    {
        get => GetValue(IsPinnedProperty);
        set => SetValue(IsPinnedProperty, value);
    }
    
    /// <summary>
    ///  If set to <see langword="false"/> the user cannot pin the tab.
    ///  The tab will still honour <see cref="IsPinned"/> if it is set programmatically.
    /// </summary>
    public bool IsPinnable
    {
        get => GetValue(IsPinnableProperty);
        set => SetValue(IsPinnableProperty, value);
    }
    
    /// <summary>
    ///  Fired when the tab becomes pinned.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? TabPinned
    {
        add => AddHandler(TabPinnedEvent, value);
        remove => RemoveHandler(TabPinnedEvent, value);
    }
    
    /// <summary>
    ///  Fired when the tab is no longer pinned.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? TabUnpinned
    {
        add => AddHandler(TabUnpinnedEvent, value);
        remove => RemoveHandler(TabUnpinnedEvent, value);
    }
    
    /// <summary>
    ///  Flips the pinned state of the tab.
    /// </summary>
    public void PinTab(bool value) => IsPinned = value;

    
    /// <summary>
    ///  Called whenever the <see cref="IsPinned"/> state changes.
    ///  Updates the pseudo classes and raises the matching routed event.
    /// </summary>
    /// <param name="isPinned">The new pinned state.</param>
    protected virtual void OnPinnedChanged(bool isPinned)
    {
        // Expose the state to the styling layer
        this.SetPseudoClass(":pinned", isPinned);
        // Let the parent tab control know the tab has changed state
        RaiseEvent(new RoutedEventArgs(isPinned ? TabPinnedEvent : TabUnpinnedEvent));
    }
}
