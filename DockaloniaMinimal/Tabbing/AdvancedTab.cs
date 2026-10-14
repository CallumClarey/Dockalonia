using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace DockaloniaMinimal.Tabbing;

/// <summary>
/// Class used to represent a tab that can be detached from a dockspace.
/// </summary>
public class AdvancedTab : TabItem
{
    // Override style key to allow styling like normal tab
    protected override Type StyleKeyOverride => typeof(TabItem);
    
    /// <summary>
    /// Routed event that will globally fire to check when tabs are closed.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> TabClosedEvent =
        RoutedEvent.Register<AdvancedTab, RoutedEventArgs>("TabClosed", RoutingStrategies.Direct);
    
    /// <summary>
    ///  Tab closed event. 
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Closed
    {
        add => AddHandler(TabClosedEvent, value);
        remove => RemoveHandler(TabClosedEvent, value); 
    }
    
    /// <summary>
    /// Fired before the <see cref="AdvancedTab"/> is closed.
    /// </summary>
    public event EventHandler<CancelEventArgs>? Closing;
    

    //Pinning test code
    
    
    public static readonly StyledProperty<bool> IsPinnedProperty =
        AvaloniaProperty.Register<AdvancedTab, bool>(nameof(IsPinned));
    

    public static readonly RoutedEvent<RoutedEventArgs> TabPinnedEvent =
        RoutedEvent.Register<AdvancedTab, RoutedEventArgs>("TabClosed", RoutingStrategies.Direct);

    
    static AdvancedTab()
    {
        // Handle property changes safely (works with XAML styles & bindings)
        IsPinnedProperty.Changed.AddClassHandler<AdvancedTab>((tab, e) =>
        {
            tab.RaiseEvent(new RoutedEventArgs(TabPinnedEvent));
        });
    }

    // 3. Keep C# property wrappers standard (GetValue / SetValue only)
    public bool IsPinned
    {
        get => GetValue(IsPinnedProperty);
        set => SetValue(IsPinnedProperty, value);
    }

    // 4. CLR Event wrapper
    public event EventHandler<RoutedEventArgs>? TabPinned
    {
        add => AddHandler(TabPinnedEvent, value);
        remove => RemoveHandler(TabPinnedEvent, value);
    }
    

    /// <summary>
    ///  The close button that the tab will use instead of the default,
    ///  If none is provided it reverts back to default.
    /// </summary>
    public Button? TabCloseBtn;
    
    
    /// <summary>
    ///  If set to <see langword="true"/> the tab will be removed from its parent control when closed.
    ///  A good example of when you not want this to happen is when something is not saved.
    ///  Not actually sure if this needs to even be in here. (I don't think you would ever use it)
    /// </summary>
    public bool RemoveOnClosed { get; set; } = true;

    
    /// <summary>
    /// Defines AXAML Property for the <see cref="RemoveOnClosed"/>
    /// </summary>
    public static readonly DirectProperty<AdvancedTab, bool> RemoveOnClosedProperty = 
        AvaloniaProperty.RegisterDirect<AdvancedTab, bool>(nameof(RemoveOnClosed), 
            o => o.RemoveOnClosed,
            (o, v) => o.RemoveOnClosed = v);

    
    /*
     *  These will be used to cache all the built templates for the tab header.
     *  E.G the little unsaved tab icon you find on applications.
     */
    private readonly Dictionary<IDataTemplate, IDataTemplate?> _headerTemplateCache = [];
    private readonly IDataTemplate? _defaultHeaderTemplate;
    public AdvancedTab()
    {
        _defaultHeaderTemplate = CreateHeaderWithCloseButton(null);
        UpdateHeaderTemplate(null);
    }


    /// <summary>
    /// Invoke the closing event before the tab is closed.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnClosing(CancelEventArgs e) => Closing?.Invoke(this,e);

    // Close the tab
    public void Close()
    {
        // Invoke the closing 
        var cancelEvent = new CancelEventArgs();
        OnClosing(cancelEvent);
        
        if(cancelEvent.Cancel) return;
        if(RemoveOnClosed) RemoveFromParentTabControl();

        Content = "<This tab has been closed>";
        // Send the event through globally to say the tab has been closed
        RaiseEvent(new RoutedEventArgs(TabClosedEvent));
    }

    /// <summary>
    /// Remove the item from the parent tab control.
    /// </summary>
    private void RemoveFromParentTabControl()
    {
        if(Parent is not TabControl tabControl) return;
        tabControl.Items.Remove(this);
    }
    
    
    /*
     * This function will check for the header template change and will then rebuild it if not already cached.
     * The header template property is inherited from the base Tab item class.
     */
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
       
        // Check the header template property is the changed one
        if (change.Property == HeaderTemplateProperty)
            UpdateHeaderTemplate(HeaderTemplate);
    }

    
    /// <summary>
    /// Take a <see cref="IDataTemplate"/> of the new header template to add to the table.
    /// Checks against the tabs cache to see if it has already been built.
    /// If not it builds the rebuilds the tab with the new template.
    /// </summary>
    /// <param name="dataTemplate"></param>
    private void UpdateHeaderTemplate(IDataTemplate? dataTemplate)
    {
        // May Need a recursion check here
        
        // Check to see if we have a valid template 
        if (dataTemplate is null)
        {
            HeaderTemplate = _defaultHeaderTemplate;
            return;
        }
        
        // If it is not contained within cache create and add it
        if(!_headerTemplateCache.TryGetValue(dataTemplate, out var dataTemplateCache)) 
            _headerTemplateCache[dataTemplate] = CreateHeaderWithCloseButton(dataTemplate);
        else HeaderTemplate = dataTemplateCache;
    }

    
    /// <summary>
    /// Creates a header template with the close button provided by the user.
    /// If there is no provided close button it will use the default internal one.
    /// </summary>
    /// <param name="innerTemplate"> Takes visual element that implements <see cref="IDataTemplate"/>.</param>
    /// <returns> Returns a backing stack panel.</returns>
    private IDataTemplate CreateHeaderWithCloseButton(IDataTemplate? innerTemplate) 
        => new FuncDataTemplate<object?>((content, _) => {
            // Create stack panel to add header
            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 8
            };
            
            // Declare output
            Control? output = null;
            
            // Check if the inner template is not null
            // Then checks if this template supports rendering then builds the visual Ui Output
            if (innerTemplate != null && innerTemplate.Match(content))
                output = innerTemplate.Build(content);
            
            // Checks there is a visual element to add if it exists add it 
            if(output != null) panel.Children.Add(output);
            else
            {
                // Build a content presenter to display if the visual could not be built
                panel.Children.Add(new ContentPresenter
                {
                    Content = content,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }
            
            // Add close button for template and attach buttons
            var closeBtn = TabCloseBtn ?? new DefaultCloseButton();
            closeBtn.Click += (_, __) => Close();
            panel.Children.Add(closeBtn);
            
            // Return header stack panel
            return panel;
        });




}