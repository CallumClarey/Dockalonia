using System.Globalization;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace DockaloniaDockspace;

public class SplitContainer : TemplatedControl
{

    // Grid that controls the split container 
    private Grid _mainGrid = null!;

    private ContentPresenter? _firstPresenter;
    private ContentPresenter? _secondPresenter;
    private Thumb? _splitterThumb;

    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<SplitContainer, Orientation>(nameof(Orientation), Orientation.Horizontal);

    public static readonly StyledProperty<double> RatioProperty =
        AvaloniaProperty.Register<SplitContainer, double>(nameof(Ratio), 0.5);

    public static readonly StyledProperty<double> MinClampProperty =
        AvaloniaProperty.Register<SplitContainer, double>(nameof(MinClamp), 0.05);
    
    public static readonly StyledProperty<double> MaxClampProperty =
        AvaloniaProperty.Register<SplitContainer, double>(nameof(MaxClamp), 0.95);
    
    public static readonly DirectProperty<SplitContainer, object?> FirstContentProperty =
        AvaloniaProperty.RegisterDirect<SplitContainer, object?>(nameof(FirstContent), o => o.FirstContent, (o, v) => o.FirstContent = v);

    public static readonly DirectProperty<SplitContainer, object?> SecondContentProperty =
        AvaloniaProperty.RegisterDirect<SplitContainer, object?>(nameof(SecondContent), o => o.SecondContent, (o, v) => o.SecondContent = v);

    
    /// <summary>
    /// The two panes of the split, first then second. A split is strictly binary, so
    /// adding a third pane throws.
    /// </summary>
    [Content]
    public AvaloniaList<object> Panes { get; } = [];

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double Ratio
    {
        get => GetValue(RatioProperty);
        set => SetValue(RatioProperty, value);
    }
    
    public double MinClamp
    {
        get => GetValue(MinClampProperty);
        set => SetValue(MinClampProperty, value);
    }

    public double MaxClamp
    {
        get => GetValue(MaxClampProperty);
        set => SetValue(MaxClampProperty, value);
    }
    
    public object? FirstContent
    {
        get => field;
        private set => SetAndRaise(FirstContentProperty, ref field, value);
    }

    public object? SecondContent
    {
        get => field;
        private set => SetAndRaise(SecondContentProperty, ref field, value);
    }

    public bool IsHorizontal => Orientation == Orientation.Horizontal;
    
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OrientationProperty || change.Property == RatioProperty)
            UpdateLayoutMetrics();
    }

    public SplitContainer()
    {
        // Pure C# Control Template 
        Template = CreateTemplate();

        // A split is strictly binary: reject a third pane loudly, before it is added.
        Panes.Validate = _ =>
        {
            if (Panes.Count >= 2)
                throw new InvalidOperationException(
                    "A SplitContainer holds exactly two panes. Nest another SplitContainer to split further.");
        };

        Panes.CollectionChanged += (_, _) =>
        {
            FirstContent = Panes.Count > 0 ? Panes[0] : null;
            SecondContent = Panes.Count > 1 ? Panes[1] : null;
        };
    }

    private FuncControlTemplate<SplitContainer> CreateTemplate() 
        => new ((control, scope) => 
        {
            // Create the grid that holds the two panes and the splitter
            control._mainGrid = new Grid();
            
            control._firstPresenter = new ContentPresenter();
            control._firstPresenter.Bind(
                ContentPresenter.ContentProperty, control.GetObservable(FirstContentProperty));

            control._secondPresenter = new ContentPresenter();
            control._secondPresenter.Bind(
                ContentPresenter.ContentProperty, control.GetObservable(SecondContentProperty));
            
            
            // A bare Thumb has no default template in the Fluent theme, so it renders
            // nothing and never receives pointer input. Give it a simple template that
            // paints its background; otherwise the splitter is invisible and un-draggable.
            var splitterBrush = new SolidColorBrush(Color.Parse("#333333"));
            control._splitterThumb = new Thumb
            {
                Background = splitterBrush,
                Template = new FuncControlTemplate<Thumb>((_, _) =>
                    new Border { Background = splitterBrush })
            };
            control._splitterThumb.DragDelta += control.OnSplitterDragDelta;

            // Add the first pane straight into the main grid
            InjectSplitPanelContent(control._mainGrid, control._firstPresenter);

            // Add splitter
            control._mainGrid.Children.Add(control._splitterThumb);

            // Add the second pane
            InjectSplitPanelContent(control._mainGrid, control._secondPresenter);
            
            control.UpdateLayoutMetrics();
            return control._mainGrid;
        });


    // Function to inject content set on either side 
    protected virtual void InjectSplitPanelContent(Grid parent, ContentPresenter content)
        => parent.Children.Add(content);
    
    
    private void UpdateLayoutMetrics()
    {
        // If any of these are null some bookie shit is happening
        if (_firstPresenter is null || _splitterThumb is null || _secondPresenter is null)
            return;
        
        // Work out percentage multipliers
        var r1 = Ratio;
        var r2 = 1.0 - Ratio;
        
        // Set the row and column defs
        var columnDefs = IsHorizontal ? $"{r1}*,Auto,{r2}*" : "*";
        var rowDefs = IsHorizontal ? "*" : $"{r1}*,Auto,{r2}*";
        _mainGrid.ColumnDefinitions = new ColumnDefinitions(columnDefs);
        _mainGrid.RowDefinitions = new RowDefinitions(rowDefs);
        
        // Set the thumb widths
        // TODO: Make this a a styleable thing in a custom splitter
        _splitterThumb.Width = IsHorizontal ? 6 : double.NaN;
        _splitterThumb.Height = IsHorizontal ? double.NaN : 6;
        var cursorType = IsHorizontal ? StandardCursorType.SizeWestEast : StandardCursorType.SizeNorthSouth;
        _splitterThumb.Cursor = new Cursor(cursorType);
        
        // Position the two panes and the splitter within the main grid.
        RowColumnToggle(_firstPresenter,0);
        RowColumnToggle(_splitterThumb,1);
        RowColumnToggle(_secondPresenter,2);
    }

    /// <summary>
    /// Helper function to toggle column rows.
    /// </summary>
    /// <param name="presenter"></param>
    /// <param name="column"></param>
    private void RowColumnToggle<T>(T presenter,int column) where T : Control
    {
        var colVal = IsHorizontal ? column : 0;
        var rowVal = IsHorizontal ? 0 : column;
        Grid.SetColumn(presenter, colVal); 
        Grid.SetRow(presenter, rowVal);
    }
    
    
    private void OnSplitterDragDelta(object? sender, VectorEventArgs e)
    {
        var totalSize = IsHorizontal ? Bounds.Width : Bounds.Height;
        if (totalSize <= 0) return;
        var delta = IsHorizontal ? e.Vector.X : e.Vector.Y;
        Ratio = Math.Clamp(Ratio + (delta / totalSize), MinClamp, MaxClamp);
    }
}