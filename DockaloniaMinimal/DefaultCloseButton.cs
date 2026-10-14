using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace DockaloniaMinimal;


/// <summary>
/// The internal default standard for close buttons within the API.
/// Cannot be inherited from if the user wants a close button they must provide their own.
/// Styled like a textblock.
/// </summary>
internal sealed class DefaultCloseButton : Button
{
    // Style override so it can use text box styling
    protected override Type StyleKeyOverride => typeof(TextBlock);
    
    // Backing rendering fields
    private Geometry? _crossGeometry;
    private Geometry? _squareGeometry;

    
    // Will measure the size of the element based on font size instead of other visual elements
    protected override Size MeasureOverride(Size availableSize) => new Size(FontSize, FontSize);

    public DefaultCloseButton()
    {
        FontSize = 20;
    }
    
    
    // Lists for the property to change 
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        // If the property is not the font size ignore it not relevant to our logic
        if (change.Property != FontSizeProperty) return;
        
        // Calculate the radius of the surrounding circle 
        var radius = FontSize / 2;
        
        // Create new ring circle
        _squareGeometry = new RectangleGeometry(
            new Rect(-radius, -radius, radius * 2, radius * 2));
        
        // Create the cross
        _crossGeometry = CreateFilledCross(radius * 1.1f, thickness: 2);
    }

    
    /// <summary>
    ///     Will draw the cross for the x using the radius of circle as a refence point.
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="thickness"></param>
    /// <returns></returns>
    private Geometry CreateFilledCross(double radius, double thickness)
    {
        List<Point> points = [];

        for (var i = 0; i < 4; i++)
        {
            var mtx = Matrix.CreateRotation((Math.Tau * 0.25) * (i + 0.5));

            points.Add(new Point(-thickness / 2, -radius).Transform(mtx));
            points.Add(new Point(thickness / 2, -radius).Transform(mtx));
            points.Add(new Point(thickness / 2, -thickness / 2).Transform(mtx));
        }

        return new PolylineGeometry(points, true);
    }

    
    // Invalid visuals on the change ever of the pointer events to change on hover
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        InvalidateVisual();
    }
    
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        InvalidateVisual();
    }
    
    
    /// <summary>
    /// Function used when render is called on the object at runtime.
    /// </summary>
    /// <param name="context"></param>
    public override void Render(DrawingContext context)
    {
        var center = Bounds.WithX(0).WithY(0).Center;

        // Push the items to the render
        // Need using so that dispose is auto called on the objects when out of scope
        using (context.PushTransform(Matrix.CreateTranslation(center)))
        using (context.PushOpacity(IsPointerOver ? 0.2 : 0.0))
        {
            context.DrawGeometry(Foreground, null, _squareGeometry!);
            using (context.PushOpacity(IsPointerOver ? 1 : 0.6))
                context.DrawGeometry(Foreground, null, _crossGeometry!);

        }
    }
}